using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermés.Core.Common;

namespace Hermés.Core
{
    public class Position
    {
        public Ticker Ticker;
        public TradeDirection Direction;
        public double Size;

        public Position(Ticker ticker, TradeDirection direction, double size)
        {
            Ticker = ticker;
            Direction = direction;
            Size = size;
        }
    }
}
