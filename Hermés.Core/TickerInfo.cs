using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hermés.Core
{
    [Serializable]
    public class TickerInfo
    {
        public Ticker Ticker;

        /// <summary>
        /// Price of one point.
        /// </summary>
        public double PointPrice;

        /// <summary>
        /// Minimum fluctuation.
        /// </summary>
        public double TickSize;

        public TickerInfo(Ticker ticker)
        {
            Ticker = ticker;
        }
    }
}
