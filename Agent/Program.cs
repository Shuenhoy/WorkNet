using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WorkNet.Agent.Worker;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace WorkNet.Agent
{
    class Program
    {
        public static void Main()
        {

            var factory = new ConnectionFactory() { HostName = AppConfigurationServices.RabbitMQ, Port = AppConfigurationServices.RabbitMQPort, UserName = "server", Password = AppConfigurationServices.RabbitMQPassword };
            var worker = new DockerWorker();

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "task_queue",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                Console.WriteLine(" [*] Waiting for messages.");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    var id = Int32.Parse(message);
                    Console.WriteLine(" [x] Received {0}", message);

                    try
                    {
                        worker.ExecTaskGroup(id).Wait();
                        Console.WriteLine(" [x] Done");

                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                    catch (AggregateException ae)
                    {
                        var error = "";
                        foreach (var e in ae.InnerExceptions)
                        {
                            Console.WriteLine(e);
                            error += e.Message;
                        }
                        Console.WriteLine(" [x] Done");
                        worker.SetError(id, error).Wait();
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                        // channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, true);
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