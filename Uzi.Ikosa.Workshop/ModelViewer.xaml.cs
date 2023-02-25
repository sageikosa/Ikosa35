using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Uzi.Ikosa.UI;
using Uzi.Visualize.Packaging;
using System.Linq;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for ModelViewer.xaml
    /// </summary>
    public partial class ModelViewer : TabItem, IPackageItem, IHostedTabItem
    {
        // TODO: SenseEffect switches...
        public ModelViewer(Model3DPart model, IHostTabControl host)
        {
            InitializeComponent();
            _Model = model;
            DataContext = model;
            if (model != null)
            {
                var _mdl = model.ResolveModel();
                mdlHolder.Children.Add(_mdl);
            }
            _Host = host;
        }

        private IHostTabControl _Host;

        private Model3DPart _Model;
        public Model3DPart Model3DPart { get { return _Model; } }

        public object PackageItem { get { return _Model; } }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _Host.RemoveTabItem(this);
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            mdlHolder.Children.Clear();
            _Model.RefreshModel();
            mdlHolder.Children.Add(_Model.ResolveModel());
        }

        #region IHostedTabItem Members
        public void CloseTabItem() { }
        #endregion
    }
}