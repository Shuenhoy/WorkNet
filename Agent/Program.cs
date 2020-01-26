using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WorkNet.Agent.Worker;
using WorkNet.Common;
using WorkNet.Common.Models;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;

namespace WorkNet.Agent
{
    class Program
    {
        public static void Main()
        {

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<Program>();
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
                var worker = new DockerWorker(channel, logger);

                channel.QueueDeclare(queue: "task_queue",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                logger.LogInformation("Waiting for tasks");
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.Deserialize<TaskGroup>();


                    try
                    {
                        worker.ExecTaskGroup(body, ea).Wait();
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        logger.LogInformation("task finished");

                    }
                    catch (AggregateException ae)
                    {
                        var error = "";
                        foreach (var e in ae.InnerExceptions)
                        {

                            error += e.ToString();
                        }
                        logger.LogError("task failed.\n" + error);
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }






                };
                channel.BasicConsume(queue: "task_queue",
                                     autoAck: false,
                                     consumer: consumer);

                Console.WriteLine(" Press [^C] to exit.");
                var exitEvent = new ManualResetEvent(false);

                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    eventArgs.Cancel = true;
                    exitEvent.Set();
                };
                exitEvent.WaitOne();
            }
        }
    }
}