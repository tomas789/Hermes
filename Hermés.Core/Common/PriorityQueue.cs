using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hermés.Core.Common
{
    // not implemented yet
    class PriorityQueue<T> : IEnumerable<T> where T : IComparable<T>
    {

        public void Enqueue(T t)
        {
            throw new NotImplementedException();
        }

        public T Dequeue()
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }


    // not variadic
    public class EventPriorityQueue
    {
        private SortedDictionary<DateTime, Queue<Event>> p_queue;

        public EventPriorityQueue()
        {
            p_queue = new SortedDictionary<DateTime, Queue<Event>>();
        }

        // slow implementation for queues of low elements
        public void Enqueue(Event e)
        {
            DateTime dt = e.Time;
            if (p_queue.ContainsKey(dt))
            {
                Queue<Event> queue = p_queue[dt];
                queue.Enqueue(e);
            }
            else
            {
                var queue = new Queue<Event>();
                queue.Enqueue(e);
                p_queue.Add(dt, queue);
            }
        }

        public Event Dequeue()
        {
            if (p_queue.Count > 0)
            {
                Queue<Event> queue = p_queue.Values.First();
                Event e = queue.Dequeue();
                if (queue.Count == 0)
                    p_queue.Remove(e.Time);
                return e;
            }
            return null;
        }

        public IEnumerator<Event> GetEnumerator()
        {
            foreach (KeyValuePair<DateTime,Queue<Event>> pair in p_queue)
            {
                foreach (Event e in pair.Value)
                {
                    yield return e;
                }
            }
        }


    }
}
