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

namespace Uzi.Ikosa.Client
{
    /// <summary>
    /// Interaction logic for ItemActionPanel.xaml
    /// </summary>
    public partial class ItemActionPanel : UserControl
    {
        public ItemActionPanel()
        {
            try { InitializeComponent(); } catch { }
        }

        private ItemSlotInfo[] _Slots;

        public ItemSlotInfo[] ItemSlots { get { return _Slots; } set { _Slots = value; } }

        //#region public void SetActions(IEnumerable<ActionInfo> actions)
        //public void SetActions(IEnumerable<ActionInfo> actions)
        //{
        //    // clear menu of items
        //    mnuItemActions.Items.Clear();
        //    if (this.ItemSlots != null)
        //    {
        //        // groups actions by itemSlot
        //        var _slotGroups = (from _a in actions
        //                           where this.ItemSlots.Any(_s=> _s.HasID(_a.ProviderID))
        //                           group _a by _a.ProviderID).ToList();
        //        ActionMenus.AddMenuItems(mnuItemActions, _slotGroups, actions, null);
        //    }

        //    // all actions provided by adjuncts
        //    var _adjGroups = (from _a in actions.Where(_a => _a.Provider is AdjunctInfo)
        //                      group _a by _a.ProviderID).ToList();
        //    ActionMenus.AddMenuItems(mnuItemActions, _adjGroups, actions, null);
        //    // TODO: if anchored to an item, use item?...
        //}
        //#endregion
    }
}