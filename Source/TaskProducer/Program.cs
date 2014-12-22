using System;
using Nelibur.ServiceModel.Clients;
using Nelibur.Sword.Extensions;
using TaskContracts;

namespace TaskProducer
{
    internal class Program
    {
        private static readonly string _serviceAddress = "http://localhost:7050/tasks";

        private static void Main()
        {
            while (true)
            {
                Console.WriteLine("Press ESC to exit or Any Key to send a message");

                ConsoleKeyInfo key = Console.ReadKey();
                if (key.Key == ConsoleKey.Escape)
                {
                    break;
                }
                10.Times().Iter(x => SendMessage());
            }
        }

        private static void SendMessage()
        {
            using (var client = new JsonServiceClient(_serviceAddress))
            {
                client.Post(new TaskCommand { Id = Guid.NewGuid() });
            }
        }
    }
}
