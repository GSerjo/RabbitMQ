using System;
using Nelibur.Sword.DataStructures;

namespace Core.RabbitMQClients
{
    public interface IRabbitMQClient : IDisposable
    {
        void ClearQueue(string queueName);
        IConsumerMQ<T> GetConsumer<T>(string exchangeName, string queueName, Func<byte[], Option<T>> dataConverter);
        IProducerMQ<T> GetProducer<T>(string exchangeName, string queueName, Func<T, Option<byte[]>> dataConverter);
    }
}
