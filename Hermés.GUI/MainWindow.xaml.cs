using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
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
using Hermés.Core.Events;
using Microsoft.Win32;
using Hermés.Core;
using Hermés.Core.Portfolios;
using Hermés.Core.DataFeeds;
using Hermés.Core.Strategies;

namespace Hermés.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Portfolio _portfolio;
        private double _pointPrice = 50;
        private double _initialCapital = 1000000;

        public static readonly DependencyProperty IsEverythingLoadedProperty =
            DependencyProperty.Register("IsEverythingLoaded", typeof (Boolean),
                typeof (MainWindow), new PropertyMetadata(false));

        public Boolean IsEverythingLoaded
        {
            get { return (Boolean)this.GetValue(IsEverythingLoadedProperty); }
            set { this.SetValue(IsEverythingLoadedProperty, value); }
        }

        public MainWindow()
        {
            _portfolio = new NaivePortfolio(_initialCapital);
            InitializeComponent();
            IsEverythingLoaded = true;
        }


        private void Pick_button_OnClick(object sender, RoutedEventArgs e)
        {
            if (Pick_textbox.Text != "File not picked yet!")
            {
                return;
            }
            var dlg = new Microsoft.Win32.OpenFileDialog();
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                var file = dlg.FileName;
                var gdf = new GoogleDataFeed(file, _pointPrice);
                _portfolio.Strategies.AddStrategy(new BuyAndHoldStrategy());
                _portfolio.DataFeeds.AddDataFeed(gdf);
                Pick_textbox.Text = "Picked File: "+file;
                Initialize_textbox.Text = "Press initialize";
            }

        }

        private void Initialize_button_OnClick(object sender, RoutedEventArgs e)
        {
            if (Pick_textbox.Text == "File not picked yet!")
                return;
            _portfolio.Broker = new AMPBroker();
            _portfolio.Initialize();
            
            Initialize_textbox.Text = "Initialized!";
            IsEverythingLoaded = false;
        }
        private void Run_button_OnClick(object sender, RoutedEventArgs e)
        {
            if (Initialize_textbox.Text != "Initialized!" || Run_textbox.Text == "Working")
                return;

            var begin = DateTime.Now;
            Run_textbox.Text = "Working";
            var runTask = new Task(_portfolio.Kernel.Run);
            runTask.ContinueWith(delegate
            {
                Run_textbox.Dispatcher.BeginInvoke((Action)delegate
                {
                    Run_textbox.Text = string.Format("Done in {0}, value {1}", DateTime.Now - begin,
                        _portfolio.PortfolioValue);
                });
            });

            runTask.Start();
        }

        private void StepButton_onClick(object sender, RoutedEventArgs e)
        {
            _portfolio.Kernel.Step();
            EventCounter.Content = string.Format("Events: {0}, Portfolio value: {1}, WallTime: {2}", 
                _portfolio.Kernel.Events.Count,
                _portfolio.PortfolioValue,
                _portfolio.Kernel.WallTime);
        }

        private void StopButton_OnClick(object sender, RoutedEventArgs e)
        {
            _portfolio.Kernel.StopSimulation();
        }
    }
}
