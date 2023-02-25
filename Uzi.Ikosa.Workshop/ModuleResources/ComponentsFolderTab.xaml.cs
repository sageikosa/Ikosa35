using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shapes;
using Uzi.Packaging;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for SiteFolderTab.xaml
    /// </summary>
    public partial class ComponentsFolderTab : TabItem, IHostedTabItem
    {
        public ComponentsFolderTab(PartsFolder folder, IHostTabControl host)
        {
            InitializeComponent();
            DataContext = folder;
            _Host = host;
            sfcControl.HostTabControl = _Host;
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
