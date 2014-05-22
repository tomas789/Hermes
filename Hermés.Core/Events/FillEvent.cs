using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermés.Core.Common;

namespace Hermés.Core.Events
{
    /// <summary>
    /// Event representing fill of previously executed order
    /// based on OrderEvent..
    /// </summary>
    public class FillEvent : Event
    {
        public DataFeed Market;
        public TradeDirection Direction;
        public double Price;
        public double FillPrice;
        public double Cost;
        public double Size;

        public FillEvent(DataFeed market, TradeDirection direction, 
            double price, double fillPrice, double cost, double size)
            : base(DateTime.Now)
        {
            Market = market;
            Direction = direction;
            Price = price;
            FillPrice = fillPrice;
            Cost = cost;
            Size = size;
        }
    }
}
