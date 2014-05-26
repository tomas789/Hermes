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
            Debug.WriteLine("Broker got OrderEvent: {0}, Time: {1}", ev, _kernel.WallTime);

            if (_kernel == null)
                throw new ArgumentNullException();

            var cost = DefaultCost;
            if (TradeCosts.ContainsKey(ev.Market))
                cost = TradeCosts[ev.Market];

            PriceKind kind;
            switch (ev.Direction) {
                case TradeDirection.Buy:
                    kind = PriceKind.Ask;
                    break;
                case TradeDirection.Sell:
                    kind = PriceKind.Bid;
                    break;
                default:
                    throw new ImpossibleException();
            }

            var fillPrice = ev.Market.CurrentPrice(kind);
            if (fillPrice == null)
                throw new Exception("Failed to get current price.");

            

            var fill = new FillEvent(_kernel.WallTime, ev.Market, ev.Direction, ev.Price, fillPrice.Close, cost, 1);
            Debug.WriteLine("AMPBroker filling: {0}, Time: {1}", fill, _kernel.WallTime);
            _kernel.AddEvent(fill);
        }

        public void Dispose()
        {
        }
    }
}
