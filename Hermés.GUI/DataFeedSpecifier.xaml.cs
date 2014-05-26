using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

namespace Hermés.GUI
{
    /// <summary>
    /// Interaction logic for DataFeedSpecifier.xaml
    /// </summary>
    public partial class DataFeedSpecifier : Window
    {
        private MainWindow _mainWindow;
        //private Dictionary<string,voodoo> 

        public struct DataFeedInitialization
        {
            public Type Type;
            public string Adress;
            public int? PointPrice;
            public CultureInfo CultureInfo;
        }



        public IEnumerable<string> DataFeedKeysItems
        {
            get { return ((IEnumerable<string>)(_mainWindow.DataFeedTypes.Keys)); }
        }

        public DataFeedSpecifier(MainWindow mainWindow)
        {
            DataContext = this;
            _mainWindow = mainWindow;
            InitializeComponent();
        }


        private void TypeBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
