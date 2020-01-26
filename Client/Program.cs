using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using WorkNet.Common.Models;
using WorkNet.Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading;

namespace WorkNet.Client
{
    [Verb("add", HelpText = "Add several single tasks")]
    public class AddOptions
    {
        [Value(0)]
        public IEnumerable<string> Commands { get; set; }
    }
    [Verb("submit", HelpText = "Submit to the server")]
    public class SubmitOptions
    {
        [Option('r', "rezip", Default = false, HelpText = "rezip the executor files")]
        public bool ReZip { get; set; }
    }
    [Verb("run", HelpText = "Add tasks and submit to the server")]
    public class RunOptions
    {
        [Option('r', "rezip", Default = false, HelpText = "rezip the executor files")]
        public bool ReZip { get; set; }
        [Option('i', "image", Required = false, Default = "ubuntu:18.04", HelpText = "Set the runtime image")]
        public string Image { get; set; }
        [Value(0, Min = 1)]
        public IEnumerable<string> Commands { get; set; }
    }
    [Verb("init", HelpText = "Create a task")]
    public class InitOptions
    {
        [Option('f', "file", Required = false, Default = "WNfile.json", HelpText = "Set the configuration file")]
        public string File { get; set; }
        [Option('i', "image", Required = false, Default = "ubuntu:18.04", HelpText = "Set the runtime image")]
        public string Image { get; set; }
        [Value(0, Min = 1)]
        public IEnumerable<string> Commands { get; set; }
    }

    [Verb("pull", HelpText = "Pull Results")]
    public class PullOptions
    {
        [Option('i', "id", HelpText = "Set the id to pull")]
        public int? Id { get; set; }
    }
    class Program
    {


        static int Main(string[] args)
        {
            var factory = new ConnectionFactory()
            {
                HostName = AppConfigurationServices.RabbitMQ,
                Port = AppConfigurationServices.RabbitMQPort,
                UserName = AppConfigurationServices.RabbitMQUsername,
                Password = AppConfigurationServices.RabbitMQPassword
            };


            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var xargs = new Dictionary<string, object>();
                xargs.Add("x-message-ttl", 1000 * 60 * 60 * 24 * 3);
                var rev = channel.QueueDeclare(durable: false, exclusive: false, autoDelete: false, arguments: xargs).QueueName;
                channel.QueueDeclare(queue: "task_queue",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                channel.BasicPublish(exchange: "", routingKey: "task_queue", basicProperties: properties, body: new WorkNet.Common.Models.TaskGroup()
                {

                    subtasks = new List<AtomicTask>(new[] { new AtomicTask(){

                    } }
                    ),
                    files = new List<(string, FileGetter)>(new (string, FileGetter)[] { }),
                    executor = new Executor()
                    {
                        worker = null,
                        source = @"
                            local stdout, stderr, exitCode = docker_arr('alpine', {'sh','-c','echo hello'})
                            return {stdout=stdout, stderr=stderr,exitCode=exitCode}
                        "
                    },
                    parameters = new Dictionary<string, object>()
                }.SerializeToByteArray());
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.Deserialize<Dictionary<string, object>>();
                    foreach (var (k, v) in body)
                    {
                        Console.WriteLine($"{k} = {v}");
                    }
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                };
                channel.BasicConsume(queue: rev, autoAck: false, consumer: consumer);
                Console.WriteLine(" Press [^C] to exit.");
                var exitEvent = new ManualResetEvent(false);

                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    eventArgs.Cancel = true;
                    exitEvent.Set();
                };
                exitEvent.WaitOne();
            }

            return 0;
            // return CommandLine.Parser.Default.ParseArguments<AddOptions, SubmitOptions, InitOptions, PullOptions, RunOptions>(args)
            //   .MapResult(
            //     (InitOptions opts) => Executor.Init(opts),
            //     (AddOptions opts) => Executor.Add(opts),
            //     (SubmitOptions opts) => Executor.Submit(opts),
            //     (PullOptions opts) => Executor.Pull(opts),
            //     (RunOptions opts) => Executor.Run(opts),

            // errs => 1);
        }
    }
}
