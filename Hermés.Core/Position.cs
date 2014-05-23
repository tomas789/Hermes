using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermés.Core.Common;

namespace Hermés.Core
{
    /// <summary>
    /// Representation of position held by portfolio.
    /// </summary>
    public class Position
    {
        public DataFeed Market;
        public TradeDirection Direction;
        public double Price;
        public double Size;

        public Position(DataFeed market, TradeDirection direction, double price, double size)
        {
            Market = market;
            Direction = direction;
            Price = price;
            Size = size;
        }

        public override string ToString()
        {
            return string.Format("Position :: DataFeed: {0}; TradeDirection: {1}; Price: {2}; Size: {3}", 
                Market, Direction, Price, Size);
        }
    }
}
