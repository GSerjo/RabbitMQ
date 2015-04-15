using System;

namespace Core.RabbitMQClients
{
    public sealed class QueueItem<T> : IQueueItem
    {
        public T Value { get; set; }
        public ulong Id { get; set; }
    }
}
