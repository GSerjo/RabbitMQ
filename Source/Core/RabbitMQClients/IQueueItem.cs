using System;

namespace Core.RabbitMQClients
{
    public interface IQueueItem
    {
        ulong Id { get; set; }
    }
}
