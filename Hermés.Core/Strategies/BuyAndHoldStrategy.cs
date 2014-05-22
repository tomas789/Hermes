using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermés.Core;
using Hermés.Core.Events;

namespace Hermés.Core.Strategies
{
    class BuyAndHoldStrategy : IStrategy
    {
        private readonly Ticker _ticker;

        public BuyAndHoldStrategy(Ticker ticker)
        {
            _ticker = ticker;
        }

        public void Initialize(Kernel kernel)
        {
            var buyEvent = new SignalEvent(_ticker, SignalKind.Buy);
            kernel.AddEvent(buyEvent);
        }

        public void DispatchEvent(Event e)
        {
        }

        public void Dispose()
        {
        }
    }
}
