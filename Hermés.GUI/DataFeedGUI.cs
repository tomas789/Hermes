using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Hermés.Core;

namespace Hermés.GUI
{
    public abstract class DataFeedGUI : StackPanel
    {
        public abstract void SetPanel();
        public abstract DataFeed GetDataFeed();
    }
}
