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

        public Position Position { get; private set; }

        public FillEvent(DateTime time, Position position)
            : base(time)
        {
            Position = position;
        }
    }
}
