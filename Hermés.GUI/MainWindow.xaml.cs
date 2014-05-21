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
using Hermés.Core.Brokers;
using Microsoft.Win32;
using Hermés.Core;
using Hermés.Core.Portfolios;
using Hermés.Core.DataFeeds;

namespace Hermés.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NaivePortfolio _naivePortfolio;
        private static double _pointPrice = 10;
        private static double _initialCapital = 1000000;

        public MainWindow()
        {
            _naivePortfolio = new NaivePortfolio(_initialCapital);
            InitializeComponent();
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
                _naivePortfolio.DataFeeds.AddDataFeed(gdf);
                Pick_textbox.Text = "Picked File: "+file;
                Initialize_textbox.Text = "Press initialize";
            }

        }

        private void Initialize_button_OnClick(object sender, RoutedEventArgs e)
        {
            if (Pick_textbox.Text == "File not picked yet!")
                return;
            _naivePortfolio.Broker = new AMPBroker();
            _naivePortfolio.Initialize();
            
            Initialize_textbox.Text = "Initialized!";
        }
        private void Run_button_OnClick(object sender, RoutedEventArgs e)
        {
            if (Initialize_textbox.Text != "Initialized!")
                return;
            if (Run_textbox.Text != "Working")
            {
                Run_textbox.Text = "Working";
                _naivePortfolio.Kernel.Run();
            }
        }
    }
}
