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
        public override void DispatchConcrete(Events.FillEvent e)
        {
            throw new NotImplementedException();
        }

        public override void DispatchConcrete(Events.MarketEvent e)
        {
            throw new NotImplementedException();
        }

        public override void DispatchConcrete(Events.OrderEvent e)
        {
            throw new NotImplementedException();
        }

        public override void DispatchConcrete(Events.SignalEvent e)
        {
            if (e.Kind == SignalKind.Hold)
                return;

            OrderDirection direction;
            switch (e.Kind)
            {
                case SignalKind.Buy:
                    direction = OrderDirection.Buy;
                    break;
                case SignalKind.Hold:
                    direction = OrderDirection.Sell;
                    break;
                default:
                    throw new ImpossibleException();
            }

            var order = new OrderEvent(e.Ticker, direction, OrderKind.Market);
        }
    }
}
