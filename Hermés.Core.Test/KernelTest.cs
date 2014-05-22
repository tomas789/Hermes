using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hermés.Core.Test
{
    [TestClass]
    public class KernelTest
    {
        class EventConsumerMock : IEventConsumer
        {
            public readonly List<Event> Events =
                new List<Event>();

            public void DispatchEvent(Event e)
            {
                Events.Add(e);
            }
        }

        class EventMock : Event
        {
            public readonly int Id;

            public EventMock(DateTime time, int id) : base(time)
            {
                Id = id;
            }
        }

        [TestMethod]
        public void Kernel_DispatchesMethod()
        {
            var kernel = new Kernel();
            var consumer = new EventConsumerMock();
            var eventList = new List<Event>();
            
            kernel.RegisterEventConsumer(consumer);

            var now = DateTime.Now;
            for (var i = 0; i < 5; ++i)
            {
                var ev = new EventMock(now, i);
                eventList.Add(ev);
                kernel.AddEvent(ev);
            }

            var task = new Task(kernel.Run);
            task.Start();

            Thread.Sleep(50);
            kernel.StopSimulation();


            Assert.AreEqual(eventList.Count, consumer.Events.Count);
            var j = 0;
            while (j < eventList.Count)
            {
                Assert.IsTrue(eventList[j].Equals(consumer.Events[j]));
                Assert.AreEqual(j, ((EventMock)consumer.Events[j]).Id);
                ++j;
            }
        }
    }
}
