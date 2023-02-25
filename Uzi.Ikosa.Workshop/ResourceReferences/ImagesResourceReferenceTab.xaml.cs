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
using Uzi.Visualize;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for ImagesResourceReferenceTab.xaml
    /// </summary>
    public partial class ImagesResourceReferenceTab : TabItem, IHostedTabItem
    {
        public ImagesResourceReferenceTab(ImagesResourceReference reference, IHostTabControl host, IkosaPackageManagerConfig packageConfig)
        {
            InitializeComponent();
            _Host = host;
            _Config = packageConfig;
            _Ref = reference;
            DataContext = this;
        }

        private readonly ImagesResourceReference _Ref;
        private readonly IHostTabControl _Host;
        private readonly IkosaPackageManagerConfig _Config;

        public ImagesResourceReference Reference => _Ref;
        public IkosaPackageManagerConfig Config => _Config;

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _Host.RemoveTabItem(this);
        }

        #region IHostedTabItem Members

        public void CloseTabItem() { }

        #endregion
    }
}
