using System;

namespace Core.RabbitMQClients
{
    public interface IProducerMQ<in T>
    {
        void EnqueueNonPersistent(T item);
        void EnqueuePersistent(T item);
    }
}
