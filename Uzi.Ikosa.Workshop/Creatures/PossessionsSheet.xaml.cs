using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.TypeListers;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Items.Wealth;
using Uzi.Ikosa.Adjuncts;
using Uzi.Core;
using System.ComponentModel;
using Uzi.Ikosa.UI;
using Uzi.Ikosa.Universal;
using Uzi.Ikosa.Items.Weapons.Ranged;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for PossessionsSheet.xaml
    /// </summary>
    public partial class PossessionsSheet : UserControl
    {
        public static RoutedCommand StoreItem = new();
        public static RoutedCommand StoreAmmo = new();
        public static RoutedCommand IdentityCmd = new();
        public static RoutedCommand AppraiseCmd = new();

        public PossessionsSheet()
        {
            InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(PossessionsSheet_DataContextChanged);
        }

        protected PresentableCreatureVM PresentableCreature
            => DataContext as PresentableCreatureVM;

        private Generic GetSelected<Generic>()
            where Generic : PresentableContext
            => lstItems?.SelectedItem as Generic;

        private PresentableContext GetSelectedContext()
            => GetSelected<PresentableContext>();

        #region void PossessionsSheet_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        void PossessionsSheet_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // update stow menu
            var _critter = PresentableCreature?.Thing;
            if (_critter != null)
            {
                WeakEventManager<IkosaPossessions, PropertyChangedEventArgs>
                    .AddHandler(_critter.IkosaPosessions, nameof(PossessionSet.PropertyChanged), _possess_PropertyChanged);
                RefreshContainerList();
            }
        }
        #endregion

        #region void _possess_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        void _possess_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // update stow menu
            RefreshContainerList();
        }
        #endregion

        #region private void RefreshContainerList()
        private void RefreshContainerList()
        {
            mnuStow.Items.Clear();
            foreach (var _container in PresentableCreature.ContextualPossessions.OfType<PresentableContainerItemVM>())
            {
                var _mnuItem = new MenuItem
                {
                    Command = PossessionsSheet.StoreItem,
                    CommandParameter = _container
                };
                var _bnd = new Binding(@"Name")
                {
                    Source = _container.Thing
                };
                _mnuItem.SetBinding(MenuItem.HeaderProperty, _bnd);
                mnuStow.Items.Add(_mnuItem);
            }
            foreach (var _container in PresentableCreature.ContextualPossessions.OfType<PresentableSlottedContainerItemVM>())
            {
                var _mnuItem = new MenuItem
                {
                    Command = PossessionsSheet.StoreItem,
                    CommandParameter = _container
                };
                var _bnd = new Binding(@"Name")
                {
                    Source = _container.Thing
                };
                _mnuItem.SetBinding(MenuItem.HeaderProperty, _bnd);
                mnuStow.Items.Add(_mnuItem);
            }
            foreach (var _kr in PresentableCreature.ContextualPossessions.OfType<KeyRingVM>())
            {
                var _mnuItem = new MenuItem
                {
                    Command = PossessionsSheet.StoreItem,
                    CommandParameter = _kr
                };
                var _bnd = new Binding(@"Name")
                {
                    Source = _kr.Thing
                };
                _mnuItem.SetBinding(MenuItem.HeaderProperty, _bnd);
                mnuStow.Items.Add(_mnuItem);
            }
            var _ammoContList =
                PresentableCreature.ContextualPossessions.OfType<QuiverVM>().Cast<PresentableContext>()
                .Union(PresentableCreature.ContextualPossessions.OfType<BoltSashVM>().Cast<PresentableContext>())
                .Union(PresentableCreature.ContextualPossessions.OfType<SlingBagVM>().Cast<PresentableContext>())
                .ToList();
            if (_ammoContList.Count > 0)
            {
                mnuStow.Items.Add(new Separator());
                foreach (var _aCont in _ammoContList)
                {
                    var _mnuItem = new MenuItem
                    {
                        Command = PossessionsSheet.StoreAmmo,
                        CommandParameter = _aCont
                    };
                    var _bnd = new Binding(@"Thing.Name")
                    {
                        Source = _aCont
                    };
                    _mnuItem.SetBinding(MenuItem.HeaderProperty, _bnd);
                    mnuStow.Items.Add(_mnuItem);
                }
            }
        }
        #endregion

        #region cmdbndRemove
        private void cmdbndRemove_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (lstItems != null)
            {
                e.CanExecute = (lstItems.SelectedItem != null) && (GetSelected<NaturalWeaponVM>() == null);
            }
            e.Handled = true;
        }

        private void cmdbndRemove_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _item = GetSelectedContext();
            if (_item != null)
            {
                if (UnContain(_item.CoreObject))
                {
                    if (_item.CoreObject is ICoreItem _thing)
                    {
                        _thing.Possessor = null;
                        PresentableCreature.DoChangedPossessions();
                    }
                }
            }
        }
        #endregion

        #region private void cmdbndNew_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cmdbndNew_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // TODO: toggle use wealth to purchase items...
            e.CanExecute = true;
            e.Handled = true;
        }
        #endregion

        #region Open Item (Containers and Key Rings)
        private void cbOpen_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // suppress open containers
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cbOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if ((e.Parameter is SlottedContainerItemBase)
                || (e.Parameter is ContainerItemBase))
            {
                var _contentsList = new ContainerContentsList()
                {
                    Owner = Window.GetWindow(this),
                    DataContext = e.Parameter
                };
                _contentsList.ShowDialog();
            }
            else if (e.Parameter is KeyRing _keyRing)
            {
                var _contentsList = new KeyRingContents()
                {
                    Owner = Window.GetWindow(this),
                    DataContext = _keyRing
                };
                _contentsList.ShowDialog();
            }
            else if (e.Parameter is ArrowBundle)
            {
            }
            else if (e.Parameter is CrossbowBoltBundle)
            {
            }
            else if (e.Parameter is SlingAmmoBundle)
            {
            }
            else if (e.Parameter is ShurikenBundle)
            {
            }
            else if (e.Parameter is Quiver _quiver)
            {
                var _ammoList = new AmmoContainerContentsList()
                {
                    Owner = Window.GetWindow(this),
                    DataContext = new AmmoBundleVM(_quiver, PresentableCreature)
                };
                _ammoList.ShowDialog();
            }
            else if (e.Parameter is BoltSash _sash)
            {
                var _ammoList = new AmmoContainerContentsList()
                {
                    Owner = Window.GetWindow(this),
                    DataContext = new AmmoBundleVM(_sash, PresentableCreature)
                };
                _ammoList.ShowDialog();
            }
            else if (e.Parameter is SlingBag _slingBag)
            {
                var _ammoList = new AmmoContainerContentsList()
                {
                    Owner = Window.GetWindow(this),
                    DataContext = new AmmoBundleVM(_slingBag, PresentableCreature)
                };
                _ammoList.ShowDialog();
            }
            else if (e.Parameter is ShurikenPouch _pouch)
            {
                var _ammoList = new AmmoContainerContentsList()
                {
                    Owner = Window.GetWindow(this),
                    DataContext = new AmmoBundleVM(_pouch, PresentableCreature)
                };
                _ammoList.ShowDialog();
            }
            e.Handled = true;
        }
        #endregion

        #region New (General)
        private void cmdbndNew_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is ItemTypeListItem _itli)
            {
                var _obj = (Activator.CreateInstance(_itli.ItemType)) as ICoreItem;
                if (_obj != null)
                {
                    _obj.Possessor = PresentableCreature.Thing;
                    PresentableCreature.DoChangedPossessions();
                }
                if (_obj is IItemBase _item)
                {
                    _item.ItemSizer.ExpectedCreatureSize = PresentableCreature.Thing?.Sizer.NaturalSize;
                }
            }
            e.Handled = true;
        }
        #endregion

        #region New Wand
        private void cmdbndNewWand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // TODO: toggle use wealth to purchase items...
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cmdbndNewWand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _source = (e.Parameter as Func<SpellSource>)();
            var _wand = new Wand($@"Wand of {_source.SpellDef.DisplayName} ({_source.CasterLevel})",
                new WoodMaterial(), 5, Size.Tiny);
            _wand.AddAdjunct(new SpellTrigger(_source, new PowerBattery(_wand, 50), 1));
            _wand.Possessor = PresentableCreature.Thing;
            PresentableCreature.DoChangedPossessions();
            e.Handled = true;
        }
        #endregion

        #region New Potion
        private void cmdbndNewPotion_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // TODO: toggle use wealth to purchase items...
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cmdbndNewPotion_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // unpack arguments
            var (_factory, _extra, _filter, _options) =
                ((Func<SpellSource>,
                string,
                Func<IEnumerable<ISpellMode>, ISpellMode>,
                (string optKey, string optValue)[]))e.Parameter;

            // resolve
            var _source = _factory();
            var _mode = _filter(_source.SpellDef.SpellModes);
            var _opts = _options == null
                ? []
                : _options.ToDictionary(_o => _o.optKey, _o => _o.optValue);
            var _potion = new Potion($@"vial of {_mode.DisplayName}{(!string.IsNullOrWhiteSpace(_extra) ? " " : string.Empty)}{_extra}", _source, _extra, _mode, _opts)
            {
                Possessor = PresentableCreature.Thing
            };
            PresentableCreature.DoChangedPossessions();
            e.Handled = true;
        }
        #endregion

        #region New CoinSet
        private void cmdbndNewCoin_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _coin = new CoinSet(new CoinCount { Count = 100, CoinType = GoldPiece.Static })
            {
                Possessor = PresentableCreature.Thing
            };
            PresentableCreature.DoChangedPossessions();
            e.Handled = true;
        }
        #endregion

        #region New KeyItem and KeyRing
        private void cmdbndNewKeyItem_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _keyItem = new KeyItem(@"Key", new Guid[] { })
            {
                Possessor = PresentableCreature.Thing
            };
            PresentableCreature.DoChangedPossessions();
            e.Handled = true;
        }

        private void cmdbndNewKeyRing_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _keyRing = new KeyRing(@"Key Ring")
            {
                Possessor = PresentableCreature.Thing
            };
            PresentableCreature.DoChangedPossessions();
            e.Handled = true;
        }
        #endregion

        #region New AmmoSet
        private void cmdbndNewAmmoSet_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is ItemTypeListItem _itli)
            {
                // get ammunition
                var _ammo = (Activator.CreateInstance(_itli.ItemType)) as IAmmunitionBase;
                var _bundle = _ammo.ToAmmunitionBundle($@"{_ammo.Name} Bundle");
                _bundle.Possessor = PresentableCreature.Thing;
                PresentableCreature.DoChangedPossessions();
            }
            e.Handled = true;
        }
        #endregion

        #region New Gem
        private void cmdbndNewGem_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _material = e.Parameter as GemMaterial;
            var _dlg = new GemName(_material)
            {
                Owner = Window.GetWindow(this)
            };
            if (_dlg.ShowDialog() ?? false)
            {
                var _gem = new Gem(_dlg.TrueName, _dlg.UnknownName,
                    _material.ValueRandomizer.RollValue(Guid.Empty, @"Gem", @"Value"), Size.Fine, 0.0625, 2, _material)
                {
                    Possessor = PresentableCreature.Thing
                };
                PresentableCreature.DoChangedPossessions();
            }
            e.Handled = true;
        }
        #endregion

        #region New SpellBook
        private void cmdbndNewSpellBook_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _book = new SpellBook(@"SpellBook")
            {
                Possessor = PresentableCreature.Thing
            };
            PresentableCreature.DoChangedPossessions();
            e.Handled = true;
        }
        #endregion

        #region New Scroll
        private void cmdbndNewScrollItem_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _scroll = new Scroll(@"Scroll", PaperMaterial.Static, 1, Size.Miniature)
            {
                Possessor = PresentableCreature.Thing
            };
            PresentableCreature.DoChangedPossessions();
            e.Handled = true;
        }
        #endregion

        #region New Ring
        private void cmdbndNewRing_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // TODO: toggle use wealth to purchase items...
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cmdbndNewRing_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is Func<MagicAugment>)
            {
                // create a ring using augment factory
                var _source = (e.Parameter as Func<MagicAugment>)();
                var _id = _source.Augmentation as IIdentification;
                var _ring = new Ring(string.Concat(@"Ring of ",
                    _id?.IdentificationInfos?.FirstOrDefault()?.Message ?? _source.MagicPowerActionSource.PowerDef.DisplayName),
                    GoldPlatingMaterial.Static, 2);
                if ((_source.Augmentation as IItemRequirements)?.RequiresMasterwork ?? true)
                {
                    _ring.AddAdjunct(new Masterwork(typeof(Masterwork)));
                }
                _ring.AddAdjunct(_source);
                _ring.Possessor = PresentableCreature.Thing;
                PresentableCreature.DoChangedPossessions();
            }
            else if (e.Parameter is Func<Ring>)
            {
                // create ring from ring factory
                var _factory = (e.Parameter as Func<Ring>);
                var _ring = _factory();
                _ring.Possessor = PresentableCreature.Thing;
                PresentableCreature.DoChangedPossessions();
            }
            e.Handled = true;
        }
        #endregion

        #region private void lstItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        private void lstItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var _selected = GetSelectedContext();
            if (_selected != null)
            {
                txtPath.Text = _selected?.CoreObject.GetPath();
            }
        }
        #endregion

        #region private bool UnContain(ICoreObject obj)
        private bool UnContain(ICoreObject obj)
        {
            foreach (var _cnt in obj.Adjuncts.OfType<Contained>().ToList())
            {
                _cnt.Container.Remove(obj);
            }
            foreach (var _hld in obj.Adjuncts.OfType<Held>().ToList())
            {
                _hld.HoldingWrapper.ClearSlots();
            }
            foreach (var _slot in obj.Adjuncts.OfType<Slotted>().ToList())
            {
                _slot.ItemSlot.SlottedItem.ClearSlots();
            }
            foreach (var _aCnt in obj.Adjuncts.OfType<AmmoContained>().ToList())
            {
                //_aCnt.Bundle.Extract((obj as IAmmunitionBundle).Prototype);
            }
            return !obj.IsPathed();
        }
        #endregion

        #region Store Item
        private void cmdbndStoreItem_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if ((lstItems != null) && ((lstItems.SelectedItem as PresentableContext)?.CoreObject is ICoreObject _object))
            {
                // only possession that cannot be stored?
                if (GetSelected<NaturalWeaponVM>() == null)
                {
                    // target container cannot be contained within the stowing object
                    e.CanExecute = !(e.Parameter as PresentableContext)?.CoreObject?.DoesPathContain(_object) ?? false;
                }
            }
            e.Handled = true;
        }

        private void cmdbndStoreItem_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _iCore = GetSelectedContext();
            if (_iCore != null)
            {
                var _item = _iCore.CoreObject;
                if (e.Parameter is PresentableContainerItemVM _cont)
                {
                    if (UnContain(_item))
                    {
                        _cont.Thing.Container.Add(_item);
                        lstItems_SelectionChanged(null, null);
                    }
                }
                else if (e.Parameter is PresentableSlottedContainerItemVM _slotCont)
                {
                    if (UnContain(_item))
                    {
                        _slotCont.Thing.Container.Add(_item);
                        lstItems_SelectionChanged(null, null);
                    }
                }
                else if (e.Parameter is KeyRingVM _keyRing)
                {
                    if (UnContain(_item))
                    {
                        _keyRing.Thing.Add(_item);
                        lstItems_SelectionChanged(null, null);
                    }
                }
            }
            e.Handled = true;
        }
        #endregion

        #region Store Ammo
        private void cmdbndStoreAmmo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if ((lstItems != null) && ((lstItems.SelectedItem as PresentableContext)?.CoreObject is IAmmunitionBundle _ammoSet))
            {
                // only possessions that cannot be stored
                if (e.Parameter is QuiverVM _quiver)
                {
                    // must be compatible types
                    if (_quiver.Thing.AmmunitionType.IsAssignableFrom(_ammoSet.AmmunitionType))
                    {
                        // container must be able to hold ammo set
                        if (_quiver.Thing.Count <= _quiver.Thing.Capacity)
                        {
                            // target container cannot be contained within the stowing object
                            e.CanExecute = !_quiver.Thing.DoesPathContain(_ammoSet);
                        }
                    }
                }
                else if (e.Parameter is BoltSashVM _sash)
                {
                    // must be compatible types
                    if (_sash.Thing.AmmunitionType.IsAssignableFrom(_ammoSet.AmmunitionType))
                    {
                        // container must be able to hold ammo set
                        if (_sash.Thing.Count <= _sash.Thing.Capacity)
                        {
                            // target container cannot be contained within the stowing object
                            e.CanExecute = !_sash.Thing.DoesPathContain(_ammoSet);
                        }
                    }
                }
                else if (e.Parameter is SlingBagVM _bag)
                {
                    // must be compatible types
                    if (_bag.Thing.AmmunitionType.IsAssignableFrom(_ammoSet.AmmunitionType))
                    {
                        // container must be able to hold ammo set
                        if (_bag.Thing.Count <= _bag.Thing.Capacity)
                        {
                            // target container cannot be contained within the stowing object
                            e.CanExecute = !_bag.Thing.DoesPathContain(_ammoSet);
                        }
                    }
                }
            }
            e.Handled = true;
        }

        private void cmdbndStoreAmmo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _ammoSet = (lstItems.SelectedItem as PresentableContext)?.CoreObject as IAmmunitionBundle;
            if ((e.Parameter is QuiverVM _aCont) && (_ammoSet is AmmunitionBundle<Arrow, BowBase> _aBundle))
            {
                if (UnContain(_ammoSet))
                {
                    // ammo set will disappear (the merged set in the container will replace in whole or part)
                    _ammoSet.Possessor = null;
                    var _leftOver = _aCont.Thing.Merge(_aBundle);
                    if ((_leftOver != null) && (_leftOver.Count == 0))
                    {
                        // dispossess empty set
                        _leftOver.Possessor = null;
                    }
                    PresentableCreature.DoChangedPossessions();
                    lstItems_SelectionChanged(null, null);
                }
            }
            else if ((e.Parameter is BoltSashVM _bCont) && (_ammoSet is AmmunitionBundle<CrossbowBolt, CrossbowBase> _bBundle))
            {
                if (UnContain(_ammoSet))
                {
                    // ammo set will disappear (the merged set in the container will replace in whole or part)
                    _ammoSet.Possessor = null;
                    var _leftOver = _bCont.Thing.Merge(_bBundle);
                    if ((_leftOver != null) && (_leftOver.Count == 0))
                    {
                        // dispossess empty set
                        _leftOver.Possessor = null;
                    }
                    PresentableCreature.DoChangedPossessions();
                    lstItems_SelectionChanged(null, null);
                }
            }
            if ((e.Parameter is SlingBagVM _sCont) && (_ammoSet is AmmunitionBundle<SlingAmmo, Sling> _sBundle))
            {
                if (UnContain(_ammoSet))
                {
                    // ammo set will disappear (the merged set in the container will replace in whole or part)
                    _ammoSet.Possessor = null;
                    var _leftOver = _sCont.Thing.Merge(_sBundle);
                    if ((_leftOver != null) && (_leftOver.Count == 0))
                    {
                        // dispossess empty set
                        _leftOver.Possessor = null;
                        PresentableCreature.DoChangedPossessions();
                    }
                    lstItems_SelectionChanged(null, null);
                }
            }
            e.Handled = true;
        }
        #endregion

        #region Edit Object Identity
        private void cmdbndIdentity_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if ((lstItems != null) && (GetSelectedContext() != null))
            {
                e.CanExecute = true;
                e.Handled = true;
            }
        }

        private void cmdbndIdentity_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _obj = GetSelectedContext();
            var _dlg = new Window
            {
                Owner = Window.GetWindow(this),
                Content = new ObjectIdentityEditor(_obj.CoreObject as CoreObject, PresentableCreature.Thing),
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,
                WindowStyle = WindowStyle.ToolWindow,
                Title = _obj.CoreObject.Name,
                SizeToContent = SizeToContent.WidthAndHeight
            };
            _dlg.ShowDialog();
            e.Handled = true;
        }
        #endregion

        #region Appraise
        private void cmdbndAppraise_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if ((lstItems != null) && (GetSelectedContext() != null))
            {
                e.CanExecute = true;
                e.Handled = true;
            }
        }

        private void cmdbndAppraise_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _obj = GetSelectedContext();
            var _dlg = new Window
            {
                Owner = Window.GetWindow(this),
                Content = new AppraisalEditor(_obj.CoreObject as CoreObject),
                WindowStyle = WindowStyle.ToolWindow,
                Title = _obj.CoreObject.Name,
                SizeToContent = SizeToContent.WidthAndHeight
            };
            _dlg.ShowDialog();
            e.Handled = true;
        }
        #endregion

        private void cbIconEdit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _ctx = GetSelectedContext();
            var _dlg = new IconEditor(_ctx)
            {
                Owner = Window.GetWindow(this)
            };
            _dlg.ShowDialog();
            PresentableCreature.DoChangedPossessions();
            e.Handled = true;
        }

        #region New SlottedItem
        private void cmdbndNewSlottedItem_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // TODO: toggle use wealth to purchase items...
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cmdbndNewSlottedItem_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _param = e.Parameter as Tuple<Func<string, SlottedItemBase>, Func<MagicAugment>>;
            var _augment = _param.Item2();
            var _id = _augment.Augmentation as IIdentification;
            var _augInfo = _id?.IdentificationInfos?.FirstOrDefault()?.Message
                ?? $@"{_augment.MagicPowerActionSource.PowerDef.DisplayName} ({_augment.MagicPowerActionSource.CasterLevel})";
            var _item = _param.Item1(_augInfo);

            _item.AddAdjunct(_augment);
            _item.Possessor = PresentableCreature.Thing;
            PresentableCreature.DoChangedPossessions();
            e.Handled = true;
        }
        #endregion

        private void cmdbndNewDevotionalSymbolItem_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is Tuple<string, DevotionalDefinition, Material> _param)
            {
                var _devotion = _param.Item1;

                var _symbol = new DevotionalSymbol($@"Symbol of {_devotion}", _devotion, _param.Item3, 5)
                {
                    Possessor = PresentableCreature.Thing
                };
                PresentableCreature.DoChangedPossessions();
                e.Handled = true;
            }
        }
    }
}
