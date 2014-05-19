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

        protected Portfolio()
        {
            Strategies = new StrategiesHelper(this);
            DataFeeds = new DataFeedHelper(this);
            Kernel = new Kernel();
        }

        #endregion

        /// <summary>
        /// Freeze current state and get ready to run simulation.
        /// </summary>
        /// <remarks>
        /// Calling this function causes correct initialization of
        /// associated parts like broker, strategies, datafeed and so on.
        /// </remarks>
        public void Initialize()
        {
            Strategies.Initialize(Kernel);
            DataFeeds.Initialize(Kernel);
            Broker.Initialize(Kernel);
        }

        #region Prerequisities

        public Kernel Kernel { get; private set; }

        /// <summary>
        /// Time of the latest event.
        /// </summary>
        public DateTime WallTime
        {
            get { return Kernel.WallTime; }
            set { Kernel.WallTime = value;  }
        }

        /// <summary>
        /// List of strategies that are registered to be potentially
        /// used by the strategy.
        /// </summary>
        public StrategiesHelper Strategies { get; private set; }

        public DataFeedHelper DataFeeds { get; private set; }


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
        /// <remarks>
        /// Deprecated: Use GetPortfolioValue instead.
        /// </remarks>
        public double PortfolioValue
        {
            get { return GetPortfolioValue(); }
        }

        #endregion

        /// <summary>
        /// Get value of portfolio including opened positions and
        /// capital in hold.
        /// </summary>
        /// <returns>Portfolio value.</returns>
        protected abstract double GetPortfolioValue();

        /// <summary>
        /// Set of all positions executed by portfolio.
        /// </summary>
        protected HashSet<Position> Positions = new HashSet<Position>();

        public readonly Dictionary<Ticker, TickerInfo> TickerInfos = 
            new Dictionary<Ticker, TickerInfo>(); 

        #region Event dispatching

        /// <summary>
        /// Dispatch event
        /// </summary>
        /// <param name="e">Event.</param>
        public void DispatchEvent(Event e)
        {
            if (e.Time > WallTime)
                Kernel.WallTime = e.Time;

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
        private bool _initialized = false;

        private readonly List<IStrategy> _strategies =
            new List<IStrategy>();

        private readonly Portfolio _portfolio;

        public StrategiesHelper(Portfolio portfolio)
        {
            _portfolio = portfolio;
        }

        internal void Initialize(Kernel kernel)
        {
            if (_initialized)
                throw new DoubleInitializationException();

            _initialized = true;
            foreach (var strategy in _strategies)
                strategy.Initialize(kernel);
        }

        public IStrategy this[int i]
        {
            get { return _strategies[i]; }
        }

        public void AddStrategy(IStrategy strategy)
        {
            if (_initialized)
                throw new InvalidOperationException(
                    "Trying to add strategy after initialization.");

            _strategies.Add(strategy);
        }
    }

    #endregion

    #region DataFeedHelper

    /// <summary>
    /// This helper allows to have indexer on member with 
    /// comfortable syntax.
    /// </summary>
    public class DataFeedHelper : IEnumerable<DataFeed>
    {
        private bool _initialized = false;

        private readonly List<DataFeed> _dataFeeds =
            new List<DataFeed>();

        private readonly Portfolio _portfolio;

        public DataFeedHelper(Portfolio portfolio)
        {
            _portfolio = portfolio;
        }

        internal void Initialize(Kernel kernel)
        {
            if (_initialized)
                throw new DoubleInitializationException();

            _initialized = true;
            foreach (var strategy in _dataFeeds)
                strategy.Initialize(kernel);
        }

        public DataFeed this[int i]
        {
            get { return _dataFeeds[i]; }
        }

        public void AddDataFeed(DataFeed dataFeed)
        {
            if (_initialized)
                throw new InvalidOperationException(
                    "Trying to add strategy after initialization.");

            _dataFeeds.Add(dataFeed);
        }

        public IEnumerator<DataFeed> GetEnumerator()
        {
            return _dataFeeds.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _dataFeeds.GetEnumerator();
        }
    }

    #endregion
}
