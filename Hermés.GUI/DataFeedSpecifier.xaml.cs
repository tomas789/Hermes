﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Diagnostics;
using Hermés.Core;
using Hermés.Core.DataFeeds;
using Hermés.GUI.DataFeedGUIs;
using Microsoft.Win32;

namespace Hermés.GUI
{
    /// <summary>
    /// Interaction logic for DataFeedSpecifier.xaml
    /// </summary>
    public partial class DataFeedSpecifier : Window
    {
        private MainWindow _mainWindow;
        public Dictionary<Type, DataFeedGUI> DataFeedGuis;

        public void SetDataFeedGuis()
        {
            DataFeedGuis = new Dictionary<Type, DataFeedGUI>();
            if (!DataFeedGuis.ContainsKey(typeof (GoogleDataFeed)))
                DataFeedGuis.Add(typeof(GoogleDataFeed),new GoogleDataFeedGUI());
        }

        public IEnumerable<string> DataFeedKeysItems
        {
            get { return ((IEnumerable<string>)(_mainWindow.DataFeedTypes.Keys)); }
        }

        public DataFeedSpecifier(MainWindow mainWindow)
        {
            DataContext = this;
            _mainWindow = mainWindow;
            SetDataFeedGuis();
            InitializeComponent();
        }

        private void TypeBox_OnSelectionChanged(object sender, RoutedEventArgs eventArgs)
        {
            if (_mainWindow == null || _mainWindow.DataFeedTypes == null ||
                TypeBox == null || TypeBox.SelectedItem == null || DataFeedPanel == null)
                return;

            DataFeedPanel.Children.Clear();

            try
            {
                Type type;
                if (!_mainWindow.DataFeedTypes.TryGetValue(TypeBox.SelectedItem.ToString(), out type))
                    throw new Exception("Internal error (no item in DataFeedTypes found)");

                DataFeedGUI gui;
                if (!DataFeedGuis.TryGetValue(type, out gui))
                    throw new Exception("Internal error (no item in DataFeedGuis found)");

                DataFeedPanel.Children.Add(gui.MakePanel());
            }
            catch (Exception e)
            {
                Debug.WriteLine("Got exception {0}", e.Message);
                var errorLabel = new Label {Content = "Error: " + e.Message};
                DataFeedPanel.Children.Clear();
                DataFeedPanel.Children.Add(errorLabel);
            }

            DataFeedPanel.Dispatcher.Invoke(DispatcherPriority.Render, (Action)(() => { }));
        }

        private void ConfirmButton_OnClick(object sender, RoutedEventArgs eventArgs)
        {
            if (_mainWindow == null || _mainWindow.DataFeedTypes == null ||
                TypeBox == null || TypeBox.SelectedItem == null || DataFeedPanel == null
                || _mainWindow.DataFeedBox == null)
                return;
            
            try
            {
                Type type;
                if (!_mainWindow.DataFeedTypes.TryGetValue(TypeBox.SelectedItem.ToString(), out type))
                    throw new Exception("Internal error (no item in DataFeedTypes found)");

                DataFeedGUI gui;
                if (!DataFeedGuis.TryGetValue(type, out gui))
                    throw new Exception("Internal error (no item in DataFeedGuis found)");

                var dataFeed = gui.GetDataFeed();
                var dataFeedName = gui.GetDataFeedName();

                // not sure about exception in here
                if (dataFeedName == "" || dataFeed == null)
                    return;

                _mainWindow.AddSelectedDataFeed(dataFeedName,dataFeed);
                DataFeedPanel.Dispatcher.Invoke(DispatcherPriority.Render, (Action)(() => { }));
                this.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Got exception {0}", e.Message);
                var errorLabel = new Label { Content = "Error: " + e.Message };
            }
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
