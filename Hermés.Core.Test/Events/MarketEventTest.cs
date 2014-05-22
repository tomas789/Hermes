using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermés.Core.Common;
using Hermés.Core.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hermés.Core.Test.EventsTest
{
    [TestClass]
    class MarketEventTest
    {
        [TestMethod]
        public void MarketEvent_TestDateTimeConstruction()
        {
            var time = DateTime.Now;
            var ev = new MarketEvent(null, time, null);
            Assert.AreEqual(time, ev.Time);
        }
    }
}
