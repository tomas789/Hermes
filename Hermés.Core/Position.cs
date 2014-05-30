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
        public DateTime OrderTime { get; private set; }
        public DataFeed Market { get; private set; }
        public TradeDirection Direction { get; private set; }
        public double OrderPrice { get; private set; }
        public double Size { get; private set; }

        public OrderKind OrderKind { get; private set; }

        private Position()
        {
        }

        public static Position MakeLimitOrder(
            DateTime time, DataFeed market, TradeDirection direction, 
            double orderPrice, double size)
        {
            return new Position()
            {
                OrderTime = time,
                Market = market,
                Direction = direction,
                OrderPrice = orderPrice,
                Size = size,
                OrderKind = Common.OrderKind.Limit
            };
        }

        public static Position MakeMarketOrder(
            DateTime time, DataFeed market, TradeDirection direction, double size)
        {
            return new Position()
            {
                OrderTime = time,
                Market = market,
                Direction = direction,
                Size = size,
                OrderKind = Common.OrderKind.Market
            };
        }

        public bool IsFilled { get; private set; }

        public DateTime FillTime { get; private set; }
        public double FillPrice { get; private set; }
        public double Cost { get; private set; }

        public void Fill(DateTime time, double fillPrice, double cost)
        {
            IsFilled = true;
            FillTime = time;
            FillPrice = fillPrice;
            Cost = cost;
        }

        public override string ToString()
        {
            return IsFilled
                ? string.Format("Filled position: At {0}, Size {1}, Direction: {2}, Cost {3}, Order price {4}",
                    FillPrice, Size, Direction, Cost, OrderPrice)
                : string.Format("Unfilled position: Size {0}, Direction: {1}, Order price {2}",
                    Size, Direction, OrderPrice);
        }
    }
}
