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

    #region Portfolio

    /// <summary>
    /// Portfolio is central point which is responsible for executing
    /// all trades suggested by strategies.
    /// </summary>
    public abstract class Portfolio : IEventConsumer
    {
        #region Constructors

        public Portfolio()
        {
            Strategies = new StrategiesHelper(this);
        }

        #endregion

        #region Prerequisities

        /// <summary>
        /// List of strategies that are registered to be potentially
        /// used by the strategy.
        /// </summary>
        public StrategiesHelper Strategies { get; private set; }


        /// <summary>
        /// Broker which will execute orders.
        /// </summary>
        public IBroker Broker;

        #endregion

        #region Portfolio valuation

        /// <summary>
        /// Evaluate overall value of portfolio.
        /// <see cref="Portfolio.GetPortfolioValue()"/> for more details.
        /// </summary>
        public double PortfolioValue
        {
            get
            {
                return GetPortfolioValue();
            }
        }

        #endregion

        /// <summary>
        /// Get value of portfolio including opened positions and
        /// capital in hold.
        /// </summary>
        /// <returns>Portfolio value.</returns>
        protected abstract double GetPortfolioValue();

        /// <summary>
        /// List of all positions taken by portfolio.
        /// </summary>
        protected Dictionary<Ticker, Position> Positions = 
            new Dictionary<Ticker, Position>();

        #region Event dispatching

        /// <summary>
        /// Dispatch event
        /// </summary>
        /// <param name="e">Event.</param>
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

        #endregion

        
    }

    #endregion

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
