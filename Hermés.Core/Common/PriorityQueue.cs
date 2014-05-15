using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hermés.Core.Common
{
    public class EventPriorityQueue
    {
        private readonly SortedDictionary<DateTime, Queue<Event>> _queue =
            new SortedDictionary<DateTime, Queue<Event>>();

        public void Enqueue(Event e)
        {
            var dt = e.Time;
            if (_queue.ContainsKey(dt))
            {
                var queue = _queue[dt];
                queue.Enqueue(e);
            }
            else
            {
                var queue = new Queue<Event>();
                queue.Enqueue(e);
                _queue.Add(dt, queue);
            }
        }

        public Event Dequeue()
        {
            if (_queue.Count <= 0) 
                return null;
            var queue = _queue.Values.First();
            var e = queue.Dequeue();
            if (queue.Count == 0)
                _queue.Remove(e.Time);
            return e;
        }

        public IEnumerator<Event> GetEnumerator()
        {
            return _queue.SelectMany(pair => pair.Value).GetEnumerator();
        }
    }
}
