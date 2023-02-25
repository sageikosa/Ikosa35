using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Uzi.Ikosa.UI;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for PanelSpaceEditorTab.xaml
    /// </summary>
    public partial class PanelSpaceEditorTab : TabItem, IPackageItem, IHostedTabItem
    {
        public PanelSpaceEditorTab(PanelCellSpace panelCell, IHostTabControl host)
        {
            InitializeComponent();
            this.DataContext = panelCell;
            _Host = host;
            this.panelSpaceEditor.Map = panelCell.CellMaterial.LocalMap;
        }

        private IHostTabControl _Host;

        public object PackageItem { get { return this.DataContext; } }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _Host.RemoveTabItem(this);
        }

        #region IHostedTabItem Members
        public void CloseTabItem() { }
        #endregion
    }
}
