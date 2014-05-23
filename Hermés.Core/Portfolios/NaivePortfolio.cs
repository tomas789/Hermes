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
            Debug.WriteLine("Portfolio got FillEvent: {0}", ev);
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
            throw new NotImplementedException();
        }
    }
}
