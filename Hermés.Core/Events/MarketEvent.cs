using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hermés.Core.Events
{
    public class MarketEvent : Event
    {
        public double? O;
        public double? H;
        public double? L;
        public double? C;
        public double? V;
        public double? I;

        public Ticker Ticker;

        public MarketEvent() : base(DateTime.Now)
        {
            
        }
    }
}
