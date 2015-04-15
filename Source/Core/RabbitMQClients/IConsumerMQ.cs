using System;
using Nelibur.Sword.DataStructures;

namespace Core.RabbitMQClients
{
    public interface IConsumerMQ<T>
    {
        Option<QueueItem<T>> Dequeue();
        void Remove(IQueueItem item);
        void Rollback(IQueueItem item);
    }
}
