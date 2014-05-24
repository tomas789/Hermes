using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Positions.Add(position);
        }

        public override void DispatchConcrete(MarketEvent e)
        {
        }

        public override void DispatchConcrete(OrderEvent e)
        {
        }

        public override void DispatchConcrete(SignalEvent e)
        {
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

            var order = new OrderEvent(e.Market, direction, OrderKind.Market);
            Kernel.AddEvent(order);
        }

        protected override double GetPortfolioValue()
        {
            var sizeHolded = new Dictionary<DataFeed, double>();
            var priceCache = new Dictionary<DataFeed, double>();
            foreach (var position in Positions)
            {
                if (!sizeHolded.ContainsKey(position.Market))
                    sizeHolded.Add(position.Market, 0);

                if (!priceCache.ContainsKey(position.Market))
                {
                    var posHolder = position;
                    Func<Position, PriceKind> selectKind = (pos) =>
                    {
                        switch (pos.Direction)
                        {
                            case TradeDirection.Buy:
                                return PriceKind.Ask;
                            case TradeDirection.Sell:
                                return PriceKind.Bid;
                            default:
                                throw new ImpossibleException();
                        }
                    };

                    var pricesBidAsk = (from datafeed in DataFeeds
                                        select datafeed.CurrentPrice(posHolder.Market, 
                                                                     selectKind(posHolder))
                                            into datafeedPrice
                                            where datafeedPrice.Close.HasValue
                                            select datafeedPrice);

                    var prices = (from datafeed in DataFeeds
                                  let kind = PriceKind.Unspecified
                                  select datafeed.CurrentPrice(posHolder.Market, kind)
                                      into datafeedPrice
                                      where datafeedPrice.Close.HasValue
                                      select datafeedPrice);

                    var price = pricesBidAsk.Concat(prices).FirstOrDefault();
                    if (price != null && price.Close.HasValue) 
                        priceCache.Add(position.Market, price.Close.Value);
                    else
                        throw new OperationCanceledException(
                            string.Format("Current price not found for ticker {0}", position.Market));
                }
                
                double change = 0;
                switch (position.Direction)
                {
                    case TradeDirection.Buy:
                        change = (priceCache[position.Market] - position.Price) * position.Size;
                        break;
                    case TradeDirection.Sell:
                        change = (position.Price - priceCache[position.Market]) * position.Size;
                        break;
                    default:
                        throw new ImpossibleException();
                }

                sizeHolded[position.Market] += change;
            }

            return _initialCapital + (from item in sizeHolded 
                                      let ticker = item.Key 
                                      let pts = item.Value
                                      let pointPrice = item.Key.PointPrice 
                                      select pts*pointPrice).Sum();
        }
    }
}
