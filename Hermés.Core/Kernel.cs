using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermés.Core.Common;

namespace Hermés.Core
{
    sealed class Kernel : IDisposable
    {
        private readonly EventPriorityQueue _events = 
            new EventPriorityQueue();
        private readonly List<IEventConsumer> _eventConsumers = 
            new List<IEventConsumer>();

        public void Run()
        {
            Dispatcher();
        }

        public void RegisterEventConsumer(IEventConsumer consumer)
        {
            _eventConsumers.Add(consumer);
        }

        public void AddEvent(Event ev)
        {
            _events.Enqueue(ev);
        }

        private void Dispatcher()
        {
            while (_events.Count != 0)
            {
                var ev = _events.Dequeue();
                foreach (var consumer in _eventConsumers)
                    consumer.DispatchEvent(ev);
            }
        }

        public void Dispose()
        {
        }
    }
}
