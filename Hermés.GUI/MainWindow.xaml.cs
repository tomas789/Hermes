using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Hermés.Core.Brokers;
using System.Diagnostics;
using Hermés.Core.Events;
using Microsoft.Win32;
using Hermés.Core;
using Hermés.Core.Portfolios;
using Hermés.Core.DataFeeds;
using Hermés.Core.Strategies;

namespace Hermés.GUI
{
    // TODO: make dependency property for conversion string->DataFeed DataFeed->string

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Portfolio _portfolio;
        private Dictionary<string, DataFeed> _dataFeeds;
        private double _pointPrice = 50;
        private double _initialCapital = 1000000;
        public Dictionary<string, Type> DataFeedTypes;

        public void SetDataFeeds()
        {
            DataFeedTypes = new Dictionary<string, Type> {{"GoogleDataFeed", typeof (GoogleDataFeed)}};
        }

        public MainWindow()
        {
            DataContext = this;
            _dataFeeds = new Dictionary<string, DataFeed>();
            SetDataFeeds();
            InitializeComponent();
        }

        public IEnumerable<string> PortfolioItems
        {
            get { return new string[] { "NaivePortfolio" }; }
        }

        public IEnumerable<string> BrokerItems
        {
            get { return new string[] { "AMPBroker" }; }
        }

        public IEnumerable<string> DataFeedItems
        {
            get { return _dataFeeds.Keys.ToArray(); }
        }

        private void RunButton_OnClick(object sender, RoutedEventArgs e)
        {
            ContinueButton.IsEnabled = false;

            var begin = DateTime.Now;
            StatusBox.Content = "Working";
            var runTask = new Task(_portfolio.Kernel.Run);
            runTask.ContinueWith(delegate
            {
                StatusBox.Dispatcher.BeginInvoke((Action)delegate
                {
                    StatusBox.Content = string.Format("Done in {0}, value {1}", DateTime.Now - begin,
                        _portfolio.PortfolioValue);
                });
            });

            runTask.Start();
        }

        private void StepButton_onClick(object sender, RoutedEventArgs e)
        {
            _portfolio.Kernel.Step();
            StatusBox.Content = string.Format("Events: {0}, Portfolio value: {1}, WallTime: {2}", 
                _portfolio.Kernel.Events.Count,
                _portfolio.PortfolioValue,
                _portfolio.Kernel.WallTime);
        }

        private void StopButton_OnClick(object sender, RoutedEventArgs e)
        {
            _portfolio.Kernel.StopSimulation();
        }

        private void ConstructButton_OnClick(object sender, RoutedEventArgs e)
        {
            var selected = PortfolioComboBox.SelectedValue;
            if (selected == null)
            {
                MessageBox.Show("Portfolio must be selected.");
                return;
            }

            switch (selected.ToString())
            {
                case "NaivePortfolio":
                    _portfolio = new NaivePortfolio(_initialCapital);
                    break;
                default:
                    throw new Exception("Impossible.");
            }

            PortfolioComboBox.IsEnabled = false;
            ConstructButton.IsEnabled = false;
            AddDataFeed.IsEnabled = false;
            DataFeedBox.IsEnabled = false;

            foreach (var dataFeed in _dataFeeds)
            {
                Debug.WriteLine("Adding datafeed: {0}", dataFeed.Key);
                _portfolio.DataFeeds.AddDataFeed(dataFeed.Value);
            }

            _portfolio.Strategies.AddStrategy(new GeneticStrategy());
            _portfolio.Broker = new AMPBroker();
            _portfolio.Initialize();

            ContinueButton.IsEnabled = true;
        }

        private void AddDataFeed_OnClick(object sender, RoutedEventArgs e)
        {
            var dfs = new DataFeedSpecifier(this);
            dfs.ShowDialog();
        }

        public void AddSelectedDataFeed(string dataFeedName, DataFeed dataFeed)
        {
            if (dataFeed == null || _dataFeeds == null || DataFeedBox == null ||
                dataFeedName == "")
                return;
            _dataFeeds.Add(dataFeedName,dataFeed);
            DataFeedBox.ItemsSource = DataFeedItems;
            RemoveDataFeed.IsEnabled = true;
        }

        private void RemoveDataFeed_OnClick(object sender, RoutedEventArgs e)
        {
            if (_dataFeeds == null || DataFeedBox == null)
                return;
            var rdf = new DataFeedRemover(this);
            rdf.ShowDialog();
        }

        public void RemoveSelectedDataFeed(string dataFeedName)
        {
            if (_dataFeeds == null || DataFeedBox == null || dataFeedName == "")
                return;
            // not sure about which exception in here
            if (! _dataFeeds.ContainsKey(dataFeedName))
                throw new IdentityNotMappedException();
            _dataFeeds.Remove(dataFeedName);
            DataFeedBox.ItemsSource = DataFeedItems;
            if (_dataFeeds.Count == 0)
                RemoveDataFeed.IsEnabled = false;
        }

        private void ExitButton_OnClick(object sender, RoutedEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void StatusBox_Update(object o, EventArgs sender)
        {
            if (StatusBox == null || StatusBox.Content == null)
                return;

            if (_portfolio == null)
            {
                StatusBox.Content = "Portfolio not initialized yet.";
                return;
            }

            StatusBox.Content = string.Format("Events: {0}, Portfolio value: {1}, WallTime: {2}",
                _portfolio.Kernel.Events.Count,
                _portfolio.PortfolioValue,
                _portfolio.Kernel.WallTime);
        }

        private void StatusBox_OnLoad(object sender, RoutedEventArgs e)
        {
            var myDispatcherTimer = new DispatcherTimer {Interval = new TimeSpan(0, 0, 0, 0, 100)};
            myDispatcherTimer.Tick += StatusBox_Update;
            myDispatcherTimer.Start();
        }

        private void GeneticEditorButton_OnClick(object sender, RoutedEventArgs e)
        {
            var window = new GeneticStrategyEditWindow();
            window.Show();
        }

    }
}
