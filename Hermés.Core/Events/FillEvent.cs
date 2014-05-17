using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermés.Core.Common;

namespace Hermés.Core.Events
{
    public class FillEvent : Event
    {
        public Ticker Ticker;
        public TradeDirection Direction;
        public double Price;
        public double FillPrice;
        public double Cost;
        public double Size;

        public FillEvent(Ticker ticker, TradeDirection direction, 
            double price, double fillPrice, double cost, double size)
            : base(DateTime.Now)
        {
            Ticker = ticker;
            Direction = direction;
            Price = price;
            FillPrice = fillPrice;
            Cost = cost;
            Size = size;
        }
    }
}
