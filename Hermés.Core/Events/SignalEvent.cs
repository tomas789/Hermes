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

    public class SignalEvent : Event
    {
        public readonly Ticker Ticker;
        public readonly SignalKind Kind;

        SignalEvent(Ticker ticker, SignalKind kind) 
            : base(DateTime.Now)
        {
            Ticker = ticker;
            Kind = kind;
        }

        SignalEvent(DateTime time, Ticker ticker, SignalKind kind)
            : base(time)
        {
            Ticker = ticker;
            Kind = kind;
        }
    }
}
