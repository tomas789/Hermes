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
        private PriorityQueue<Event> events;
        private IEnumerable<IEventConsumer> eventConsumers;

        public void Run()
        {
            throw new NotImplementedException();
        }

        public void AddEvent(Event ev)
        {
            throw new NotImplementedException();
        }

        private void Dispatcher()
        {
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
