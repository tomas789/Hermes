using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hermés.Core.Events
{
    public class FillEvent : Event
    {
        public FillEvent() : base(DateTime.Now)
        {
            
        }
    }
}
