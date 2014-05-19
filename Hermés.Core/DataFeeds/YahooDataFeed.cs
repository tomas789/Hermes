using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hermés.Core.Events;
using Hermés.Core.Common;

namespace Hermés.Core.DataFeeds
{
    
    // TODO: GetData(TextReader), IsValidNumber(string)
    // TODO: Tests
    class YahooDataFeed : DataFeed
    {
        private bool _initialized = false;
        private readonly Dictionary<string, string> _header =
            new Dictionary<string, string>();
        private readonly Dictionary<string,int> _dataFormat =
            new Dictionary<string, int>();
        private readonly SortedDictionary<DateTime,MarketEvent> _data = 
            new SortedDictionary<DateTime, MarketEvent>();
        private string _fileName;
        private TextReader _textReader;
        private Ticker _ticker;

        private Ticker _ticker;

        private int _timeZoneOffset;
        private double _interval;

        public int Count { get; set; }


        public YahooDataFeed(Ticker ticker, string fileName)
        {
            _ticker = ticker;
            _fileName = fileName;
            Count = 0;
        }


        public YahooDataFeed(Ticker ticker, TextReader textReader)
        {
            _ticker = ticker;
            _textReader = textReader;
            Count = 0;
        }


        public YahooDataFeed(Ticker ticker)
        {
            _ticker = ticker;
            Count = 0;
        }
        

        public void Initialize(Kernel kernel)
        {
            if (_initialized)
                throw new DoubleInitializationException();
            _initialized = true;

            base.Initialize(kernel);

            if (_fileName != null)
                GetData(_fileName);
            else 
                GetData(_textReader);
        }


        private void GetData(string fileName)
        {
            using (var file = new StreamReader(fileName))
            {
                string line = null;

                ReadHeader(ref line, file);

                HeaderAdditionalSetting();

                ReadBody(ref line, file);
            }
        }

        // TODO: make it better
        private void GetData(TextReader textReader)
        {
            string line = null;

            ReadHeader(ref line, (StreamReader)textReader);

            HeaderAdditionalSetting();

            ReadBody(ref line, (StreamReader)textReader);
        }


        /// <summary>
        /// Read header lines from file
        /// </summary>
        private void ReadHeader(ref string line, StreamReader file)
        {
            while ((line = file.ReadLine()) != null)
            {
                if (!HeaderCheck(line))
                    break;
            }

            if (line == null)
                throw new InvalidDataException();

            // constrains for working datafeed
            string[] vitalData = { "COLUMNS", "INTERVAL", "EXCHANGE" };
            if (vitalData.Any(w => !_header.ContainsKey(_header[w])))
                throw new InvalidDataException();

            // all possible deliminators for column names 
            char[] delimiters = { ',' };
            var words = (_header["COLUMNS"]).Split(delimiters);
            for (var i = 0; i < words.Length; ++i)
                _dataFormat.Add(words[i], i);
        }

        private void HeaderAdditionalSetting()
        {
            _timeZoneOffset = !_header.ContainsKey(_header["TIMEZONE_OFFSET"]) ?
                0 : Convert.ToInt32(_header["TIMEZONE_OFFSET"]);

            _interval = Convert.ToDouble(_header["INTERVAL"]);
        }

        private MarketEvent BodyLineSetter(string[] parsedLine, DateTime dt, 
                                           ref bool timeSet)
        {
            if (parsedLine.Length != _dataFormat.Count)
                throw new ImpossibleException();

            // setting date
            DateTime time;
            string timeString = parsedLine[_dataFormat["DATE"]];
            if (timeString.Length == 0)
                throw new ImpossibleException(); // wrong data file

            if (timeString[0] == 'a')
            {
                if (! timeSet)
                    timeSet = true;
                if (timeString.Length < 2)
                    throw new ImpossibleException(); // wrong data file
                var pureTime = timeString.Substring(1);
                if (! IsValidNumber(pureTime))
                    throw new ImpossibleException(); // wrong data file
                dt = UnixTimeStampToDateTime(
                    Convert.ToDouble(pureTime)
                    ).AddMinutes(_timeZoneOffset);
                time = dt;
            }
            else 
            {
                if (! timeSet)
                    throw new ImpossibleException(); // not time set so far
                if (! IsValidNumber(timeString))
                    throw new ImpossibleException(); // wrong data file
                time = dt.AddSeconds(
                    _interval*Convert.ToDouble(timeString));
            }

            var priceGroup = PriceGroupBuilder(parsedLine);

            var marketEvent = new MarketEvent(_ticker, time, priceGroup);
            return marketEvent;
        }

