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
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for SliverEditorTab.xaml
    /// </summary>
    public partial class SliverEditorTab : TabItem, IPackageItem, IHostedTabItem
    {
        public SliverEditorTab(SliverCellSpace sliverCell, IHostTabControl host)
        {
            InitializeComponent();
            DataContext = sliverCell;
            _Host = host;
            sliverEditor.Map = sliverCell.CellMaterial.LocalMap;
        }

        private IHostTabControl _Host;

        public object PackageItem => DataContext;

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _Host.RemoveTabItem(this);
        }

        #region IHostedTabItem Members
        public void CloseTabItem() { }
        #endregion
    }
}
