using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace HelloServer
{
    internal class Program
    {
        private const string HostName = "localhost";
        private const string Password = "guest";
        private const string UserName = "guest";
        private const string QueueName = "HelloQueue";

        private static void Main()
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = HostName,
                Password = Password,
                UserName = UserName
            };

            var connection = connectionFactory.CreateConnection();
            IModel model = connection.CreateModel();
            model.BasicQos(0, 1, false);

            var consumer = new QueueingBasicConsumer(model);
            model.BasicConsume(QueueName, false, consumer);

            var tokenSource = new CancellationTokenSource();
            Task.Run(() => ReceiveMessage(tokenSource.Token, consumer, model), tokenSource.Token);

            Console.WriteLine("Press ESC to exit");
            if (Console.ReadKey().Key == ConsoleKey.Escape)
            {
                tokenSource.Cancel();
            }
            model.Close();
        }

        private static void ReceiveMessage(CancellationToken token, QueueingBasicConsumer consumer, IModel model)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }
                BasicDeliverEventArgs args = consumer.Queue.Dequeue();
                var message = Encoding.Default.GetString(args.Body);
                model.BasicAck(args.DeliveryTag, false);
                Console.WriteLine("Message: {0}", message);
            }
        }
    }
}
