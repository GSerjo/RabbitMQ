using System;
using Core;
using Core.Extensions;
using Core.RabbitMQClients;
using HelloContracts;
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
        private static readonly DataSerializer _dataSerializer = new DataSerializer();
        private static IProducerMQ<Data> _producer;

        private static void Main()
        {
            //            Send1();
            Send2();
        }

        private static void Send1()
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
                100000.Times().Iter(x => SendMessage(model, x));
            }
            model.Close();
        }

        private static void Send2()
        {
            RabbitMQClient rabbitClient = RabbitMQClient.Configure(x =>
            {
                x.HostName = HostName;
                x.Password = Password;
                x.UserName = UserName;
            }).Create();

            _producer = rabbitClient.GetProducer<Data>(ExchangeName, QueueName, _dataSerializer.ToProto);

            while (true)
            {
                Console.WriteLine("Press ESC to exit or Any Key to send a message");

                ConsoleKeyInfo key = Console.ReadKey();
                if (key.Key == ConsoleKey.Escape)
                {
                    break;
                }
                100000.Times().Iter(x => SendMessage(x));
            }
        }

        private static void SendMessage(int messageIndex)
        {
            _producer.EnqueuePersistent(new Data { Id = Guid.NewGuid() });
            Console.WriteLine("Message {0}, was sent", messageIndex);
        }

        private static void SendMessage(IModel model, int messageIndex)
        {
            var data = new Data
            {
                Id = Guid.NewGuid()
            };

            IBasicProperties properties = model.CreateBasicProperties();
            properties.SetPersistent(true);
            model.BasicPublish(ExchangeName, string.Empty, properties, _dataSerializer.ToProto(data).Value);
            Console.WriteLine("Message {0}, was sent", messageIndex);
        }
    }
}
