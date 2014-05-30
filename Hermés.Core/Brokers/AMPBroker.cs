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

            var position = ev.Position;

            var cost = DefaultCost;
            if (TradeCosts.ContainsKey(ev.Position.Market))
                cost = TradeCosts[ev.Position.Market];

            PriceKind kind;
            switch (ev.Position.Direction) {
                case TradeDirection.Buy:
                    kind = PriceKind.Ask;
                    break;
                case TradeDirection.Sell:
                    kind = PriceKind.Bid;
                    break;
                default:
                    throw new ImpossibleException();
            }

            var fillPrice = ev.Position.Market.CurrentPrice(kind);
            if (fillPrice == null)
                throw new Exception("Failed to get current price.");

            position.Fill(_kernel.WallTime, fillPrice.Close, cost);
            Debug.WriteLine("AMPBroker filling: {0}, Time: {1}", position, _kernel.WallTime);
            _kernel.AddEvent(new FillEvent(_kernel.WallTime, position));
        }

        public void Dispose()
        {
        }
    }
}
