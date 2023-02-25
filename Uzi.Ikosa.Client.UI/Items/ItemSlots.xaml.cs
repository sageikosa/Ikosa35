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
using Uzi.Ikosa.Proxy.ViewModel;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Client.UI
{
    /// <summary>
    /// Interaction logic for ItemSlots.xaml
    /// </summary>
    public partial class ItemSlots : UserControl
    {
        public ItemSlots()
        {
            try { InitializeComponent(); } catch { }
            var _uri = new Uri(@"/Uzi.Ikosa.Client.UI;component/Items/ItemListTemplates.xaml", UriKind.Relative);
            Resources.MergedDictionaries.Add(Application.LoadComponent(_uri) as ResourceDictionary);
            Resources.Add(@"slctItemListTemplate", ItemListTemplateSelector.GetDefault(Resources));
            lstSlots.SelectionMode = SelectionMode.Single;
            // TODO: show carrying information here as well...
        }

        public ActorModel Actor => DataContext as ActorModel;

        #region public ICommand DoActionCommand { get; set; } (DEPENDENCY)
        public ICommand DoActionCommand
        {
            get => (ICommand)GetValue(DoActionCommandProperty);
            set => SetValue(DoActionCommandProperty, value);
        }

        // Using a DependencyProperty as the backing store for DoActionCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DoActionCommandProperty =
            DependencyProperty.Register(nameof(DoActionCommand), typeof(ICommand), typeof(ItemSlots), new PropertyMetadata(null));
        #endregion

        public IEnumerable<ItemSlotInfo> HoldingSlots
            => SlottedItems.Where(_is => _is.SlotType.Equals(@"Holding", StringComparison.OrdinalIgnoreCase));

        public IEnumerable<ItemSlotInfo> SlottedItems
            => lstSlots.ItemsSource as IEnumerable<ItemSlotInfo>;

        public CreatureInfo Creature
            => grpCarrying.DataContext as CreatureInfo;

        #region private void lstSlots_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        private void lstSlots_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var _menu = new MenuViewModel();
            if ((Actor != null) && (lstSlots?.SelectedItem is ItemSlotInfo _slot))
            {
                var _acts = Actor.Actions
                     .Where(_a => (_a.ID == _slot.ID) || (_a.ID == _slot.ItemInfo?.ID))
                     .ToList();
                if (_acts.Any())
                {
                    // actions if defined
                    ActionMenuBuilder.AddContextMenu(_menu, _acts, DoActionCommand);
                }

                if (_slot.HasIdentities)
                {
                    // identities if possible
                    var _idents = IdentitiesMenuViewModel.GetContextMenu(Actor, _slot.ItemInfo, Actor.SetIdentity);
                    if (_idents.SubItems.Any())
                    {
                        // only if we need to separate actions from identities
                        //if (_acts.Any())
                        //    _menu.SubItems.Add(new SeparatorViewModel());
                        _menu.SubItems.Add(_idents);
                    }
                }
            }

            if (_menu.SubItems.Any())
            {
                // set context menu
                lstSlots.ContextMenu.ItemsSource = _menu.SubItems;
            }
            else
            {
                // cancel
                lstSlots.ContextMenu.ItemsSource = null;
                lstSlots.ContextMenu.IsOpen = false;
                e.Handled = true;
            }
        }
        #endregion
    }
}
