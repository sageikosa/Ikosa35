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
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Client
{
    /// <summary>
    /// Interaction logic for InventoryAdmin.xaml
    /// </summary>
    public partial class InventoryAdmin : UserControl
    {
        public static RoutedCommand ShowList = new RoutedCommand();

        public InventoryAdmin()
        {
            try { InitializeComponent(); } catch { }
        }

        public IEnumerable<ActionInfo> Actions
        {
            get { return (IEnumerable<ActionInfo>)GetValue(ActionsProperty); }
            set { SetValue(ActionsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LocalActionPanel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActionsProperty =
            DependencyProperty.Register(@"ActionsProperty", typeof(IEnumerable<ActionInfo>), typeof(InventoryAdmin),
            new UIPropertyMetadata(ActionsChanged));

        private static void ActionsChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs args)
        {
            var _invAdmin = depObj as InventoryAdmin;
            _invAdmin.RefreshActionButtons();
        }

        public void SetItemSlotInfo(ItemSlotInfo[] slots)
        {
            lvMainItemSlots.ItemsSource = slots.Where(_s => !_s.SlotType.Contains(@"-Mount")).ToList();
            lvMainMountSlots.ItemsSource = slots.Where(_s => _s.SlotType.Contains(@"-Mount")).ToList();
            lvOtherMountSlots.ItemsSource = slots.Where(_s => _s.SlotType.Contains(@"-Mount")).ToList();
        }

        #region Main ShowList
        private void cbMainShowList_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter != null)
                e.CanExecute = (e.Parameter as ListView).Visibility == System.Windows.Visibility.Collapsed;
            e.Handled = true;
        }

        private void cbMainShowList_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (var _lv in gridMain.Children.OfType<ListView>().Where(_l => _l != e.Parameter))
                _lv.Visibility = System.Windows.Visibility.Collapsed;
            (e.Parameter as ListView).Visibility = System.Windows.Visibility.Visible;
            lblMySide.Content = (e.Parameter as ListView).Tag;
            e.Handled = true;
        }
        #endregion

        #region Other ShowList
        private void cbOtherShowList_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter != null)
                e.CanExecute = (e.Parameter as ListView).Visibility == System.Windows.Visibility.Collapsed;
            e.Handled = true;
        }

        private void cbOtherShowList_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (var _lv in gridOther.Children.OfType<ListView>().Where(_l => _l != e.Parameter))
                _lv.Visibility = System.Windows.Visibility.Collapsed;
            (e.Parameter as ListView).Visibility = System.Windows.Visibility.Visible;
            lblOtherSide.Content = (e.Parameter as ListView).Tag;
            e.Handled = true;
        }
        #endregion

        private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source == lvMainContainers)
                cntMain.Content = lvMainContainers.SelectedItem;
            else if (e.Source == lvMainItemSlots)
                cntMain.Content = lvMainItemSlots.SelectedItem;
            else if (e.Source == lvMainMountSlots)
                cntMain.Content = lvMainMountSlots.SelectedItem;
            else if (e.Source == lvOtherContainers)
                cntOther.Content = lvOtherContainers.SelectedItem;
            else if (e.Source == lvOtherMountSlots)
                cntOther.Content = lvOtherMountSlots.SelectedItem;
            else if (e.Source == lvOtherObjects)
                cntOther.Content = lvOtherObjects.SelectedItem;

            RefreshActionButtons();
        }

        private void RefreshActionButtons()
        {
            // actions for selected items
            spActions.Children.Clear();
            var _mounts = (new MountSlotInfo[] { MainMountSlot(), OtherMountSlot() }).Where(_ms => _ms != null).ToList();
            var _holdings = (new ItemSlotInfo[] { MainHoldingSlot(), OtherMountSlot() }).Where(_hs => _hs != null).ToList();
            // TODO: containers?
            if (_mounts.Any() && _holdings.Any())
            {
                var _myMount = _mounts.First();
                var _myHolding = _holdings.First();
                if ((_myHolding.ItemInfo == null) && (_myMount.MountedItem != null))
                {
                    var _act = Actions.FirstOrDefault(_a => (_a.ID == _myMount.ID) && _a.Key == @"DrawWieldable");
                    if (_act != null)
                    {
                        var _aim = _act.AimingModes.OfType<OptionAimInfo>().FirstOrDefault();
                        var _opt = _aim.Options.FirstOrDefault(_o => _o.Key == _myHolding.ID.ToString());
                        var _trg = new OptionTargetInfo
                        {
                            Key = _aim.Key,
                            OptionKey = _opt.Key
                        };
                        spActions.Children.Add(new Button
                        {
                            Content = _act.DisplayName,
                            // NOTE: referenced code, removed definition
                            // Command = ActionMenus.DoAction,
                            CommandParameter = new Tuple<ActionInfo, AimTargetInfo>(_act, _trg)
                        });
                    }
                }
                else if ((_myMount.MountedItem == null) && (_myHolding.ItemInfo != null))
                {
                    var _act = Actions.FirstOrDefault(_a => (_a.ID == _myMount.ID) && _a.Key == @"SheatheWieldable");
                    if (_act != null)
                    {
                        var _aim = _act.AimingModes.OfType<OptionAimInfo>().FirstOrDefault();
                        var _opt = _aim.Options.FirstOrDefault(_o => _o.Key == _myHolding.ItemInfo.ID.ToString());
                        var _trg = new OptionTargetInfo
                        {
                            Key = _aim.Key,
                            OptionKey = _opt.Key
                        };
                        spActions.Children.Add(new Button
                        {
                            Content = _act.DisplayName,
                            // NOTE: referenced code, removed definition
                            // Command = ActionMenus.DoAction,
                            CommandParameter = new Tuple<ActionInfo, AimTargetInfo>(_act, _trg)
                        });
                    }
                }
                // TODO: nothing (incompatible mount slot, or no items in either)
            }
            else if (_holdings.Count == 2)
            {
                // TODO: possibly switch item to an empty hand
                // TODO: possibly swap items?
            }
            else if (_mounts.Count == 2)
            {
                // TODO: probably not much unless an empty hand is available to move an item from one mount to another
            }
            else if (_holdings.Count == 1)
            {
                // TODO: something in hand to go away
                // TODO: something from a container to go into an empty hand...
            }
            else
            {
                // TODO: .... other item types (containers, nearby objects, 
            }
        }

        private ItemSlotInfo MainHoldingSlot()
        {
            var _slot = cntMain.Content as ItemSlotInfo;
            if ((_slot!=null) && (_slot.SlotType == @"Holding"))
                return _slot;
            return null;
        }

        private ItemSlotInfo OtherHoldingSlot()
        {
            var _slot = cntOther.Content as ItemSlotInfo;
            if ((_slot!=null) && (_slot.SlotType == @"Holding"))
                return _slot;
            return null;
        }

        private MountSlotInfo MainMountSlot()
        {
            return cntMain.Content as MountSlotInfo;
        }

        private MountSlotInfo OtherMountSlot()
        {
            return cntOther.Content as MountSlotInfo;
        }
    }
}
