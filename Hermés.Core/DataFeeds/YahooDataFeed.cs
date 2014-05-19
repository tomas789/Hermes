using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hermés.Core.Events;
using Hermés.Core.Common;

namespace Hermés.Core.DataFeeds
{
    
    class YahooDataFeed : DataFeed
    {
        private bool _initialized = false;
        private readonly Dictionary<string, string> _header =
            new Dictionary<string, string>();
        private readonly Dictionary<string,int> _dataFormat =
            new Dictionary<string, int>();
        private readonly List<MarketEvent> _data = 
            new List<MarketEvent>();

        private int _timeZoneOffset;
        private double _interval;

        public ulong Count { get; set; }

        YahooDataFeed()
        {
            Count = 0;
        }

        public void Initialize(string adressOfFile, Kernel kernel)
        {
            if (_initialized)
                throw new DoubleInitializationException();

            base.Initialize(kernel);

            GetData(adressOfFile);
            _initialized = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// TODO: check for first body line validation of time
        /// </remarks>
        /// <param name="adressOfFile"></param>
        /// 
        private void GetData(string adressOfFile)
        {
            // TODO: Dispose
            var file = new StreamReader(adressOfFile);
            string line;

            // header
            {
                // TODO: 
                while ((line = file.ReadLine()) != null)
                {
                    if (! HeaderCheck(line))
                        break;
                }

                if (line == null)
                    throw new InvalidDataException();

                // constrains for working datafeed
                string[] vitalData = { "COLUMNS", "INTERVAL", "EXCHANGE" };
                if (vitalData.Any(w => !_header.ContainsKey(_header[w])))
                    throw new InvalidDataException();

                // all possible deliminators for column names 
                char[] deliminators = { ',' };
                var words = (_header["COLUMNS"]).Split(deliminators);
                for (var i = 0; i < words.Length; ++i)
                    _dataFormat.Add(words[i],i);
            }

            // header additional setting
            {
                _timeZoneOffset = !_header.ContainsKey(_header["TIMEZONE_OFFSET"]) ? 
                    0 : Convert.ToInt32(_header["TIMEZONE_OFFSET"]);

                _interval = Convert.ToDouble(_header["INTERVAL"]);
            }

            // body
            {
                // all possible deliminators for columns 
                char[] deliminators = { ',' };
                var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

                // line comes with first value from trying to get header
                while (line != null)
                {
                    ++Count;
                    // empty line should not occur in yahoo data feed
                    if (line.Length == 0)
                        throw new ImpossibleException();

                    var fields = line.Split(deliminators);
                    var marketEvent = BodyLineSetter(fields,dt);
                    _data.Add(marketEvent);

                    line = file.ReadLine();
                }
            }

        }

        private MarketEvent BodyLineSetter(string[] parsedLine, DateTime dt)
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
                if (timeString.Length < 2)
                    throw new ImpossibleException(); // wrong data file
                string pureTime = timeString.Substring(1);
                if (! IsValidNumber(pureTime))
                    throw new ImpossibleException(); // wrong data file
                dt = UnixTimeStampToDateTime(
                    Convert.ToDouble(pureTime)
                    ).AddMinutes(_timeZoneOffset);
                time = dt;
            }
            else 
            {
                if (! IsValidNumber(timeString))
                    throw new ImpossibleException(); // wrong data file
                time = dt.AddSeconds(
                    _interval*Convert.ToDouble(timeString));
            }

            var priceGroup = PriceGroupBuilder(parsedLine);

            var marketEvent = new MarketEvent(time, priceGroup);
            return marketEvent;
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
            if (_dataFormat.ContainsKey("part"))
            {
                var res = parsedLine[_dataFormat["part"]];
                if (IsValidNumber(res))
                    obj = Convert.ToDouble(res);
            }
            else
                obj = null;
        }

        // not implemented yet
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

        // TODO: Deliminator :)
        // TODO: Consider using string.Split
        // header must be set as part=VALUE, where = is just some deliminator
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

            // i will parse string as part:deliminator:rest, 
            // where deliminator is any character and ':' are missing
            var rest = line.Substring(part.Length + 1);
            
            _header.Add(part,rest);
            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// here have to be specified all part of header ... sad but true
        /// TODO: Convert to single big conjunction.
        /// </remarks>
        /// <param name="line"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// TODO: Remove NotImplementedException if not required
        /// </remarks>
        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override PriceGroup CurrentPrice(Ticker ticker, PriceKind priceKind)
        {
            throw new NotImplementedException();
        }
    }
}
