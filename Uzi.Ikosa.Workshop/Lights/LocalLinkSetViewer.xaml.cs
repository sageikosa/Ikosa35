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
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for LocalLinkSetViewer.xaml
    /// </summary>
    public partial class LocalLinkSetViewer : TabItem, IHostedTabItem
    {
        public LocalLinkSetViewer(LocalLinkSet linkSet, IHostTabControl host)
        {
            InitializeComponent();
            this.DataContext = linkSet;
            _Host = host;
        }

        private IHostTabControl _Host;

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _Host.RemoveTabItem(this);
        }

        private void lstLinks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: change semantics
            if ((stackLinkLights != null) && (lstLinks != null))
            {
                var _link = (lstLinks.SelectedItem as LocalLink);
                stackLinkLights.Children.Clear();
                stackLinkLights.Children.Add(new ContentControl { Content = _link.LightA });
                stackLinkLights.Children.Add(new ContentControl { Content = _link.LightB });
            }
        }

        #region IHostedTabItem Members
        public void CloseTabItem() { }
        #endregion
    }
}
