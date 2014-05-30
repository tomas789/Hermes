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
            AddPosition(ev.Time, ev.Position);
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

            var position = Position.MakeMarketOrder(WallTime, e.Market, direction, 1);
            Kernel.AddEvent(new OrderEvent(WallTime, position));
        }

        protected override double GetPortfolioValue()
        {
            var price = _initialCapital;

            lock (Positions)
            {
                var currentPriceCache = new Dictionary<DataFeed, PriceGroup>();

                foreach (var position in Positions)
                {
                    var market = position.Market;
                    if (!currentPriceCache.ContainsKey(market))
                    {
                        var currentPrice = market.CurrentPrice();
                        if (currentPrice == null)
                            throw new InvalidOperationException(
                                string.Format("Unable to get current price of {0} at time {1}", market, WallTime));

                        currentPriceCache.Add(market, currentPrice);
                    }

                    price += position.Valuate(currentPriceCache[market]);
                }
            }

            return price;

            /*
            Dictionary<DataFeed, List<Position>> posByMarket;
            lock (Positions)
            {
                posByMarket =
                    (from item in Positions.Values
                        from position in item
                        group position by position.Market
                        into g
                        select g).ToDictionary(g => g.Key, g => g.ToList());
            }

            var portfolioValue = _initialCapital;
            foreach (var item in posByMarket)
            {
                var market = item.Key;
                var positions = item.Value;

                var sizeInHold = 0.0;
                var lastPrice = 0.0;

                foreach (var position in positions)
                {
                    double size;

                    switch (position.Direction)
                    {
                        case TradeDirection.Buy:
                            size = position.Size;
                            break;
                        case TradeDirection.Sell:
                            size = -1*position.Size;
                            break;
                        default:
                            throw new ImpossibleException();
                    }

                    if (sizeInHold == 0) 
                    {
                        sizeInHold = size;
                        lastPrice = position.FillPrice;
                    }
                    else
                    {
                        portfolioValue += (position.FillPrice - lastPrice) * size * market.PointPrice;
                        sizeInHold += position.Size;
                        lastPrice = position.FillPrice;
                    }
                }

                if (sizeInHold < 1e-6) 
                    continue;

                var directedPriceKind = sizeInHold > 0 ? PriceKind.Ask : PriceKind.Bid;
                var currentPrice = market.CurrentPrice(directedPriceKind);

                portfolioValue += (currentPrice.Close - lastPrice) * sizeInHold * market.PointPrice;
            }

            return portfolioValue; */
        }
    }
}
