﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hermés.Core.Common;

namespace Hermés.Core
{
    /// <summary>
    /// Source of data from markets.
    /// </summary>
    /// <remarks>
    /// This is especially suitable for offline datafeeds. Expect some
    /// refactoring to happen when any kind of online datafeed will be 
    /// added.
    /// </remarks>
    public abstract class DataFeed : IDisposable
    {
        protected bool Initialized = false;
        public Kernel Kernel;

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// TODO: Replace with background private implementation.
        /// </remarks>
        public CultureInfo CultureInfo = new CultureInfo("en");

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// TODO: Remove.
        /// </remarks>
        /// <returns></returns>
        public CultureInfo getCultureInfo()
        {
            return CultureInfo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// TODO: Remove.
        /// </remarks>
        /// <returns></returns>
        public void setCultureInfo(CultureInfo newCultureInfo)
        {
            if (Initialized)
                // cannot set CultureInfo after Initializing
                throw new InvalidOperationException(); 
            CultureInfo = newCultureInfo;
        }

        private bool _hasBidAsk = false;

        public bool HasBidAsk
        {
            get { return _hasBidAsk; };
            protected set
            {
                if (Initialized)
                    throw new InvalidOperationException("Changing state after initialization"); 
                _hasBidAsk = value;
            };
        }

        public double PointPrice { get; private set; }

        protected DataFeed(double pointPrice)
        {
            PointPrice = pointPrice;
        }

        /// <summary>
        /// Initialize all resources and freeze inner state if any.
        /// </summary>
        /// <param name="kernel"></param>
        public virtual void Initialize(Kernel kernel)
        {
            if (Initialized)
                throw new DoubleInitializationException();
            Initialized = true;
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo;
            Kernel = kernel;
        }

        public abstract void Dispose();

        /// <summary>
        /// Get current price of given value.
        /// </summary>
        /// <remarks>
        /// Returns <value>null</value> if current DataFeed
        /// doesn't have this information.
        /// </remarks>
        /// <param name="market">Ticker to get price of.</param>
        /// <param name="priceKind">Kind of price.</param>
        /// <returns>Current price group.</returns>
        public abstract PriceGroup CurrentPrice(DataFeed market, PriceKind priceKind);

        public abstract PriceGroup GetHistoricalPriceGroup(int lookbackPeriod);
    }
}
