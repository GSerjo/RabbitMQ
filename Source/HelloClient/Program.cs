using System;
using System.Text;
using Core.Extensions;
using Nelibur.Sword.Extensions;
using RabbitMQ.Client;

namespace HelloClient
{
    internal class Program
    {
        private const string ExchangeName = "HelloExchange";
        private const string HostName = "localhost";
        private const string Password = "guest";
        private const string QueueName = "HelloQueue";
        private const string UserName = "guest";

        private static void Main()
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = HostName,
                Password = Password,
                UserName = UserName
            };

            IConnection connection = connectionFactory.CreateConnection();
            IModel model = connection.CreateModel();

            model.CreateDurableQueue(QueueName);
            model.CreateDurableDirectExcange(ExchangeName);
            model.QueueBind(QueueName, ExchangeName, string.Empty);

            while (true)
            {
                Console.WriteLine("Press ESC to exit or Any Key to send a message");

                ConsoleKeyInfo key = Console.ReadKey();
                if (key.Key == ConsoleKey.Escape)
                {
                    break;
                }
                10.Times().Iter(x => SendMessage(model, Guid.NewGuid().ToString()));
            }

            model.Close();
        }

        private static void SendMessage(IModel model, string message)
        {
            IBasicProperties properties = model.CreateBasicProperties();
            properties.SetPersistent(true);
            byte[] messageData = Encoding.Default.GetBytes(message);
            model.BasicPublish(ExchangeName, string.Empty, properties, messageData);
            Console.WriteLine("Message {0}, was sent", message);
        }
    }
}
