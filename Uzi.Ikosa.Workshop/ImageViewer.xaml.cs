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
using Uzi.Visualize.Packaging;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for ImageViewer.xaml
    /// </summary>
    public partial class ImageViewer : TabItem, IPackageItem, IHostedTabItem
    {
        public ImageViewer(BitmapImagePart image, IHostTabControl host)
        {
            InitializeComponent();
            this.DataContext = image;
            _Image = image;
            _Host = host;
        }

        private IHostTabControl _Host;

        private BitmapImagePart _Image;
        public BitmapImagePart ImagePart { get { return _Image; } }

        public object PackageItem { get { return _Image; } }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _Host.RemoveTabItem(this);
        }

        #region IHostedTabItem Members
        public void CloseTabItem() { }
        #endregion
    }
}
