using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Hermés.GUI;
using Hermés.Core;
using Hermés.Core.DataFeeds;
using Button = System.Windows.Controls.Button;
using Label = System.Windows.Controls.Label;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using TextBox = System.Windows.Controls.TextBox;

namespace Hermés.GUI.DataFeedGUIs
{
    class GoogleDataFeedGUI : DataFeedGUI
    {
        private StackPanel _panel;

        private Button _fileNameButton;
        private TextBox _fileNameTextBox;
        private Label _pointPriceLabel;
        private TextBox _pointPriceTextBox;
        private Label _cultureInfoLabel;
        private TextBox _cultureInfoTextBox;

        private Button _confirmButton;
        private Button _cancelButton;

        public override StackPanel MakePanel()
        {
            _panel = new StackPanel();
            
            _fileNameButton = new Button { Content = "Select File", Visibility = Visibility.Visible };
            _fileNameTextBox = new TextBox { Visibility = Visibility.Visible, IsEnabled = false };
            _fileNameButton.Click += _fileNameButton_OnButtonClick;
            _pointPriceLabel = new Label { Content = "Point Price: ", Visibility = Visibility.Visible };
            _pointPriceTextBox = new TextBox { Visibility = Visibility.Visible };
            _cultureInfoLabel = new Label { Content = "Culture Info: ", Visibility = Visibility.Visible };
            _cultureInfoTextBox = new TextBox { Text = "en", Visibility = Visibility.Visible };
            _confirmButton = new Button { Content = "Confirm Adding DataFeed", Visibility = Visibility.Visible };
            _confirmButton.Click += _confirmButton_OnButtonClick;
            _cancelButton = new Button { Content = "Storno Adding DataFeed", Visibility = Visibility.Visible };
            _cancelButton.Click += _cancelButton_OnButtonClick;

            _panel.Children.Add(_fileNameButton);
            _panel.Children.Add(_fileNameTextBox);
            _panel.Children.Add(_pointPriceLabel);
            _panel.Children.Add(_pointPriceTextBox);
            _panel.Children.Add(_cultureInfoLabel);
            _panel.Children.Add(_cultureInfoTextBox);
            return _panel;
        }

        private void _cancelButton_OnButtonClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void _confirmButton_OnButtonClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void _fileNameButton_OnButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            //Debug.WriteLine("POOOOF");
            var dlg = new OpenFileDialog();
            dlg.ShowDialog();
            var fileName = dlg.FileName;
            _fileNameTextBox.Text = fileName;
        }

        public override DataFeed GetDataFeed()
        {
            // beware, here could be problem with parsing and exceptions
            var pointPrice = Convert.ToDouble(_pointPriceLabel.Content.ToString());
            var gdf = new GoogleDataFeed(_fileNameTextBox.Text, pointPrice);
            gdf.setCultureInfo(new CultureInfo(_cultureInfoTextBox.Text));
            return gdf;
        }
    }
}
