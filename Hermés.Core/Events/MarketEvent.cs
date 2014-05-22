using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermés.Core.Common;

namespace Hermés.Core.Events
{
    /// <summary>
    /// Event representing arrival of new data from market.
    /// </summary>
    public class MarketEvent : Event
    {
        public Ticker Ticker;

        public PriceGroup Price;

        public MarketEvent(Ticker ticker, DateTime time, PriceGroup price) 
            : base(time)
        {
            Ticker = ticker;
            Price = price;
        }
    }
}