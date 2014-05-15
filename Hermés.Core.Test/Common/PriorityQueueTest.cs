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
        [TestMethod]
        public void EventMockCheck()
        {
            var dt = DateTime.Now;
            var e1 = new EventMock(dt, 0);
            var e2 = new EventMock(dt, 0);
            Assert.IsTrue(e1.Equals(e2));
            //Assert.AreEqual(e1.Value,e2.Value);
            //Assert.AreEqual(e1.Time, e2.Time);
        }

        [TestMethod]
        public void EventPriorityQueue_EnqueueAddFiveElements()
        {
            
        }

        [TestMethod]
        public void EventPriorityQueue_DequeueFromEmpty()
        {

        }

        [TestMethod]
        public void EventPriorityQueue_QueueFiveElementsAndDequeueFiveElements()
        {
            var queue = new EventPriorityQueue();
            
            var dt = DateTime.Now;
            for (var i = 0; i < 4; ++i) 
                queue.Enqueue(new EventMock(dt + TimeSpan.FromHours(i),i));
            queue.Enqueue(new EventMock(dt, 4));

            var listtest = new List<Event>();
            listtest.Add(new EventMock(dt,0));
            listtest.Add(new EventMock(dt,4));
            for (var i = 1; i < 4; ++i)
                listtest.Add(new EventMock(dt + TimeSpan.FromHours(i),i));

            var listtrue = new List<Event>();
            for (var i = 0; i < 5; ++i)
            {
                var e = queue.Dequeue();
                if (e != null)
                {
                    listtrue.Add(e);
                }
                else 
                    listtrue.Add(null);
            }

            for (var i = 0; i < 5; ++i)
                Assert.IsTrue(listtest[i].Equals(listtrue[i]));
        }

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
                var e = new EventMock(time + TimeSpan.FromDays(i), 0);
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
