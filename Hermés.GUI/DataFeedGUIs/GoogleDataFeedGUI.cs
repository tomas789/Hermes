using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Hermés.GUI;
using Hermés.Core;
using Hermés.Core.DataFeeds;

namespace Hermés.GUI.DataFeedGUIs
{
    class GoogleDataFeedGUI : DataFeedGUI
    {
        private StackPanel _panel;

        public override StackPanel MakePanel()
        {
            _panel = new StackPanel();
            
            var nameButton = new Button {Content = "Select File", Visibility = Visibility.Visible};
            nameButton.Click += OnButtonClick;
            var valueLabel = new Label { Content = "Value: ", Visibility = Visibility.Visible };
            var valueTextBox = new TextBox { Visibility = Visibility.Visible };

            _panel.Children.Add(nameButton);
            _panel.Children.Add(valueLabel);
            _panel.Children.Add(valueTextBox);
            return _panel;
        }

        private void OnButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            Debug.WriteLine("POOOOF");
        }

        public override DataFeed GetDataFeed()
        {
            return null;
        }
    }
}
