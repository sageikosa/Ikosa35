using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using Uzi.Ikosa.Guildsmanship;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for LocalMapSiteTab.xaml
    /// </summary>
    public partial class LocalMapSiteTab : TabItem, IHostedTabItem
    {
        public LocalMapSiteTab(LocalMapSite localMapSite, IHostTabControl host)
        {
            InitializeComponent();
            DataContext = localMapSite;
            _Host = host;
        }

        private readonly IHostTabControl _Host;

        public LocalMapSite LocalMapSite => DataContext as LocalMapSite;

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _Host.RemoveTabItem(this);
            e.Handled = true;
        }

        public void CloseTabItem() { }
    }
}
