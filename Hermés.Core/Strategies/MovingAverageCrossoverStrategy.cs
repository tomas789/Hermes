using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermés.Core.Common;
using Hermés.Core.Events;

namespace Hermés.Core.Strategies
{
    class MovingAverageCrossoverStrategy : IStrategy
    {
        private Ticker _ticker;
        private int _slowPeriod;
        private int _fastPeriod;

        public MovingAverageCrossoverStrategy(Ticker ticker, int slowPeriod, int fastPeriod)
        {
            _ticker = ticker;
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
