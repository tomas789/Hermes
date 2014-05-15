using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermés.Core.Common;

namespace Hermés.Core.Events
{
    public class OrderEvent : Event
    {
        public Ticker Ticker;
        public TradeDirection Direction;
        public OrderKind Kind;
        public double Price;

        public OrderEvent(Ticker ticker, TradeDirection direction, OrderKind kind, double price = Double.NaN)
            : base(DateTime.Now)
        {
            Ticker = ticker;
            Direction = direction;
            Kind = kind;
            Price = price;
        }
    }
}
