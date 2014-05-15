using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Hermés.Core.DataFeed
{
    
    class YahooDataFeed
    {
        private Dictionary<string, string> header;
        private List<string> data_format;

        private bool Header_Setter(string line, string part)
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
                header.Add(part,"");
                return true;
            }

            // i will parse string as part,deliminator,rest, where deliminator is any character
            var rest = line.Substring(part.Length + 1);
            
            header.Add(part,rest);
            return true;
        }

        private bool Header_Part_Check(string line)
        {
            if (Header_Setter(line, "EXCHANGE"))
                return true;
            if (Header_Setter(line, "MARKET_OPEN_MINUTE"))
                return true;
            if (Header_Setter(line, "MARKET_CLOSE_MINUTE"))
                return true;
            if (Header_Setter(line, "INTERVAL"))
                return true;
            if (Header_Setter(line, "COLUMNS"))            
                return true;
            if (Header_Setter(line, "DATA"))
                return true;
            if (Header_Setter(line, "TIMEZONE_OFFSET"))
                return true;

            return false;
        }

        YahooDataFeed(string targetInputFile, Kernel kernel)
        {
            char[] deliminators = {','};
            header = new Dictionary<string, string>();
            var sr = new StreamReader(targetInputFile);
            string line;

            while ((line = sr.ReadLine()) != null)
            {
                if (! Header_Part_Check(line)) break;
            }

            if (line == null)
            {
                // end of file or maybe even not header
                // throw exception MISSING DATA
            }

            if (! header.ContainsKey("COLUMNS"))
            {
                // throw exception UNKNOWN DATA FORMAT
            }

            string[] words = (header["COLUMNS"]).Split(deliminators);
            data_format = new List<string>(words);

            
        }
    }
}
