using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Hermés.Core.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hermés.Core.Common;

namespace Hermés.Core.Test.Common
{
    public class EventMock : Event
    {
        public int Value;

        public EventMock(DateTime time, int v) : base(time)
        {
            Value = v;
        }

        public override bool Equals(Event obj)
        {
            if (obj is EventMock)
                return (this.Time.Equals(obj.Time) && this.Value.Equals(((EventMock) obj).Value));
            return (base.Equals(obj));
        }

    }

    [TestClass]
    public class EventPriorityQueueTest
    {
        public void EventPriorityQueue_TestOnSample(List<Event> expected, EventPriorityQueue queue)
        {
            foreach (var e in expected)
                Assert.IsTrue(e.Equals(queue.Dequeue()));
        }

        [TestMethod]
        public void EventPriorityQueue_SamePriorityElements()
        {
            var expected = new List<Event>();
            var queue = new EventPriorityQueue();
            var time = DateTime.Now;
            for (var i = 0; i < 5; ++i)
            {
                var e = new EventMock(time, 0);
                expected.Add(e);
                queue.Enqueue(e);
            }

            EventPriorityQueue_TestOnSample(expected, queue);
        }

        [TestMethod]
        public void EventPriorityQueue_DifferentPriorityElementsRightOrder()
        {
            var expected = new List<Event>();
            var queue = new EventPriorityQueue();
            var time = DateTime.Now;
            for (var i = 0; i < 5; ++i)
            {
                var shifted = time.AddDays(1);
                var e = new EventMock(shifted, 0);
                expected.Add(e);
                queue.Enqueue(e);
            }

            EventPriorityQueue_TestOnSample(expected, queue);
        }

        [TestMethod]
        public void EventPriorityQueue_DifferentPriorityElementsDifferentOrder()
        {
            var expected = new List<Event>();
            var queue = new EventPriorityQueue();
            var time = DateTime.Now;
            for (var i = 0; i < 5; ++i)
            {
                var e = new EventMock(time + TimeSpan.FromDays(i), 0);
                expected.Add(e);
            }

            for (var i = expected.Count - 1; i >= 0; --i)
                queue.Enqueue(expected[i]);

            EventPriorityQueue_TestOnSample(expected, queue);
        }
    }
}
