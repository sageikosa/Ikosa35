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
    /// Interaction logic for VariableTab.xaml
    /// </summary>
    public partial class VariableTab : TabItem, IHostedTabItem
    {
        public VariableTab(PartsFolder folder, IHostTabControl host)
        {
            InitializeComponent();
            DataContext = new VariableFolderVM(folder.Parent as Module, folder, host.GetWindow());
            _Host = host;
        }

        private readonly IHostTabControl _Host;

        public VariableFolderVM VariableFolder => DataContext as VariableFolderVM;
        public object PackageItem => VariableFolder.PartsFolder;

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _Host.RemoveTabItem(this);
        }

        // IHostedTabItem Members
        public void CloseTabItem() { }
    }
}
