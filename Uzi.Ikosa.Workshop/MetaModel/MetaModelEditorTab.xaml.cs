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
using Uzi.Visualize.Packaging;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for MetaModelEditorTab.xaml
    /// </summary>
    public partial class MetaModelEditorTab : TabItem, IHostedTabItem
    {
        public MetaModelEditorTab(MetaModel metaModel, IHostTabControl host)
        {
            InitializeComponent();
            DataContext = metaModel;
            _Host = host;
        }

        private IHostTabControl _Host;

        public MetaModel MetaModel { get { return this.DataContext as MetaModel; } }
        public object PackageItem { get { return MetaModel; } }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _Host.RemoveTabItem(this);
        }

        #region IHostedTabItem Members

        public void CloseTabItem() { }

        #endregion
    }
}