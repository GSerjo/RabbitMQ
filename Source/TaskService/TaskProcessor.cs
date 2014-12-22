using System;
using System.Text;
using Nelibur.ServiceModel.Services.Operations;
using Nelibur.Sword.Extensions;
using Newtonsoft.Json;
using TaskContracts;

namespace TaskService
{
    public sealed class TaskProcessor : IPostOneWay<TaskCommand>
    {
        private readonly BusSpace _busSpace;

        public TaskProcessor(BusSpace busSpace)
        {
            _busSpace = busSpace;
        }

        public void PostOneWay(TaskCommand request)
        {
            request.ToOption()
                   .Map(JsonConvert.SerializeObject)
                   .Map(Encoding.Default.GetBytes)
                   .Do(Enqueue)
                   .Do(x => Console.WriteLine("Task: {0} processed", request.Id));
        }

        private void Enqueue(byte[] message)
        {
            _busSpace.EnqueuePersistentMessage(message);
        }
    }
}
