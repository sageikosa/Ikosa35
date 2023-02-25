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
    /// Interaction logic for ItemElementTab.xaml
    /// </summary>
    public partial class ItemElementTab : TabItem, IHostedTabItem
    {
        public ItemElementTab(PartsFolder folder, IHostTabControl host)
        {
            InitializeComponent();
            DataContext = new ItemElementFolderVM(folder.Parent as Module, folder);
            _Host = host;
        }

        private readonly IHostTabControl _Host;

        public ItemElementFolderVM ItemElementFolder => DataContext as ItemElementFolderVM;
        public object PackageItem => ItemElementFolder.PartsFolder;

        private void btnClose_Click(object sender, RoutedEventArgs e) 
            => _Host.RemoveTabItem(this);

        // IHostedTabItem Members
        public void CloseTabItem() { }
    }
}
