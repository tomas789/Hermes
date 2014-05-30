using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermés.Core.Common;

namespace Hermés.Core.Events
{
    /// <summary>
    /// Order event.
    /// </summary>
    public class OrderEvent : Event
    {
        public Position Position { get; private set; }

        public OrderEvent(DateTime time, Position position)
            : base(time)
        {
            Position = position;
        }
    }
}
