using System;
using System.ServiceModel.Web;
using Core;
using Core.Extensions;
using Core.RabbitMQClients;
using HelloContracts;
using Nelibur.ServiceModel.Services;
using Nelibur.ServiceModel.Services.Default;
using Nelibur.ServiceModel.Services.Operations;
using Nelibur.Sword.Extensions;
using RabbitMQ.Client;

namespace HelloClient
{
    internal class Program : IPostOneWay<DataRequest>
    {
        private const string ExchangeName = "HelloExchange";
        private const string HostName = "localhost";
        private const string Password = "guest";
        private const string QueueName = "HelloQueue";
        private const string UserName = "guest";
        private static readonly DataSerializer _dataSerializer = new DataSerializer();
        private static IProducerMQ<Data> _producer;
        private static WebServiceHost _service;
        private RabbitMQClient _rabbitClient;

        public void PostOneWay(DataRequest request)
        {
            var data = new Data
            {
                Id = request.Id
            };
            _producer.EnqueuePersistent(data);
        }

        private static void Main()
        {
            new Program().Run();
        }

        private void Run()
        {
            InitialiseProducer();

            _service = new WebServiceHost(typeof(JsonServicePerCall));
            _service.Open();

            NeliburRestService.Configure(x => x.Bind<DataRequest, Program>());

            //            Send1();
            //            Send2();

            Console.WriteLine("Press Any Key to send a message");
            Console.ReadKey();

            _rabbitClient.SafeDispose();
            _service.Close();
        }

        private void Send1()
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

        private void Send2()
        {
            InitialiseProducer();

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

        private void InitialiseProducer()
        {
            _rabbitClient = RabbitMQClient.Configure(x =>
            {
                x.HostName = HostName;
                x.Password = Password;
                x.UserName = UserName;
            }).Create();

            _producer = _rabbitClient.GetProducer<Data>(ExchangeName, QueueName, _dataSerializer.ToProto);
        }

        private void SendMessage(int messageIndex)
        {
            _producer.EnqueuePersistent(new Data { Id = Guid.NewGuid() });
            Console.WriteLine("Message {0}, was sent", messageIndex);
        }

        private void SendMessage(IModel model, int messageIndex)
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
