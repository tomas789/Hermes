using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Hermés.GUI
{
    /// <summary>
    /// Interaction logic for DataFeedRemover.xaml
    /// </summary>
    public partial class DataFeedRemover : Window
    {
        private MainWindow _mainWindow;

        public IEnumerable<string> DataFeedKeysItems
        {
            get { return _mainWindow.DataFeedItems; }
        }

        public DataFeedRemover(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            DataContext = this;
            InitializeComponent();
        }

        private void ConfirmButton_OnClick(object sender, RoutedEventArgs e)
        {
            _mainWindow.RemoveSelectedDataFeed(DataFeedBox.SelectedItem.ToString());
            this.Close();
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DataFeedBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConfirmButton.IsEnabled = true;
        }
    }
}