        private void ReadBody(ref string line, StreamReader file)
        {
            var timeSet = false;

            // all possible deliminators for columns 
            char[] delimiters = { ',' };
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            // line comes with first value from trying to get header
            while (line != null)
            {
                ++Count;
                // empty line should not occur in yahoo data feed
                if (line.Length == 0)
                    throw new ImpossibleException();

                var fields = line.Split(delimiters);
                var marketEvent = BodyLineSetter(fields, dt, ref timeSet);
                _data.Add(marketEvent.Time, marketEvent);

                line = file.ReadLine();
            }
        }

        private PriceGroup PriceGroupBuilder(string[] parsedLine)
        {
            var pricegroup = new PriceGroup();

            PriceGroupSetter(parsedLine, "CLOSE", ref pricegroup.Close);
            PriceGroupSetter(parsedLine, "OPEN", ref pricegroup.Open);
            // haven't found original open interest look
            PriceGroupSetter(parsedLine, "OPEN_INTEREST", ref pricegroup.OpenInterenst); 
            PriceGroupSetter(parsedLine, "LOW", ref pricegroup.Low);
            PriceGroupSetter(parsedLine, "HIGH", ref pricegroup.High);
            PriceGroupSetter(parsedLine, "VOLUME", ref pricegroup.Volume);

            return pricegroup;
        }

        private void PriceGroupSetter(string[] parsedLine, string part, ref double? obj)
        {
            if (_dataFormat.ContainsKey(part))
            {
                var res = parsedLine[_dataFormat[part]];
                if (IsValidNumber(res))
                    obj = Convert.ToDouble(res);
                else 
                    throw new ImpossibleException();
            }
            else
                obj = null;
        }

        // TODO: implement method
        private bool IsValidNumber(string number)
        {
            return true;
        }

        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dt = dt.AddSeconds(unixTimeStamp).ToLocalTime();
            return dt;
        }


        // header must be set as part=VALUE, where = is just some delimiter
        private bool HeaderSetter(string line, string part)
        {
            // check, if its possible to be this part of header
            if (line.Length < part.Length)
                return false;

            // check, if its exactly what i have been looking for
            if (line.Substring(0, part.Length) != part)
                return false;

            // check, if there is some closer specification or just empty value
            if (line.Length < part.Length + 2)
            {
                _header.Add(part,"");
                return true;
            }

            // i will split string as part, one char, rest 
            var rest = line.Substring(part.Length + 1);
            
            _header.Add(part,rest);
            return true;
        }


        private bool HeaderCheck(string line)
        {
            if (HeaderSetter(line, "EXCHANGE"))
                return true;
            if (HeaderSetter(line, "MARKET_OPEN_MINUTE"))
                return true;
            if (HeaderSetter(line, "MARKET_CLOSE_MINUTE"))
                return true;
            if (HeaderSetter(line, "INTERVAL"))
                return true;
            if (HeaderSetter(line, "COLUMNS"))            
                return true;
            if (HeaderSetter(line, "DATA"))
                return true;
            if (HeaderSetter(line, "TIMEZONE_OFFSET"))
                return true;
            
            return false;
        }


        public override void Dispose()
        {
        }

        public override PriceGroup CurrentPrice(Ticker ticker, PriceKind priceKind)
        {
            if (PriceKind.Unspecified != priceKind)
                return null;
            if (ticker.Equals(_ticker))
                if (_data.ContainsKey(base.Kernel.WallTime))
                    return _data[base.Kernel.WallTime].Price;
            return null;
        }

        public override PriceGroup GetHistoricalPriceGroup(int lookbackPeriod)
        {
            throw new NotImplementedException();
        }
    }
}
