using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hermés.Core;
using Hermés.Core.Common;
using Hermés.Core.Events;


namespace Hermés.Core.Test
{
    class DataFeedMock : DataFeed
    {
        private readonly PriceGroup _constantPriceGroup;

        public DataFeedMock(double constantPrice)
        {
            _constantPriceGroup = new PriceGroup()
            {
                Open = constantPrice,
                High = constantPrice,
                Low = constantPrice,
                Close = constantPrice,
                Volume = 0,
                OpenInterenst = 0
            };
        }

        public override void Dispose()
        {
        }

        public override PriceGroup CurrentPrice(Ticker ticker, PriceKind priceKind)
        {
            return _constantPriceGroup;
        }
    }

    [TestClass]
    public class NaivePortfolioTest
    {
        [TestMethod]
        public void NaivePortfolio_EmptyPortfolioValue()
        {
            const double initial = 1000.0;
            var kernel = new Kernel();
            var portfolio = new NaivePortfolio(initial);

            Assert.AreEqual(initial, portfolio.PortfolioValue);
        }

        [TestMethod]
        public void NaivePortfolio_SingleTickerPortfolioValue()
        {
            const double initial = 1000.0;
            var portfolio = new NaivePortfolio(initial);
            var datafeed = new DataFeedMock(300);
            var ticker = new Ticker("RUT", "CME", 1, 2014);
            var tickerInfo = new TickerInfo(ticker)
            {
                PointPrice = 50, 
                TickSize = 1/4
            };

            var kernel = portfolio.Kernel;

            portfolio.TickerInfos.Add(ticker, tickerInfo);
            portfolio.DataFeeds.AddDataFeed(datafeed);

            kernel.AddEvent(new FillEvent(ticker, TradeDirection.Buy, 100, 100, 0, 1));
            kernel.AddEvent(new FillEvent(ticker, TradeDirection.Buy, 100, 100, 0, 1));
            kernel.AddEvent(new FillEvent(ticker, TradeDirection.Buy, 100, 100, 0, 1));

            kernel.RegisterEventConsumer(portfolio);
            kernel.Run();

            var expectedValue = initial + 3*200*tickerInfo.PointPrice;
            Assert.AreEqual(expectedValue, portfolio.PortfolioValue);
        }

        [TestMethod]
        public void NaivePortfolio_SingleTickerSellPortfolioValue()
        {
            const double initial = 1000.0;
            var portfolio = new NaivePortfolio(initial);
            var datafeed = new DataFeedMock(300);
            var ticker = new Ticker("RUT", "CME", 1, 2014);
            var tickerInfo = new TickerInfo(ticker)
            {
                PointPrice = 50,
                TickSize = 1 / 4
            };

            var kernel = portfolio.Kernel;

            portfolio.TickerInfos.Add(ticker, tickerInfo);
            portfolio.DataFeeds.AddDataFeed(datafeed);

            kernel.AddEvent(new FillEvent(ticker, TradeDirection.Sell, 100, 100, 0, 1));
            kernel.AddEvent(new FillEvent(ticker, TradeDirection.Sell, 100, 100, 0, 1));
            kernel.AddEvent(new FillEvent(ticker, TradeDirection.Sell, 100, 100, 0, 1));

            kernel.RegisterEventConsumer(portfolio);
            kernel.Run();

            var expectedValue = initial - 3 * 200 * tickerInfo.PointPrice;
            Assert.AreEqual(expectedValue, portfolio.PortfolioValue);
        }
    }
}
