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

        public NaivePortfolio(double initialCapital)
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
        }

        protected override double GetPortfolioValue()
        {
            throw new NotImplementedException();
        }
    }
}
