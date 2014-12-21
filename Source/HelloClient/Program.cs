using System;
using System.Text;
using Core.Extensions;
using RabbitMQ.Client;

namespace HelloClient
{
    internal class Program
    {
        private const string HostName = "localhost";
        private const string Password = "guest";
        private const string UserName = "guest";
        private const string QueueName = "HelloQueue";
        private const string ExchangeName = "HelloExchange";

        private static void Main()
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = HostName,
                Password = Password,
                UserName = UserName
            };

            var connection = connectionFactory.CreateConnection();
            IModel model = connection.CreateModel();

            model.CreateDurableQueue(QueueName);
            model.CreateDurableDirectExcange(ExchangeName);
            model.QueueBind(QueueName, ExchangeName, string.Empty);

            var properties = model.CreateBasicProperties();
            properties.SetPersistent(true);

            var messageData = Encoding.Default.GetBytes("Test message");

            model.BasicPublish(ExchangeName, string.Empty, properties, messageData);

            Console.WriteLine("Done");
        }
    }
}
