using System;
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

        private void TypeBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var t = _mainWindow.DataFeedTypes[TypeBox.SelectedItem.ToString()];
            MainGrid.Children.Remove(DataFeedPanel);
            DataFeedPanel = DataFeedGuis[t].MakePanel();
            DataFeedPanel.Margin = new Thickness(0,104,0,0);
            MainGrid.Children.Add(DataFeedPanel);
            

        }
    }
}
