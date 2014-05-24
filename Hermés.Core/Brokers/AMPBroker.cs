using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermés.Core.Common;
using Hermés.Core.Events;

namespace Hermés.Core.Brokers
{
    public class AMPBroker : IBroker
    {
        private Kernel _kernel = null;

        public double DefaultCost = 2;
        public readonly Dictionary<DataFeed, double> TradeCosts = 
            new Dictionary<DataFeed, double>(); 

        public void Initialize(Kernel kernel)
        {
            _kernel = kernel;
        }

        public void DispatchEvent(Event e)
        {
            var ts = new TypeSwitch()
                .Case((OrderEvent x) => DispatchConcrete(x));

            ts.Switch(e);
        }

        private void DispatchConcrete(OrderEvent ev)
        {
            if (_kernel == null)
                throw new ArgumentNullException();

            var cost = DefaultCost;
            if (TradeCosts.ContainsKey(ev.Market))
                cost = TradeCosts[ev.Market];

            var fill = new FillEvent(ev.Market, ev.Direction, ev.Price, ev.Price, cost, 1);
            _kernel.AddEvent(fill);
        }

        public void Dispose()
        {
        }
    }
}
