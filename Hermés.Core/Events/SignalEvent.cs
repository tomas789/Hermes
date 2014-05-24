using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hermés.Core.Events
{
    public enum SignalKind
    {
        Buy, Sell, Hold
    }

    /// <summary>
    /// Signal geenrated by Strategy.
    /// </summary>
    public class SignalEvent : Event
    {
        public readonly DataFeed Market;
        public readonly SignalKind Kind;

        public SignalEvent(DateTime time, DataFeed market, SignalKind kind)
            : base(time)
        {
            Market = market;
            Kind = kind;
        }
    }
}
