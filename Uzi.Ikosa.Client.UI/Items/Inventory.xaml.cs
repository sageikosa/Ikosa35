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
using Uzi.Ikosa.Proxy.IkosaSvc;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Proxy.ViewModel;

namespace Uzi.Ikosa.Client.UI
{
    /// <summary>
    /// Interaction logic for Inventory.xaml
    /// </summary>
    public partial class Inventory : UserControl
    {
        public Inventory()
        {
            try { InitializeComponent(); } catch { }
            var _uri = new Uri(@"/Uzi.Ikosa.Client.UI;component/Items/ItemListTemplates.xaml", UriKind.Relative);
            Resources.MergedDictionaries.Add(Application.LoadComponent(_uri) as ResourceDictionary);
            Resources.Add(@"slctItemListTemplate", ItemListTemplateSelector.GetDefault(Resources));
        }

        public ActorModel Actor => DataContext as ActorModel;

        public IEnumerable<PossessionInfo> Possessions
            => lstItems.ItemsSource as IEnumerable<PossessionInfo>;

        #region private void lstItems_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        private void lstItems_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var _menu = new MenuViewModel();
            if (lstItems?.SelectedItem is PossessionInfo _possess)
            {
                if (_possess.HasIdentities)
                {
                    var _idents = IdentitiesMenuViewModel.GetContextMenu(Actor, _possess.ObjectInfo, Actor.SetIdentity);
                    if (_idents.SubItems.Any())
                    {
                        _menu.SubItems.Add(_idents);
                    }

                    if (_menu.SubItems.Any())
                    {
                        // set context menu
                        lstItems.ContextMenu.ItemsSource = _menu.SubItems;
                        return;
                    }
                }
            }

            if (_menu.SubItems.Any())
            {
                // set context menu
                lstItems.ContextMenu.ItemsSource = _menu.SubItems;
            }
            else
            {
                // cancel
                lstItems.ContextMenu.ItemsSource = null;
                lstItems.ContextMenu.IsOpen = false;
                e.Handled = true;
            }
        }
        #endregion
    }
}
