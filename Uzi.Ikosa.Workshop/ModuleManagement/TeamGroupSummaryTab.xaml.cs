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
    /// Interaction logic for TeamGroupSummaryTab.xaml
    /// </summary>
    public partial class TeamGroupSummaryTab : TabItem, IHostedTabItem
    {
        public TeamGroupSummaryTab(PartsFolder folder, IHostTabControl host)
        {
            InitializeComponent();
            var _module = folder.Parent as Module;
            var _tgs = new TeamGroupSummaryFolderVM(_module, folder);
            DataContext = _tgs;
            _Host = host;
        }

        private readonly IHostTabControl _Host;

        public TeamGroupSummaryFolderVM TeamGroupSummaryFolder => DataContext as TeamGroupSummaryFolderVM;
        public object PackageItem => TeamGroupSummaryFolder.PartsFolder;

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _Host.RemoveTabItem(this);
        }

        // IHostedTabItem Members
        public void CloseTabItem() { }
    }
}
