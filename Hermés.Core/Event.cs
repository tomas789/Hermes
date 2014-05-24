using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Hermés.Core
{
    /// <summary>
    /// Representation of event.
    /// </summary>
    public abstract class Event : IComparable<Event>, IEquatable<Event>
    {
        public DateTime Time { protected set; get; }

        protected Event(DateTime time)
        {
            Time = time;
        }

        public int CompareTo(Event other)
        {
            return Time.CompareTo(other.Time);
        }

        public virtual bool Equals(Event other)
        {
            return Time.Equals(other.Time);
        }
    }
}
