using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace WorkNet.Server.Services
{
    public class RabbitMQService
    {
        private IConnection connection;
        private IModel channel;
        public RabbitMQService(IServiceScopeFactory sf)
        {
            var factory = new ConnectionFactory() { HostName = "rabbitmq", UserName = "server", Password=Environment.GetEnvironmentVariable("WN_AGENT_RabbitMQPassword") };;
            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            channel.QueueDeclare(queue: "task_queue",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
        }
        public void Publish(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            channel.BasicPublish(exchange: "",
                                 routingKey: "task_queue",
                                 basicProperties: properties,
                                 body: body);
        }

        public void Publish<T>(T message)
        {
            Publish(JsonSerializer.Serialize(message));
        }
    }
}