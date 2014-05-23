using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
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
            Debug.WriteLine("Broker got OrderEvent: {0}", ev);
            if (_kernel == null)
                throw new ArgumentNullException();

            var cost = DefaultCost;
            if (TradeCosts.ContainsKey(ev.Market))
                cost = TradeCosts[ev.Market];

            PriceGroup group = null;
            if (double.IsNaN(ev.Price))
            {
                switch (ev.Direction)
                {
                    case TradeDirection.Buy:
                        group = ev.Market.CurrentPrice(ev.Market, PriceKind.Ask);
                        break;
                    case TradeDirection.Sell:
                        group = ev.Market.CurrentPrice(ev.Market, PriceKind.Bid);
                        break;
                    default:
                        throw new ImpossibleException();
                }

                if (group == null)
                    group = ev.Market.CurrentPrice(ev.Market, PriceKind.Unspecified);

                if (group == null)
                    throw new InvalidOperationException("Trying to buy without price knowledge.");
            }

            var fill = new FillEvent(ev.Market, ev.Direction, ev.Price, group != null ? group.Close : ev.Price, cost, 1);
            _kernel.AddEvent(fill);
        }

        public void Dispose()
        {
        }
    }
}
