using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermés.Core;
using Hermés.Core.Common;
using Hermés.Core.Events;

namespace Hermés.Core
{
    public abstract class Portfolio : IEventConsumer
    {
        public Portfolio()
        {
            Strategies = new StrategiesHelper(this);
        }

        public StrategiesHelper Strategies { get; private set; }

        public IBroker Broker;

        public double PortfolioValue
        {
            get
            {
                return GetPortfolioValue();
            }
        }

        protected Dictionary<Ticker, Position> Positions = 
            new Dictionary<Ticker, Position>(); 

        public void DispatchEvent(Event e)
        {
            var ts = new TypeSwitch()
                .Case((FillEvent x) => DispatchConcrete(x))
                .Case((MarketEvent x) => DispatchConcrete(x))
                .Case((OrderEvent x) => DispatchConcrete(x))
                .Case((SignalEvent x) => DispatchConcrete(x));

            ts.Switch(e);
        }

        public abstract void DispatchConcrete(FillEvent e);
        public abstract void DispatchConcrete(MarketEvent e);
        public abstract void DispatchConcrete(OrderEvent e);
        public abstract void DispatchConcrete(SignalEvent e);

        protected abstract double GetPortfolioValue();
    }

    #region StrategiesHelper

    /// <summary>
    /// This helper allows to have indexer on member with 
    /// comfortable syntax.
    /// </summary>
    public class StrategiesHelper
    {
        private readonly List<IStrategy> _strategies = 
            new List<IStrategy>();

        private readonly Portfolio _portfolio;

        public StrategiesHelper(Portfolio portfolio)
        {
            _portfolio = portfolio;
        }

        public IStrategy this[int i]
        {
            get { return _strategies[i]; }
        }

        public void AddStrategy(IStrategy strategy)
        {
            strategy.Initialize(_portfolio);
            _strategies.Add(strategy);
        }
    }

#endregion
}
