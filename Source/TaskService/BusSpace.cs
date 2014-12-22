using System;
using Core.Extensions;
using RabbitMQ.Client;

namespace TaskService
{
    public sealed class BusSpace
    {
        private const string ExchangeName = "TaskExchange";
        private const string HostName = "localhost";
        private const string Password = "guest";
        private const string QueueName = "TaskQueue";
        private const string UserName = "guest";

        private readonly IModel _model;

        public BusSpace()
        {
            _model = CreateModel();
        }

        public void EnqueuePersistentMessage(byte[] message)
        {
            IBasicProperties properties = _model.CreateBasicProperties();
            properties.SetPersistent(true);
            _model.BasicPublish(ExchangeName, string.Empty, properties, message);
        }

        public void CreateQueues()
        {
            _model.CreateDurableQueue(QueueName);
            _model.CreateDurableDirectExcange(ExchangeName);
            _model.QueueBind(QueueName, ExchangeName, string.Empty);
        }

        private IModel CreateModel()
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = HostName,
                Password = Password,
                UserName = UserName
            };

            IConnection connection = connectionFactory.CreateConnection();
            return connection.CreateModel();
        }
    }
}
