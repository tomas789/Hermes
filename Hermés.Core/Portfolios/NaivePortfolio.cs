using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Hermés.Core.Common;
using Hermés.Core.Events;

namespace Hermés.Core.Portfolios
{
    /// <summary>
    /// This portfolio executes every signal obtained from every strategy
    /// which was registered.
    /// </summary>
    /// <remarks>
    /// Warning: This moght not be the best possible implementation you
    /// want to run with real money. For testing purposes only.
    /// </remarks>
    public class NaivePortfolio : Portfolio
    {
        private readonly double _initialCapital;

        public NaivePortfolio(double initialCapital)
        {
            _initialCapital = initialCapital;
        }

        public override void DispatchConcrete(FillEvent ev)
        {
            var position = new Position(ev.Market, ev.Direction, ev.FillPrice, ev.Size);
            Positions.Add(ev.Time, position);
        }

        public override void DispatchConcrete(MarketEvent e)
        {
        }

        public override void DispatchConcrete(OrderEvent e)
        {
        }

        public override void DispatchConcrete(SignalEvent e)
        {
            Debug.WriteLine("NaivePortfolio got SignalEvent: {0}, Time: {1}", e, WallTime);
            if (e.Kind == SignalKind.Hold)
                return;

            TradeDirection direction;
            switch (e.Kind)
            {
                case SignalKind.Buy:
                    direction = TradeDirection.Buy;
                    break;
                case SignalKind.Sell:
                    direction = TradeDirection.Sell;
                    break;
                default:
                    throw new ImpossibleException();
            }

            var order = new OrderEvent(WallTime, e.Market, direction, OrderKind.Market);
            Kernel.AddEvent(order);
        }

        protected override double GetPortfolioValue()
        {
            var posByMarket = new Dictionary<DataFeed, List<Position>>();
            foreach (var item in Positions.OrderBy(item => item.Key))
            {
                List<Position> pos;
                if (!posByMarket.TryGetValue(item.Value.Market, out pos))
                    posByMarket.Add(item.Value.Market, new List<Position>());
                posByMarket[item.Value.Market].Add(item.Value);
            }

            double portfolioValue = _initialCapital;
            foreach (var item in posByMarket)
            {
                var market = item.Key;
                var positions = item.Value;

                var sizeInHold = 0.0;
                var lastPrice = 0.0;

                foreach (var position in positions)
                {
                    if (sizeInHold == 0) 
                    {
                        sizeInHold = position.Size;
                        lastPrice = position.Price;
                    }
                    else
                    {
                        portfolioValue += (position.Price - lastPrice) * position.Size * market.PointPrice;
                        sizeInHold += position.Size;
                        lastPrice = position.Price;
                    }
                }

                if (sizeInHold != 0)
                {
                    var directedPriceKind = sizeInHold > 0 ? PriceKind.Ask : PriceKind.Bid;
                    var currentPrice = market.CurrentPrice(directedPriceKind);

                    portfolioValue += (currentPrice.Close - lastPrice) * sizeInHold * market.PointPrice;
                }
            }

            return portfolioValue;
        }
    }
}
