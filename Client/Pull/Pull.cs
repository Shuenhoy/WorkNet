using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WorkNet.Common;
using WorkNet.Common.Models;
using System;
using System.Threading;
using System.Linq;
using System.IO;

namespace WorkNet.Client.Pull
{
    public static class Puller
    {
        public static int Pull(IModel channel, PullOptions opts)
        {

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                Console.WriteLine($"receive {ea.BasicProperties.Type} message from {opts.Id}");
                try
                {
                    var basePath = $"wn_out/{opts.Id}/{ea.BasicProperties.CorrelationId}_{ea.BasicProperties.MessageId}";
                    if (ea.BasicProperties.Type == "result")
                    {
                        Directory.CreateDirectory(basePath);
                        var result = ea.Body.Deserialize<TaskResult>();
                        foreach (var (k, v) in result.payload)
                        {
                            if (v is FilePair pr)
                            {
                                pr.file.WriteTo($"{basePath}/{pr.filename}");
                            }
                        }
                        var output = result
                            .payload
                            .Filter(kv => !(kv.Value is FilePair))
                            .ToDictionary(kv => kv.Key, kv => (kv.Value as ObjectPayload).obj).SerializeToJson();
                        File.WriteAllText($"{basePath}/wn_result.json", output);
                    }
                    else
                    {

                    }
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (AggregateException exp)
                {
                    Console.WriteLine(exp);
                }
            };
            channel.BasicConsume(queue: opts.Id,
                                 autoAck: false,
                                 consumer: consumer);

            var exitEvent = new ManualResetEvent(false);
            Console.WriteLine("Press [^C] to exit.");
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                exitEvent.Set();
            };

            exitEvent.WaitOne();
            return 0;
        }
    }
}