using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Nelibur.Sword.DataStructures;
using Nelibur.Sword.Extensions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Core.RabbitMQClients
{
    public sealed class RabbitMQClient : IDisposable
    {
        private readonly IConnection connection;

        private RabbitMQClient(IRabbitMQClientConfig config)
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = config.HostName,
                UserName = config.UserName,
                Password = config.Password
            };
            connection = connectionFactory.CreateConnection();
        }

        public void Dispose()
        {
            connection.Dispose();
        }

        public static IRabbitMQClientConfig Configure(Action<IRabbitMQClientConfig> action)
        {
            return new RabbitMQClientConfig().Config(action);
        }

        public void ClearQueue(string queueName)
        {
            using (IModel model = connection.CreateModel())
            {
                model.QueuePurge(queueName);
            }
        }

        public IConsumerMQ<T> GetConsumer<T>(string exchangeName, string queueName, Func<byte[], Option<T>> dataConverter)
        {
            if (string.IsNullOrWhiteSpace(exchangeName))
            {
                throw new ArgumentException("exchangeName");
            }
            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new ArgumentException("queueName");
            }

            CreateDurableQueue(queueName);
            CreateDurableDirectExcange(exchangeName);
            QueueBind(queueName, exchangeName, string.Empty);

            return new Consumer<T>(queueName, connection, dataConverter);
        }

        public IProducerMQ<T> GetProducer<T>(string exchangeName, string queueName, Func<T, Option<byte[]>> dataConverter)
        {
            if (string.IsNullOrWhiteSpace(exchangeName))
            {
                throw new ArgumentException("exchangeName");
            }
            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new ArgumentException("queueName");
            }

            CreateDurableQueue(queueName);
            CreateDurableDirectExcange(exchangeName);
            QueueBind(queueName, exchangeName, string.Empty);

            return new Producer<T>(exchangeName, connection, dataConverter);
        }

        private void CreateDurableDirectExcange(string exchangeName)
        {
            using (IModel model = connection.CreateModel())
            {
                model.ExchangeDeclare(exchangeName, ExchangeType.Direct, true);
            }
        }

        private void CreateDurableQueue(string queueName)
        {
            using (IModel model = connection.CreateModel())
            {
                model.QueueDeclare(queueName, true, false, false, null);
            }
        }

        private void QueueBind(string queueName, string exchangeName, string routingKey)
        {
            using (IModel model = connection.CreateModel())
            {
                model.QueueBind(queueName, exchangeName, routingKey);
            }
        }


        private sealed class Consumer<T> : IConsumerMQ<T>
        {
            private readonly QueueingBasicConsumer consumer;
            private readonly Func<byte[], Option<T>> dataConverter;
            private readonly IModel model;
            private readonly object modelLocker = new object();

            public Consumer(string queueName, IConnection connection, Func<byte[], Option<T>> dataConverter)
            {
                this.dataConverter = dataConverter;
                model = connection.CreateModel();
                model.BasicQos(0, (ushort)Environment.ProcessorCount, false);

                consumer = new QueueingBasicConsumer(model);
                model.BasicConsume(queueName, false, consumer);
            }

            public Option<QueueItem<T>> Dequeue()
            {
                BasicDeliverEventArgs args = consumer.Queue.Dequeue();
                return dataConverter(args.Body).Map(x => new QueueItem<T> { Id = args.DeliveryTag, Value = x });
            }

            public void Remove(IQueueItem item)
            {
                lock (modelLocker)
                {
                    model.BasicAck(item.Id, false);
                }
            }

            public void Rollback(IQueueItem item)
            {
                model.BasicReject(item.Id, true);
            }
        }


        private sealed class Producer<T> : IProducerMQ<T>
        {
            private readonly IConnection connection;
            private readonly Func<T, Option<byte[]>> dataConverter;
            private readonly string exchangeName;
            private readonly IModel model;

            public Producer(string exchangeName, IConnection connection, Func<T, Option<byte[]>> dataConverter)
            {
                if (string.IsNullOrWhiteSpace(exchangeName))
                {
                    throw new ArgumentException("exchangeName");
                }

                this.connection = connection;
                this.dataConverter = dataConverter;
                this.exchangeName = exchangeName;
                model = connection.CreateModel();
            }

            public void EnqueueNonPersistent(T item)
            {
                Enqueue(item, false);
            }

            public void EnqueuePersistent(T item)
            {
                Enqueue(item, true);
            }

            private void Enqueue(T item, bool setMessagePersistent)
            {
                dataConverter(item)
                    .ThrowOnEmpty(() => new ArgumentException("item"))
                    .Do(x => Publish(x, setMessagePersistent));
            }

            private void Publish(byte[] message, bool setMessagePersistent)
            {
                IBasicProperties properties = model.CreateBasicProperties();
                properties.SetPersistent(setMessagePersistent);
                model.BasicPublish(exchangeName, string.Empty, properties, message);
            }
        }


        private class RabbitMQClientConfig : IRabbitMQClientConfig
        {
            public string HostName { get; set; }
            public string Password { get; set; }
            public string UserName { get; set; }

            public RabbitMQClient Create()
            {
                Validate();
                return new RabbitMQClient(this);
            }

            public IRabbitMQClientConfig Config(Action<IRabbitMQClientConfig> action)
            {
                action(this);
                return this;
            }

            private void Validate()
            {
                var nullCheck = new List<string>
                {
                    HostName, Password, UserName
                };
                if (nullCheck.Any(x => x == null))
                {
                    throw new ConfigurationErrorsException();
                }
            }
        }
    }
}
