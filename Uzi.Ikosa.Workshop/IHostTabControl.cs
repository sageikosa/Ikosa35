using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Uzi.Ikosa.Workshop
{
    public interface IHostTabControl
    {
        void RemoveTabItem(IHostedTabItem item);
        void FindOrOpen<TabType>(Func<TabType, bool> match, Func<TabType> generate) where TabType : TabItem;
        Window GetWindow();
    }
}
