using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Hermés.Core;
using Hermés.Core.Events;
using Hermés.Core.Common;

namespace Hermés.Core.Strategies
{
    public class BuyAndHoldStrategy : IStrategy
    {
        private readonly List<DataFeed> _dataFeeds;
        private readonly HashSet<DataFeed> _generatedSignals = 
            new HashSet<DataFeed>();

        public Kernel Kernel;
        private bool _initialized = false;

        public BuyAndHoldStrategy()
        {
            Debug.WriteLine("BuyAndHoldStrategy is default initialized.");
            _dataFeeds = new List<DataFeed>();
        }

        public BuyAndHoldStrategy(List<DataFeed> dataFeeds)
        {
            _dataFeeds = dataFeeds;
        }

        public void Initialize(Kernel kernel)
        {
            if (_initialized)
                throw new DoubleInitializationException();
            _initialized = true;

            Kernel = kernel;
        }

        public void DispatchEvent(Event e)
        {
            var ts = new TypeSwitch()
                .Case((MarketEvent x) => DispatchConcrete(x));

            ts.Switch(e);
        }

        private void DispatchConcrete(MarketEvent ev)
        {
            if (_generatedSignals.Contains(ev.Market) || (_dataFeeds.Count != 0 && !_dataFeeds.Contains(ev.Market)))
                return;

            var signal = new SignalEvent(Kernel.WallTime, ev.Market, SignalKind.Buy);
            Kernel.AddEvent(signal);
            _generatedSignals.Add(ev.Market);
            Debug.WriteLine("Buy and hold signal: {0}, Time: {1}", signal, Kernel.WallTime);
        }

        public void Dispose()
        {
        }
    }
}
