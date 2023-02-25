using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Uzi.Ikosa.Guildsmanship;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for SettlementTab.xaml
    /// </summary>
    public partial class SettlementTab : TabItem, IHostedTabItem
    {
        public SettlementTab(Settlement settlement, IHostTabControl host)
        {
            InitializeComponent();
            DataContext = settlement;
            _Host = host;
        }

        private readonly IHostTabControl _Host;

        public Settlement Settlement => DataContext as Settlement;

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _Host.RemoveTabItem(this);
            e.Handled = true;
        }

        public void CloseTabItem() { }
    }
}
