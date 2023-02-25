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
using Uzi.Packaging;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for IconFolderPreviewTab.xaml
    /// </summary>
    public partial class IconFolderPreviewTab : TabItem, IHostedTabItem
    {
        public IconFolderPreviewTab(PartsFolder folder, IHostTabControl host)
        {
            InitializeComponent();
            DataContext = folder;
            _Host = host;
            ifpControl.HostTabControl = _Host;
        }

        private readonly IHostTabControl _Host;

        public PartsFolder PartsFolder => DataContext as PartsFolder;
        public object PackageItem => PartsFolder;

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _Host.RemoveTabItem(this);
        }

        // IHostedTabItem Members
        public void CloseTabItem() { }
    }
}
