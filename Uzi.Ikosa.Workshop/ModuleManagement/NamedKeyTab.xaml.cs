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
    /// Interaction logic for NamedKeyTab.xaml
    /// </summary>
    public partial class NamedKeyTab : TabItem, IHostedTabItem
    {
        public NamedKeyTab(NamedKeysPart part, IHostTabControl host)
        {
            InitializeComponent();
            DataContext = new NamedKeysPartVM(part);
            _Host = host;
        }

        private readonly IHostTabControl _Host;

        public NamedKeysPartVM NamedKeysPart => DataContext as NamedKeysPartVM;
        public object PackageItem => NamedKeysPart.Part;

        private void btnClose_Click(object sender, RoutedEventArgs e)
            => _Host.RemoveTabItem(this);

        // IHostedTabItem Members
        public void CloseTabItem() { }
    }
}
