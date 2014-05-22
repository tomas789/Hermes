﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public Kernel Kernel;

        /// <summary>
        /// Initialize all resources and freeze inner state if any.
        /// </summary>
        /// <param name="kernel"></param>
        public virtual void Initialize(Kernel kernel)
        {
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
        /// <param name="ticker">Ticker to get price of.</param>
        /// <param name="priceKind">Kind of price.</param>
        /// <returns>Current price group.</returns>
        public abstract PriceGroup CurrentPrice(Ticker ticker, PriceKind priceKind);

        public abstract PriceGroup GetHistoricalPriceGroup(int lookbackPeriod);
    }
}
