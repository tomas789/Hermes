using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermés.Core.Common;

namespace Hermés.Core.Events
{
    public class MarketEvent : Event
    {
        public PriceGroup Price;

        public MarketEvent(DateTime time, PriceGroup price) : base(time)
        {
            Price = price;
        }
    }
}
