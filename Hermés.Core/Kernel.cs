using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermés.Core.Common;

namespace Hermés.Core
{
    /// <summary>
    /// Class responsible for event registering and dispatching.
    /// </summary>
    /// <remarks>
    /// You should never create custom Kernel class unless you know 
    /// what are you doing. Consider using Kernel instance which was
    /// created with subclass of Portfolio.
    /// </remarks>
    public sealed class Kernel : IDisposable
    {
        private readonly EventPriorityQueue _events = 
            new EventPriorityQueue();
        private readonly List<IEventConsumer> _eventConsumers = 
            new List<IEventConsumer>();

        public DateTime WallTime;

        /// <summary>
        /// Run simulation.
        /// </summary>
        /// <remarks>
        /// TODO: Rethink how to stop simulation.
        /// TODO: Consider asynchronous running dispatcher.
        /// </remarks>
        public void Run()
        {
            Dispatcher();
        }

        /// <summary>
        /// Subscribe class implementing IEventConsumer interface to 
        /// event dispatching.
        /// </summary>
        /// <remarks>
        /// Every event dispatched by this Kernel will be also send to
        /// <paramref name="consumer"/> in order in which they was 
        /// subscribed.
        /// </remarks>
        /// <param name="consumer">Event consumer to register</param>
        public void RegisterEventConsumer(IEventConsumer consumer)
        {
            _eventConsumers.Add(consumer);
        }

        /// <summary>
        /// Add new event to queue.
        /// </summary>
        /// <param name="ev">Event to add.</param>
        public void AddEvent(Event ev)
        {
            _events.Enqueue(ev);
        }

        /// <summary>
        /// Internal implementation of dispatcher.
        /// </summary>
        private void Dispatcher()
        {
            while (_events.Count != 0)
            {
                var ev = _events.Dequeue();
                foreach (var consumer in _eventConsumers)
                    consumer.DispatchEvent(ev);
            }
        }

        /// <summary>
        /// Release all resources held by Kernel.
        /// </summary>
        /// <remarks>
        /// In future events might be asynchronously fetched from
        /// external source. Like other simulation and so on.
        /// TODO: Remove and add again when needed.
        /// </remarks>
        public void Dispose()
        {
        }
    }
}
