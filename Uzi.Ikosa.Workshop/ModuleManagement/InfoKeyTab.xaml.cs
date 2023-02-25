using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Uzi.Ikosa.Guildsmanship;
using Uzi.Ikosa.UI;
using Uzi.Packaging;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for InfoKeyTab.xaml
    /// </summary>
    public partial class InfoKeyTab : TabItem, IHostedTabItem
    {
        public InfoKeyTab(PartsFolder folder, IHostTabControl host)
        {
            InitializeComponent();
            DataContext = new InfoKeyFolderVM(folder.Parent as Module, folder);
            _Host = host;
        }

        private readonly IHostTabControl _Host;

        public InfoKeyFolderVM InfoKeyFolder => DataContext as InfoKeyFolderVM;
        public object PackageItem => InfoKeyFolder.PartsFolder;

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _Host.RemoveTabItem(this);
        }

        // IHostedTabItem Members
        public void CloseTabItem() { }
    }
}
