using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermés.Core.Common;

namespace Hermés.Core.Events
{
    /// <summary>
    /// Order event.
    /// </summary>
    public class OrderEvent : Event
    {
        public DataFeed Market;
        public TradeDirection Direction;
        public OrderKind Kind;
        public double Price;

        public OrderEvent(DataFeed market, TradeDirection direction, OrderKind kind, double price = Double.NaN)
            : base(DateTime.Now)
        {
            Market = market;
            Direction = direction;
            Kind = kind;
            Price = price;
        }
    }
}
