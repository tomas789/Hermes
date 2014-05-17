using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermés.Core.Common;

namespace Hermés.Core
{
    public abstract class DataFeed : IDisposable
    {
        public Kernel Kernel;

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
        /// <returns>Current price.</returns>
        public abstract double? CurrentPrice(Ticker ticker, PriceKind priceKind);
    }
}
