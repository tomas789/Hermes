using System;

namespace Hermés.Core
{
    /// <summary>
    /// Description of contract.
    /// </summary>
    /// <remarks>
    /// User is responsible for coherent naming. This isn't doing anything
    /// like close-matching tickers.
    /// </remarks>
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

        /// <summary>
        /// Compare Tickers for exact match.
        /// </summary>
        /// <param name="other">Ticker to compare with.</param>
        /// <returns>True iff this and <paramref name="other"/> are matching</returns>
        public bool Equals(Ticker other)
        {
            return Name == other.Name &&
                   Exchange == other.Exchange &&
                   Month == other.Month &&
                   Year == other.Year;
        }
    }
}
