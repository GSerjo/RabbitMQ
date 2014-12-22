using System;
using System.ServiceModel.Web;
using Nelibur.ServiceModel.Services;
using Nelibur.ServiceModel.Services.Default;
using TaskContracts;

namespace TaskService
{
    internal class Program
    {
        private static WebServiceHost _service;

        private static void Main()
        {
            var busSpase = new BusSpace();
            busSpase.CreateQueues();

            var taskProcessor = new TaskProcessor(busSpase);

            NeliburRestService.Configure(x => x.Bind<TaskCommand, TaskProcessor>(() => taskProcessor));

            _service = new WebServiceHost(typeof(JsonServicePerCall));
            _service.Open();

            Console.WriteLine("Task Service is running");
            Console.WriteLine("Press any key to exit");

            Console.ReadKey();
            _service.Close();
        }
    }
}
