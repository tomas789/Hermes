using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hermés.Core.Events
{
    public class MarketEvent : Event
    {
        public MarketEvent() : base(DateTime.Now)
        {
            
        }
    }
}
