using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hermés.Core.DataFeeds;
using Hermés.Core.Brokers;

namespace Hermés.Core.Test.DataFeeds
{
    class PortfolioMock : Portfolio
    {

        protected override double GetPortfolioValue()
        {
            throw new NotImplementedException();
        }

        public override void DispatchConcrete(Events.FillEvent e)
        {
            throw new NotImplementedException();
        }

        public override void DispatchConcrete(Events.MarketEvent e)
        {
            throw new NotImplementedException();
        }

        public override void DispatchConcrete(Events.OrderEvent e)
        {
            throw new NotImplementedException();
        }

        public override void DispatchConcrete(Events.SignalEvent e)
        {
            throw new NotImplementedException();
        }
    }

    [TestClass]
    public class GoogleDataFeedTest
    {
        [TestMethod]
        public void GoogleDataFeed_HeaderParsing()
        {
            var input = @"EXCHANGE%3DINDEXDJX
MARKET_OPEN_MINUTE=570
MARKET_CLOSE_MINUTE=960
INTERVAL=86400
COLUMNS=DATE,CLOSE,HIGH,LOW,OPEN,VOLUME
DATA=
TIMEZONE_OFFSET=-240
a1395086400,16247.22,16270.34,16066.37,16066.37,88926593
1,16336.19,16369.94,16245.93,16245.93,79145434
2,16222.17,16363.32,16126.29,16335.71,90113882
3,16331.05,16353.98,16160.33,16221.98,91530690
4,16302.77,16456.45,16290.79,16332.69,353669863
7,16276.69,16380.51,16215.56,16303.28,110620603
8,16367.88,16407.18,16279.2,16279.2,89111351
9,16268.99,16466.04,16268.99,16370.71,92767094
10,16264.23,16300.94,16191.79,16268.67,93647449
11,16323.06,16414.86,16267.77,16267.77,86374508
14,16457.66,16480.85,16324.22,16324.22,104513694
15,16532.61,16565.73,16457.6,16458.05,88007115
16,16573,16588.19,16506.6,16532.8,78122361
17,16572.55,16604.15,16527.6,16572.36,77219599
18,16412.71,16631.63,16392.77,16576.02,104354205
21,16245.87,16421.38,16244.01,16414.15,116540728
22,16256.14,16296.86,16180.28,16245.16,98511400
23,16437.18,16438.82,16256.37,16256.37,91545981
24,16170.22,16456.12,16153.34,16437.24,112550245
25,16026.75,16168.87,16015.32,16168.87,119551971";

            using (var testDataStream = new StringReader(input))
            {
                var ticker = new Ticker("DUMMY", "DUMMY", 1, 2000);
                var googleDataFeed = new GoogleDataFeed(ticker, testDataStream);

                var portfolio = new PortfolioMock {Broker = new AMPBroker()};
                portfolio.DataFeeds.AddDataFeed(googleDataFeed);
                portfolio.Initialize();

                Assert.AreEqual(20, googleDataFeed.Count);
            }
        }

    }
}
