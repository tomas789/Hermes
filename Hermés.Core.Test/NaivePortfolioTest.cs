using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hermés.Core;
using System.Threading;
using System.Threading.Tasks;
using Hermés.Core.Common;
using Hermés.Core.Events;
using Hermés.Core.Portfolios;

namespace Hermés.Core.Test
{
    class DataFeedMock : DataFeed
    {
        private readonly PriceGroup _constantPriceGroup;

        public DataFeedMock(double constantPrice, double pointPrice)
            : base(pointPrice)
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
        public override PriceGroup CurrentPrice(PriceKind priceKind = PriceKind.Unspecified)
        {
            return _constantPriceGroup;
        }

        public override PriceGroup GetHistoricalPriceGroup(int lookbackPeriod)
        {
            throw new NotImplementedException();
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
            var initial = 1000.0;
            var pointPrice = 50.0;
            var portfolio = new NaivePortfolio(initial);
            var datafeed = new DataFeedMock(300, pointPrice);

            var kernel = portfolio.Kernel;

            portfolio.DataFeeds.AddDataFeed(datafeed);

            kernel.AddEvent(new FillEvent(datafeed, TradeDirection.Buy, 100, 100, 0, 1));
            kernel.AddEvent(new FillEvent(datafeed, TradeDirection.Buy, 100, 100, 0, 1));
            kernel.AddEvent(new FillEvent(datafeed, TradeDirection.Buy, 100, 100, 0, 1));

            kernel.RegisterEventConsumer(portfolio);
            var task = new Task(kernel.Run);
            task.Start();

            Thread.Sleep(50);
            kernel.StopSimulation();

            var expectedValue = initial + 3 * 200 * pointPrice;
            Assert.AreEqual(expectedValue, portfolio.PortfolioValue);
        }

        [TestMethod]
        public void NaivePortfolio_SingleTickerSellPortfolioValue()
        {
            const double initial = 1000.0;
            var pointPrice = 50.0;
            var portfolio = new NaivePortfolio(initial);
            var datafeed = new DataFeedMock(300, pointPrice);

            var kernel = portfolio.Kernel;

            portfolio.DataFeeds.AddDataFeed(datafeed);

            kernel.AddEvent(new FillEvent(datafeed, TradeDirection.Sell, 100, 100, 0, 1));
            kernel.AddEvent(new FillEvent(datafeed, TradeDirection.Sell, 100, 100, 0, 1));
            kernel.AddEvent(new FillEvent(datafeed, TradeDirection.Sell, 100, 100, 0, 1));

            kernel.RegisterEventConsumer(portfolio);
            var task = new Task(kernel.Run);
            task.Start();

            Thread.Sleep(50);
            kernel.StopSimulation();

            var expectedValue = initial - 3 * 200 * pointPrice;
            Assert.AreEqual(expectedValue, portfolio.PortfolioValue);
        }
    }
}
