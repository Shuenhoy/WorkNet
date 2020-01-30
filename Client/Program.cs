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
    [Verb("run", HelpText = "Add tasks and submit to the server")]
    public class RunOptions
    {
        [Option('i', "id", Default = "", HelpText = "task group id")]
        public string Id { get; set; }
        [Option('r', "rezip", Default = false, HelpText = "rezip the executor files")]
        public bool ReZip { get; set; }

        [Option('e', "executor", Default = "./wn_executor.lua", HelpText = "executor script")]
        public string Executor { get; set; }

        [Value(0, Min = 1)]
        public IEnumerable<string> Commands { get; set; }
    }

    [Verb("pull", HelpText = "Pull Results")]
    public class PullOptions
    {
        [Value(0)]
        public string Id { get; set; }
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

                channel.QueueDeclare(queue: "task_queue",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);



                return CommandLine.Parser.Default.ParseArguments<RunOptions, PullOptions>(args)
                    .MapResult(
                        (RunOptions opts) =>
                            Submit.Submitter.Submit(channel, opts,
                                new Submit.Divider.NaiveDivider() { num = (int)channel.ConsumerCount("task_queue") })
                        ,
                        (PullOptions opts) =>
                            Pull.Puller.Pull(channel, opts)
                        ,
                         Error => -1);
            }

        }
    }
}
