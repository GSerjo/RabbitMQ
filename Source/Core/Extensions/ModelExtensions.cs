using System;
using RabbitMQ.Client;

namespace Core.Extensions
{
    public static class ModelExtensions
    {
        public static void CreateDurableQueue(this IModel model, string queueName)
        {
            model.QueueDeclare(queueName, true, false, false, null);
        }

        public static void CreateDurableDirectExcange(this IModel model, string exchangeName)
        {
            model.ExchangeDeclare(exchangeName, ExchangeType.Direct, true);
        }
    }
}
