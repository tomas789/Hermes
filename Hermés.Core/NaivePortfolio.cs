using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermés.Core.Common;
using Hermés.Core.Events;

namespace Hermés.Core
{
    class NaivePortfolio : Portfolio
    {
        private readonly List<FillEvent> _fillEvents =
            new List<FillEvent>();

        private double _initialCapital;

        public NaivePortfolio(Kernel kernel, double initialCapital)
            : base(kernel)
        {
            _initialCapital = initialCapital;
        }

        public override void DispatchConcrete(FillEvent ev)
        {
            _fillEvents.Add(ev);
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
                case SignalKind.Hold:
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
            foreach (var position in Positions.Values)
            {
                if (!sizeHolded.ContainsKey(position.Ticker))
                    sizeHolded.Add(position.Ticker, 0);

                double change = 0;
                switch (position.Direction)
                {
                    case TradeDirection.Buy:
                        change = position.Size;
                        break;
                    case TradeDirection.Sell:
                        change = (-1)*position.Size;
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
