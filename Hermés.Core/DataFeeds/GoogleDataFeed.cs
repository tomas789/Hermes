using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hermés.Core.Events;
using Hermés.Core.Common;

namespace Hermés.Core.DataFeeds
{
    public class GoogleDataFeed : DataFeed
    {
        private readonly SortedDictionary<DateTime, PriceGroup> _data =
            new SortedDictionary<DateTime, PriceGroup>();

        private readonly string _fileName;
        private readonly TextReader _inputFileReader;

        public int Count
        {
            get { return _data.Count; }
        }

        public GoogleDataFeed(string fileName, double pointPrice)
            : base(pointPrice)
        {
            _fileName = fileName;
        }

        public GoogleDataFeed(TextReader textReader, double pointPrice)
            : base(pointPrice)
        {
            _inputFileReader = textReader;
        }

        public override void Initialize(Kernel kernel)
        {
            base.Initialize(kernel);

            if (_fileName == null && _inputFileReader == null)
                return;

            if (_fileName != null)
                using (var inFile = new StreamReader(_fileName))
                    ParseInputFile(inFile);
            else
                ParseInputFile(_inputFileReader);
        }

        private void ParseInputFile(TextReader inFile)
        {
            var headers = new List<string>();
            var data = new List<string>();

            string line;
            var readingHeaders = true;

            if ((line = inFile.ReadLine()) != null)
                headers.Add(line);

            while ((line = inFile.ReadLine()) != null)
            {
                if (line.Contains("="))
                {
                    if (!readingHeaders)
                        throw new InvalidDataException("Mixing headers and data");

                    headers.Add(line);
                }
                else
                {
                    if (readingHeaders)
                        readingHeaders = false;

                    data.Add(line);
                }
            }

            var headerInfo = ParseYahooHeaderInfo(headers);
            ParseDataPart(data, headerInfo);
            EmitEvents();
        }

        private GoogleHeaderInfo ParseYahooHeaderInfo(IList<string> headers)
        {
            var headerInfo = new GoogleHeaderInfo();

            if (headers.Count == 0)
                throw new InvalidDataException("Invalid headers format.");

            var exchanheHeader = headers[0];
            if (!exchanheHeader.StartsWith("EXCHANGE%3D"))
                throw new InvalidDataException("Invalid headers format.");

            headerInfo.Exchange = exchanheHeader.Remove(0, "EXCHANGE%3D".Length);

            foreach (var parts in headers.Skip(1).Select(header => header.Split(new[] {'='})))
            {
                if (parts.Length != 2)
                    throw new InvalidDataException("Invalid headers format.");

                switch (parts[0])
                {
                    case "MARKET_OPEN_MINUTE":
                        break;
                    case "MARKET_CLOSE_MINUTE":
                        break;
                    case "INTERVAL":
                        if (!int.TryParse(parts[1], out headerInfo.Interval))
                            throw new InvalidDataException("Invalid headers format.");
                        break;
                    case "COLUMNS":
                        headerInfo.Columns.AddRange(parts[1].Split(new[] {','}));
                        if (!headerInfo.Columns.Contains("DATE"))
                            throw new InvalidDataException("Invalid headers format.");
                        break;
                    case "DATA":
                        break;
                    case "TIMEZONE_OFFSET":
                        if (!int.TryParse(parts[1], out headerInfo.TimezoneOffset))
                            throw new InvalidDataException("Invalid headers format.");
                        break;
                    default:
                        throw new InvalidDataException("Invalid headers format.");
                }
            }

            return headerInfo;
        }

        /// <summary>
        /// Parse data part of input file.
        /// </summary>
        /// <remarks>
        /// Throws exception if anything unexpected happend.
        /// </remarks>
        /// <param name="data"></param>
        /// <param name="headerInfo"></param>
        private void ParseDataPart(IEnumerable<string> data, GoogleHeaderInfo headerInfo)
        {
            var absoluteTimestamp = -1;

            foreach (var parts in data.Select(row => row.Split(new[] {','})))
            {
                if (parts.Length != headerInfo.Columns.Count)
                    throw new InvalidDataException("Invalid headers format.");

                var time = new DateTime();
                var group = new PriceGroup();

                for (var i = 0; i < headerInfo.Columns.Count; ++i)
                {
                    double value;
                    switch (headerInfo.Columns[i])
                    {
                        case "DATE":
                            if (parts[i][0] == 'a')
                            {
                                if (!int.TryParse(parts[i].Substring(1), out absoluteTimestamp))
                                    throw new InvalidDataException("Invalid headers format.");

                                time = headerInfo.GetDateTimeFromTimestamp(absoluteTimestamp);
                            }
                            else
                            {
                                int relativeTimestamp;
                                if (!int.TryParse(parts[i], out relativeTimestamp))
                                    throw new InvalidDataException("Invalid headers format.");

                                time = headerInfo.GetDateTimeFromTimestamp(
                                    absoluteTimestamp + relativeTimestamp);
                            }

                            break;
                        case "CLOSE":
                            if (!double.TryParse(parts[i], out value))
                                throw new InvalidDataException("Invalid headers format.");
                            group.Close = value;
                            break;
                        case "HIGH":
                            if (!double.TryParse(parts[i], out value))
                                throw new InvalidDataException("Invalid headers format.");
                            group.High = value;
                            break;
                        case "LOW":
                            if (!double.TryParse(parts[i], out value))
                                throw new InvalidDataException("Invalid headers format.");
                            group.Low = value;
                            break;
                        case "OPEN":
                            if (!double.TryParse(parts[i], out value))
                                throw new InvalidDataException("Invalid headers format.");
                            group.OpenInterenst = value;
                            break;
                        case "VOLUME":
                            if (!double.TryParse(parts[i], out value))
                                throw new InvalidDataException("Invalid headers format.");
                            group.Volume = value;
                            break;
                    }
                }

                _data.Add(time, group);
            }
        }

        /// <summary>
        /// Emit data from this DataFeed to Kernel.
        /// </summary>
        private void EmitEvents()
        {
            foreach (var item in _data)
            {
                var ev = new MarketEvent(this, item.Key, item.Value);
                Kernel.AddEvent(ev);

            }
        }

        public override void Dispose()
        {
        }

        public override PriceGroup CurrentPrice(DataFeed market, PriceKind priceKind)
        {
            Debug.WriteLine("GoogleDataFeed asked for CurrentPrice: Market: {0}; PriceKind: {1}", market, priceKind);
            if (PriceKind.Unspecified != priceKind || !market.Equals(this) || _data.Count == 0)
                return null;

            PriceGroup group;
            if (_data.TryGetValue(Kernel.WallTime, out group))
                return group;

            var closestTime = _data.Aggregate(DateTime.MinValue,
                (time, item) => (item.Key <= Kernel.WallTime && item.Key > time) ? item.Key : time);
            return closestTime == DateTime.MinValue ? null : _data[closestTime];
        }

        public override PriceGroup GetHistoricalPriceGroup(int lookbackPeriod)
        {
            throw new NotImplementedException();
        }

        private class GoogleHeaderInfo
        {
            public string Exchange;
            public int Interval;
            public int TimezoneOffset;
            public List<string> Columns = new List<string>();

            public DateTime GetDateTimeFromTimestamp(int timestamp)
            {
                var basetime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                return basetime.AddSeconds(timestamp - 60*TimezoneOffset);
            }
        }
    }
}
