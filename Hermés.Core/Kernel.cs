using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        private bool _running = true;

        public EventPriorityQueue Events { get { return _events; } }

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

        public void Step()
        {
            DispatcherStep();
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

        public void UnregisterEventConsumer(IEventConsumer consumer)
        {
            _eventConsumers.Remove(consumer);
        }

        public void StopSimulation()
        {
            lock (_events)
            {
                _running = false;
                Monitor.PulseAll(_events);
            }
        }

        /// <summary>
        /// Add new event to queue.
        /// </summary>
        /// <param name="ev">Event to add.</param>
        public void AddEvent(Event ev)
        {
            lock (_events)
            {
                _events.Enqueue(ev);
                Monitor.Pulse(_events);
            }
            
        }

        /// <summary>
        /// Internal implementation of dispatcher.
        /// </summary>
        private void Dispatcher()
        {
            while (_running)
            {
                if (_events.Count == 0)
                    lock (_events)
                        Monitor.Wait(_events);

                DispatcherStep();
            }
        }

        private void DispatcherStep()
        {
            if (_events.Count == 0)
                return;

            var ev = _events.Dequeue();
            foreach (var consumer in _eventConsumers)
                consumer.DispatchEvent(ev);
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
