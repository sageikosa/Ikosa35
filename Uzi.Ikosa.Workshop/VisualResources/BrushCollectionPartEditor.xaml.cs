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
using System.Windows.Media.Media3D;
using System.Windows.Controls.Primitives;
using Uzi.Ikosa.UI;
using Uzi.Visualize;
using Uzi.Visualize.Packaging;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for TileSetEditor.xaml
    /// </summary>
    public partial class BrushCollectionPartEditor : TabItem, IPackageItem, IHostedTabItem
    {
        public BrushCollectionPartEditor(BrushCollectionPart collection, IHostTabControl host)
        {
            InitializeComponent();
            DataContext = collection;
            _Host = host;
        }

        private IHostTabControl _Host;

        public BrushCollectionPart Collection => DataContext as BrushCollectionPart;

        public object PackageItem => Collection;

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _Host.RemoveTabItem(this);
        }

        #region IHostedTabItem Members
        public void CloseTabItem() { }
        #endregion
    }
}
