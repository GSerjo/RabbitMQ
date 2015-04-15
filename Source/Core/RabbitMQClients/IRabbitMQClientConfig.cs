using System;

namespace Core.RabbitMQClients
{
    public interface IRabbitMQClientConfig
    {
        string HostName { get; set; }
        string Password { get; set; }
        string UserName { get; set; }
        IRabbitMQClient Create();
    }
}
