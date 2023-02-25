using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for KeyRingTab.xaml
    /// </summary>
    public partial class KeyRingTab : TabItem, IHostedTabItem
    {
        public KeyRingTab(KeyRingVM keyRing, IHostTabControl host)
        {
            InitializeComponent();
            DataContext = keyRing;
            _Host = host;
        }

        private IHostTabControl _Host;

        public KeyRing KeyRing => (DataContext as KeyRingVM)?.Thing;

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _Host.RemoveTabItem(this);
        }

        #region IHostedTabItem Members
        public void CloseTabItem() { }
        #endregion

        private void miNewKeyRing_Click(object sender, RoutedEventArgs e)
        {
            KeyRing.Add(new KeyRing(@"Key Ring") { Possessor = KeyRing.Possessor });
            e.Handled = true;
        }

        private void miNewKey_Click(object sender, RoutedEventArgs e)
        {
            KeyRing.Add(new KeyItem(@"Key", new Guid[] { }) { Possessor = KeyRing.Possessor });
            e.Handled = true;
        }

        private void miRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (lstItems.SelectedItem is IKeyRingMountable _item)
            {
                KeyRing.Remove(_item);
            }
            e.Handled = true;
        }
    }
}
