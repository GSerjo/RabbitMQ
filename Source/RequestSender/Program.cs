using System;
using HelloContracts;
using Nelibur.ServiceModel.Clients;
using Nelibur.Sword.Extensions;
using RequestSender.Properties;

namespace RequestSender
{
    internal class Program
    {
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
                10000.Times().Iter(x => SendMessage(x));
            }
        }

        private static void SendMessage(int messageIndex)
        {
            using (var client = new JsonServiceClient(Settings.Default.HelloAddress))
            {
                client.Post(new DataRequest { Id = Guid.NewGuid()});
            }
            Console.WriteLine("MessageIndex: {0}", messageIndex);
        }
    }
}
