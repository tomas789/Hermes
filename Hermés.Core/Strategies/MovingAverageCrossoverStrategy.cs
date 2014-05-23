using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermés.Core.Common;
using Hermés.Core.Events;

namespace Hermés.Core.Strategies
{
    public class MovingAverageCrossoverStrategy : IStrategy
    {
        private DataFeed _market;
        private int _slowPeriod;
        private int _fastPeriod;

        public MovingAverageCrossoverStrategy(DataFeed market, int slowPeriod, int fastPeriod)
        {
            _market = market;
            _slowPeriod = slowPeriod;
            _fastPeriod = fastPeriod;
        }

        public void Initialize(Kernel kernel)
        {
            throw new NotImplementedException();
        }

        public void DispatchEvent(Event e)
        {
            var ts = new TypeSwitch()
                .Case((FillEvent x) => DispatchConcrete(x))
                .Case((MarketEvent x) => DispatchConcrete(x))
                .Case((OrderEvent x) => DispatchConcrete(x))
                .Case((SignalEvent x) => DispatchConcrete(x));

            ts.Switch(e);
        }

        private void DispatchConcrete(FillEvent ev)
        {
        }

        private void DispatchConcrete(MarketEvent ev)
        {
        }

        private void DispatchConcrete(OrderEvent ev)
        {
        }

        private void DispatchConcrete(SignalEvent ev)
        {
        }

        public void Dispose()
        {
        }
    }
}
