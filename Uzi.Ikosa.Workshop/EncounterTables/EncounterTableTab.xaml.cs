using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Uzi.Ikosa.Guildsmanship;
using Uzi.Packaging;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for EncounterTableTab.xaml
    /// </summary>
    public partial class EncounterTableTab : TabItem, IHostedTabItem
    {
        // TODO: encounter table VMs
        public EncounterTableTab(EncounterTable table, IHostTabControl host)
        {
            InitializeComponent();
            DataContext = table;
            _Host = host;
        }

        private readonly IHostTabControl _Host;

        public EncounterTable EncounterTable => DataContext as EncounterTable;

        public object PackageItem => EncounterTable;

        private void btnClose_Click(object sender, RoutedEventArgs e)
            => _Host.RemoveTabItem(this);

        // IHostedTabItem Members
        public void CloseTabItem() { }
    }
}
