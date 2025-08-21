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
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>Interaction logic for EquipmentSheet.xaml</summary>
    public partial class EquipmentSheet : UserControl
    {
        public static RoutedCommand SlotItemCommand = new();
        public static RoutedCommand DoubleSlotItemCommand = new();
        public static RoutedCommand UnslotItemCommand = new();
        public static RoutedCommand HoldItemCommand = new();
        public static RoutedCommand MountItemCommand = new();
        public static RoutedCommand UnmountItemCommand = new();

        // NOTE: the next two are used if the slot is holding a double weapon
        public static RoutedCommand SwapMainHeadCommand = new();
        public static RoutedCommand UseTwoHandedCommand = new();
        public static RoutedCommand UseDoubleCommand = new();

        public EquipmentSheet()
        {
            InitializeComponent();
        }

        protected PresentableCreatureVM PresentableCreature
            => DataContext as PresentableCreatureVM;

        private Creature Creature
            => PresentableCreature.Thing;

        #region private void tbControl_MouseDown(object sender, MouseButtonEventArgs e)
        private void tbControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var _tb = sender as TextBlock;
            var _itemSlot = (_tb.Tag as PresentableItemSlotVM).ItemSlot;

            var _menu = new ContextMenu();
            _menu.Items.Add(new MenuItem
            {
                Header = @"Clear Slot",
                Command = UnslotItemCommand,
                CommandParameter = _itemSlot
            });
            _menu.Items.Add(new Separator());

            switch (_itemSlot.SlotType)
            {
                case ItemSlot.HoldingSlot:
                    #region slot wielding
                    // any item that can be slotted into a holding slot directly
                    var _doSlot = new MenuItem { Header = @"Wield" };
                    _menu.Items.Add(_doSlot);
                    var _doDoubleSlot = new MenuItem { Header = @"Two-Slot Wield" };
                    _menu.Items.Add(_doDoubleSlot);
                    foreach (var _item in from _i in PresentableCreature.ContextualPossessions
                                          let _si = _i.CoreObject as ISlottedItem
                                          where (_si != null)
                                            && _si.SlotType.Equals(_itemSlot.SlotType, StringComparison.OrdinalIgnoreCase)
                                            && (_si.MainSlot == null)
                                          let _n = _si as NaturalWeapon
                                          where (_n == null)
                                            || string.IsNullOrEmpty(_n.SlotSubType)
                                            || _n.SlotSubType.Equals(_itemSlot.SubType, StringComparison.OrdinalIgnoreCase)
                                          select new { presentable = _i, slotted = _si })
                    {
                        // NOTE: even two-hand weapons can be wielded in one hand, but they might not provide actions
                        _doSlot.Items.Add(new MenuItem
                        {
                            Header = _item.presentable,
                            Command = SlotItemCommand,
                            CommandParameter = new KeyValuePair<ItemSlot, ISlottedItem>(_itemSlot, _item.slotted)
                        });
                        if (!(_item.presentable is NaturalWeaponVM))
                        {
                            // do not allow natural weapons to be double-wielded
                            _doDoubleSlot.Items.Add(new MenuItem
                            {
                                Header = _item,
                                Command = DoubleSlotItemCommand,
                                CommandParameter = new KeyValuePair<ItemSlot, ISlottedItem>(_itemSlot, _item.slotted)
                            });
                        }
                    }
                    #endregion

                    _menu.Items.Add(new Separator());

                    #region holding
                    // holding wrapper for practically any item (except certain sets?)...
                    var _doHold = new MenuItem { Header = @"Hold" };
                    _menu.Items.Add(_doHold);
                    foreach (var _item in from _i in PresentableCreature.ContextualPossessions
                                          where !(_i is NaturalWeaponVM)
                                          let _si = _i.CoreObject as ISlottedItem
                                          where ((_si == null)
                                          || ((_si.SlotType != _itemSlot.SlotType) && (_si.MainSlot == null)))
                                          select new { presentable = _i, slotted = _si })
                    {
                        _doHold.Items.Add(new MenuItem
                        {
                            Header = _item.presentable,
                            Command = HoldItemCommand,
                            CommandParameter = new KeyValuePair<ItemSlot, CoreItem>(_itemSlot, _item.slotted as CoreItem)
                        });
                    }
                    #endregion
                    break;

                case ItemSlot.WieldMount:
                case ItemSlot.LargeWieldMount:
                case ItemSlot.BackShieldMount:
                    _menu.Items.Insert(1, new MenuItem
                    {
                        Header = @"Unmount",
                        Command = UnmountItemCommand,
                        CommandParameter = _itemSlot
                    });
                    // slotting
                    foreach (var _item in from _i in PresentableCreature.ContextualPossessions
                                          let _si = _i.CoreObject as ISlottedItem
                                          where (_si != null)
                                            && (_si.SlotType == _itemSlot.SlotType)
                                            && (_si.MainSlot == null)
                                          select new { presentable = _i, slotted = _si })
                    {
                        _menu.Items.Add(new MenuItem
                        {
                            Header = _item.presentable,
                            Command = SlotItemCommand,
                            CommandParameter = new KeyValuePair<ItemSlot, ISlottedItem>(_itemSlot, _item.slotted)
                        });
                    }

                    var _mountSlot = _itemSlot as MountSlot;
                    if (_mountSlot != null)
                    {
                        _menu.Items.Add(new Separator());

                        // mounting
                        var _mount = new MenuItem { Header = @"Mount" };
                        _menu.Items.Add(_mount);
                        foreach (var _item in from _i in PresentableCreature.ContextualPossessions
                                              let _wm = _i.CoreObject as IWieldMountable
                                              where (_wm != null)
                                                && _wm.SlotTypes.Contains(_mountSlot.SlotType)
                                                && !WieldMounted.IsWieldMounted(_wm)
                                              select new { presentable = _i, wieldable = _wm })
                        {
                            _mount.Items.Add(new MenuItem
                            {
                                Header = _item.presentable,
                                Command = MountItemCommand,
                                CommandParameter = new KeyValuePair<MountSlot, IWieldMountable>(_mountSlot, _item.wieldable)
                            });
                        }
                    }
                    break;

                default:
                    // slotting
                    foreach (var _item in from _i in PresentableCreature.ContextualPossessions
                                          let _si = _i.CoreObject as ISlottedItem
                                          where (_si != null)
                                            && (_si.SlotType == _itemSlot.SlotType)
                                            && (_si.MainSlot == null)
                                          select new { presentable = _i, slotted = _si })
                    {
                        _menu.Items.Add(new MenuItem
                        {
                            Header = _item.presentable,
                            Command = SlotItemCommand,
                            CommandParameter = new KeyValuePair<ItemSlot, ISlottedItem>(_itemSlot, _item.slotted)
                        });
                    }

                    break;
            }

            #region double weapon commands
            // double weapon commands
            if ((_itemSlot.SlottedItem != null)
                && _itemSlot.SlottedItem is DoubleMeleeWeaponBase)
            {
                var _dbl = _itemSlot.SlottedItem as DoubleMeleeWeaponBase;
                _menu.Items.Add(new Separator());
                _menu.Items.Add(new MenuItem
                {
                    Header = @"Swap Main Head",
                    Command = SwapMainHeadCommand,
                    CommandParameter = _dbl
                });
                _menu.Items.Add(new MenuItem
                {
                    Header = @"Use as Double Weapon",
                    Command = UseDoubleCommand,
                    CommandParameter = _dbl
                });
                _menu.Items.Add(new MenuItem
                {
                    Header = @"Use as Two-Handed Weapon",
                    Command = UseTwoHandedCommand,
                    CommandParameter = _dbl
                });
            }
            #endregion

            // show
            _menu.PlacementTarget = _tb;
            _menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            _menu.IsOpen = true;
        }
        #endregion

        #region private void ClearAssociations(IAdjunctable item)
        private void ClearAssociations(IAdjunctable item)
        {
            var _slotItem = item as ISlottedItem;
            if ((_slotItem != null) && ((_slotItem.MainSlot != null) || (_slotItem.SecondarySlot != null)))
            {
                _slotItem.ClearSlots();
            }

            foreach (var _held in item.Adjuncts.OfType<Held>().ToList())
            {
                _held.HoldingWrapper.ClearSlots();
            }

            foreach (var _mounted in item.Adjuncts.OfType<WieldMounted>().ToList())
            {
                var _wrapper = _mounted.MountSlot.MountWrapper;
                if (_wrapper != null)
                {
                    _wrapper.UnmountItem();
                }
            }

            foreach (var _contained in item.Adjuncts.OfType<Contained>().ToList())
            {
                _contained.Container.Remove(item as ICoreObject);
            }

            foreach (var _tokened in item.Adjuncts.OfType<Tokened>().ToList())
            {
                _tokened.Eject();
            }
        }
        #endregion

        #region Slot Item
        private void bndSlotItemCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var _kvp = (KeyValuePair<ItemSlot, ISlottedItem>)e.Parameter;
            e.CanExecute = (_kvp.Key.SlottedItem != _kvp.Value);
            e.Handled = true;
        }

        private void bndSlotItemCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _kvp = (KeyValuePair<ItemSlot, ISlottedItem>)e.Parameter;
            ClearAssociations(_kvp.Value);
            _kvp.Value.SetItemSlot(_kvp.Key);
            PresentableCreature.Rebind();
            PresentableCreature.DoChangedItemSlots();
            e.Handled = true;
        }
        #endregion

        #region Unslot Item
        private void bndUnslotItemCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (e.Parameter is ItemSlot _itemSlot)
                && _itemSlot.AllowUnSlotAction
                && (_itemSlot.SlottedItem != null)
                && (_itemSlot.SlottedItem.UnslottingTime != null);
            e.Handled = true;
        }

        private void bndUnslotItemCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _itemSlot = e.Parameter as ItemSlot;
            _itemSlot.SlottedItem.ClearSlots();
            PresentableCreature.Rebind();
            PresentableCreature.DoChangedItemSlots();
            e.Handled = true;
        }
        #endregion

        #region Double Slot Item
        private void bndDoubleSlotItemCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var _kvp = (KeyValuePair<ItemSlot, ISlottedItem>)e.Parameter;
            var _avail = Creature.Body.ItemSlots.AllSlots
                .Select(_is => _is.SlotType.Equals(ItemSlot.HoldingSlot, StringComparison.OrdinalIgnoreCase) && _is != _kvp.Key)
                .ToList();
            e.CanExecute = (_avail.Count > 0) && (_kvp.Key.SlottedItem != _kvp.Value);
            e.Handled = true;
        }

        private void bndDoubleSlotItemCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _kvp = (KeyValuePair<ItemSlot, ISlottedItem>)e.Parameter;
            ClearAssociations(_kvp.Value);
            var _avail = Creature.Body.ItemSlots.AllSlots
                .Where(_is => _is.SlotType.Equals(ItemSlot.HoldingSlot, StringComparison.OrdinalIgnoreCase) && _is != _kvp.Key)
                .ToList();
            if (_avail.Count > 0)
            {
                var _empty = _avail.FirstOrDefault(_is => _is.SlottedItem == null);
                if (_empty != null)
                {
                    _kvp.Value.SetItemSlot(_kvp.Key, _empty);
                }
                else
                {
                    _kvp.Value.SetItemSlot(_kvp.Key, _avail.First());
                }
            }
            PresentableCreature.Rebind();
            PresentableCreature.DoChangedItemSlots();
            e.Handled = true;
        }
        #endregion

        #region Hold Item
        private void bndHoldItemCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void bndHoldItemCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _kvp = (KeyValuePair<ItemSlot, CoreItem>)e.Parameter;
            ClearAssociations(_kvp.Value);
            var _wrapper = new HoldingWrapper(Creature, _kvp.Value);
            _wrapper.SetItemSlot(_kvp.Key);
            PresentableCreature.Rebind();
            PresentableCreature.DoChangedItemSlots();
            e.Handled = true;
        }
        #endregion

        #region Swap Main Head
        private void bndSwapMainCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void bndSwapMainCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _dbl = e.Parameter as DoubleMeleeWeaponBase;
            _dbl.MainHeadIndex = Math.Abs(_dbl.MainHeadIndex - 1); // abs(1 - 1)=0; abs(0 - 1)=1
            PresentableCreature.Rebind();
            PresentableCreature.DoChangedItemSlots();
            e.Handled = true;
        }
        #endregion

        #region Use as Double Weapon
        private void bndUseDoubleCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var _dbl = e.Parameter as DoubleMeleeWeaponBase;
            e.CanExecute = (_dbl.SecondarySlot != null) && _dbl.UseAsTwoHanded;
            e.Handled = true;
        }

        private void bndUseDoubleCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _dbl = e.Parameter as DoubleMeleeWeaponBase;
            _dbl.UseAsTwoHanded = false;
            PresentableCreature.Rebind();
            PresentableCreature.DoChangedItemSlots();
            e.Handled = true;
        }
        #endregion

        #region Use as Two-Handed Weapon
        private void bndUseTwoHandedCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var _dbl = e.Parameter as DoubleMeleeWeaponBase;
            e.CanExecute = (_dbl.SecondarySlot != null) && !_dbl.UseAsTwoHanded;
            e.Handled = true;
        }

        private void bndUseTwoHandedCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _dbl = e.Parameter as DoubleMeleeWeaponBase;
            _dbl.UseAsTwoHanded = true;
            PresentableCreature.Rebind();
            PresentableCreature.DoChangedItemSlots();
            e.Handled = true;
        }
        #endregion

        #region Mount Item in Mount Slot
        private void cbMountItemCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var _kvp = (KeyValuePair<MountSlot, IWieldMountable>)e.Parameter;
            e.CanExecute = (_kvp.Key.MountedItem != _kvp.Value);
            e.Handled = true;
        }

        private void cbMountItemCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _kvp = (KeyValuePair<MountSlot, IWieldMountable>)e.Parameter;
            ClearAssociations(_kvp.Value);
            if (_kvp.Key.MountWrapper == null)
            {
                var _wrap = new MountWrapper(Creature, _kvp.Value, _kvp.Key.SlotType);
                _wrap.SetItemSlot(_kvp.Key);
            }
            else
            {
                _kvp.Key.MountWrapper.MountItem(_kvp.Value);
            }
            PresentableCreature.Rebind();
            PresentableCreature.DoChangedItemSlots();
            e.Handled = true;
        }
        #endregion

        #region Unmount Item from Mount Slot
        private void cbUnmountItemCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var _mountSlot = e.Parameter as MountSlot;
            e.CanExecute = (_mountSlot != null) && (_mountSlot.MountedItem != null);
            e.Handled = true;
        }

        private void cbUnmountItemCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _mountSlot = e.Parameter as MountSlot;
            if (_mountSlot != null)
            {
                _mountSlot.MountWrapper.UnmountItem();
            }

            PresentableCreature.Rebind();
            PresentableCreature.DoChangedItemSlots();
            e.Handled = true;
        }
        #endregion
    }
}
