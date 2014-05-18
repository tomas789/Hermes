using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermés.Core.Common;
using Hermés.Core.Events;

namespace Hermés.Core
{
    public class NaivePortfolio : Portfolio
    {
        private readonly List<FillEvent> _fillEvents =
            new List<FillEvent>();

        private readonly double _initialCapital;

        public NaivePortfolio(double initialCapital)
        {
            _initialCapital = initialCapital;
        }

        public override void DispatchConcrete(FillEvent ev)
        {
            var position = new Position(ev.Ticker, ev.Direction, ev.FillPrice, ev.Size);
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

            var order = new OrderEvent(e.Ticker, direction, OrderKind.Market);
            Kernel.AddEvent(order);
        }

        protected override double GetPortfolioValue()
        {
            var sizeHolded = new Dictionary<Ticker, double>();
            var priceCache = new Dictionary<Ticker, double>();
            foreach (var position in Positions)
            {
                if (!sizeHolded.ContainsKey(position.Ticker))
                    sizeHolded.Add(position.Ticker, 0);

                if (!priceCache.ContainsKey(position.Ticker))
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
                                        select datafeed.CurrentPrice(posHolder.Ticker, 
                                                                     selectKind(posHolder))
                                            into datafeedPrice
                                            where datafeedPrice.HasValue
                                            select datafeedPrice);

                    var prices = (from datafeed in DataFeeds
                                  let kind = PriceKind.Unspecified
                                  select datafeed.CurrentPrice(posHolder.Ticker, kind)
                                      into datafeedPrice
                                      where datafeedPrice.HasValue
                                      select datafeedPrice);

                    var price = pricesBidAsk.Concat(prices).FirstOrDefault();
                    if (price.HasValue) 
                        priceCache.Add(position.Ticker, price.Value);
                    else
                        throw new OperationCanceledException(
                            string.Format("Current price not found for ticker {0}", position.Ticker));
                }
                
                double change = 0;
                switch (position.Direction)
                {
                    case TradeDirection.Buy:
                        change = (priceCache[position.Ticker] - position.Price) * position.Size;
                        break;
                    case TradeDirection.Sell:
                        change = (position.Price - priceCache[position.Ticker]) * position.Size;
                        break;
                    default:
                        throw new ImpossibleException();
                }

                sizeHolded[position.Ticker] += change;
            }

            var value = _initialCapital;
            foreach (var item in sizeHolded)
            {
                var ticker = item.Key;
                var pts = item.Value;

                if (!TickerInfos.ContainsKey(ticker))
                    throw new InvalidOperationException(
                        string.Format("Unable to find infos about ticker {0}", ticker));

                value += pts * TickerInfos[ticker].PointPrice;
            }

            return value;
        }
    }
}
