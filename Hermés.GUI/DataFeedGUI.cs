using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Hermés.Core;

namespace Hermés.GUI
{
    public abstract class DataFeedGUI
    {
        public abstract StackPanel MakePanel();
        public abstract DataFeed GetDataFeed();
        public abstract string GetDataFeedName();
    }
}
