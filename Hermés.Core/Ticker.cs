using System;

namespace Hermés.Core
{
    public class Ticker : IEquatable<Ticker>
    {
        public string Name;
        public string Exchange;
        public int Month;
        public int Year;

        public Ticker(string name, string exchange, int month, int year)
        {
            if (!(month >= 1 && month <= 12)) 
                throw new ArgumentException("Month out of range.");

            Name = name;
            Exchange = exchange;
            Month = month;
            Year = year;
        }

        public bool Equals(Ticker other)
        {
            return Name == other.Name &&
                   Exchange == other.Exchange &&
                   Month == other.Month &&
                   Year == other.Year;
        }
    }
}
