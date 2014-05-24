﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Diagnostics;
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

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
        }

        public IEnumerable<string> ComboItems
        {
            get { return new string[] {"NaivePortfolio", "Test 2"}; }
        }

        private void InitializeButton_OnClick(object sender, RoutedEventArgs e)
        {
            _portfolio.Broker = new AMPBroker();
            _portfolio.Initialize();

            RunButton.IsEnabled = true;
            InitializeButton.IsEnabled = false;
        }
        private void RunButton_OnClick(object sender, RoutedEventArgs e)
        {
            var begin = DateTime.Now;
            StatusBox.Text = "Working";
            var runTask = new Task(_portfolio.Kernel.Run);
            runTask.ContinueWith(delegate
            {
                StatusBox.Dispatcher.BeginInvoke((Action)delegate
                {
                    StatusBox.Text = string.Format("Done in {0}, value {1}", DateTime.Now - begin,
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
            SelectFile.IsEnabled = false;
            AddDataFeed.IsEnabled = false;
            DataFeedBox.IsEnabled = false;

            foreach (var datafeed in from object filenameItem in DataFeedBox.Items
                select filenameItem.ToString()
                into filename
                select new GoogleDataFeed(filename, 1))
            {
                Debug.WriteLine("Adding datafeed: {0}", datafeed);
                _portfolio.DataFeeds.AddDataFeed(datafeed);
            }

            _portfolio.Strategies.AddStrategy(new BuyAndHoldStrategy());

            InitializeButton.IsEnabled = true;
        }

        private void AddDataFeed_OnClick(object sender, RoutedEventArgs e)
        {
            if (SelectedFile == null || 
                SelectedFile.Text == null || 
                SelectedFile.Text == "none")
                return;

            var filename = SelectedFile.Text;
            DataFeedBox.Items.Add(filename);
            SelectedFile.Text = "none";
        }

        private void SelectedFile_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (AddDataFeed == null)
                return;

            var text = SelectedFile.Text ?? "none";
            AddDataFeed.IsEnabled = text != "none";
        }

        private void SelectFile_OnClick(object sender, RoutedEventArgs e)
        {
            if (SelectedFile == null || SelectedFile.Text == null)
                return;

            var dlg = new Microsoft.Win32.OpenFileDialog();
            var result = dlg.ShowDialog();

            if (result != true) 
                return;

            var filename = dlg.FileName;
            SelectedFile.Text = filename;
        }
    }
}
