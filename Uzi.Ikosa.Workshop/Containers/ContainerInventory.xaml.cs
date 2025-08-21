using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Uzi.Core;
using Uzi.Ikosa.TypeListers;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Items.Wealth;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Universal;
using Uzi.Ikosa.Items.Weapons.Ranged;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for ContainerInventory.xaml
    /// </summary>
    public partial class ContainerInventory : UserControl
    {
        public ContainerInventory()
        {
            InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(ContainerInventory_DataContextChanged);
        }

        private void ContainerInventory_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
            => mnuAddItem.DataContext = VM?.VisualResources;

        private ContainerObject _Container => VM?.Thing;
        private PresentableContainerObjectVM VM => DataContext as PresentableContainerObjectVM;

        private void cmdbndNew_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // NOTE: checking container max weight would require creating item from type
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cmdbndNew_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is ItemTypeListItem)
            {
                var _itli = e.Parameter as ItemTypeListItem;
                var _obj = Activator.CreateInstance(_itli.ItemType) as ICoreObject;
                if (_Container.CanHold(_obj))
                {
                    _Container.Add(_obj);
                }

                VM?.DoChangedContents();
            }
            e.Handled = true;
        }

        private void cmdbndRemove_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (lstItems != null)
            {
                e.CanExecute = lstItems.SelectedItem != null;
                e.Handled = true;
            }
        }

        private void cmdbndRemove_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _ = _Container.Remove((lstItems.SelectedItem as PresentableContext)?.CoreObject);
            e.Handled = true;
            VM?.DoChangedContents();
        }

        private void cbIconEdit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _dlg = new IconEditor(lstItems.SelectedItem as PresentableContext)
            {
                Owner = Window.GetWindow(this)
            };
            _ = _dlg.ShowDialog();
            VM?.DoChangedContents();
            e.Handled = true;
        }

        private void cmdbndNewWand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // NOTE: checking container max weight would require creating item
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cmdbndNewWand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _source = (e.Parameter as Func<SpellSource>)();
            var _wand = new Wand($@"wand of {_source.SpellDef.DisplayName} ({_source.CasterLevel})",
                new WoodMaterial(), 5, Size.Tiny);
            _ = _wand.AddAdjunct(new SpellTrigger(_source, new PowerBattery(_wand, 50), 1));
            if (_Container.CanHold(_wand))
            {
                _Container.Add(_wand);
            }

            VM?.DoChangedContents();
            e.Handled = true;
        }

        private void cmdbndNewPotion_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // NOTE: checking container max weight would require creating item
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
            var _potion = new Potion($@"vial of {_mode.DisplayName}{(!string.IsNullOrWhiteSpace(_extra) ? " " : string.Empty)}{_extra}", _source, _extra, _mode, _opts);
            if (_Container.CanHold(_potion))
            {
                _Container.Add(_potion);
            }

            VM?.DoChangedContents();
            e.Handled = true;
        }

        private void cmdbndNewCoin_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _coin = new CoinSet(new CoinCount { Count = 100, CoinType = GoldPiece.Static });
            if (_Container.CanHold(_coin))
            {
                _Container.Add(_coin);
            }

            VM?.DoChangedContents();
            e.Handled = true;
        }

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
                    _material.ValueRandomizer.RollValue(Guid.Empty, @"Gem", @"Value"), Size.Fine, 0.0625, 2, _material);
                if (_Container.CanHold(_gem))
                {
                    _Container.Add(_gem);
                }

                VM?.DoChangedContents();
            }
            e.Handled = true;
        }

        private void cmdbndNewKeyItem_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _keyItem = new KeyItem(@"key", new Guid[] { });
            if (_Container.CanHold(_keyItem))
            {
                _Container.Add(_keyItem);
            }

            VM?.DoChangedContents();
            e.Handled = true;
        }

        private void cmdbndNewKeyRing_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _keyRing = new KeyRing(@"keyring");
            if (_Container.CanHold(_keyRing))
            {
                _Container.Add(_keyRing);
            }

            VM?.DoChangedContents();
            e.Handled = true;
        }

        private void cmdbndNewAmmoSet_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is ItemTypeListItem)
            {
                var _itli = e.Parameter as ItemTypeListItem;

                // get ammunition
                var _ammo = Activator.CreateInstance(_itli.ItemType) as IAmmunitionBase;
                var _bundle = _ammo.ToAmmunitionBundle($@"{_ammo.Name} bundle");

                if (_Container.CanHold(_bundle))
                {
                    _Container.Add(_bundle);
                }

                VM?.DoChangedContents();
            }
            e.Handled = true;
        }

        private void cmdbndNewSpellBook_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _book = new SpellBook(@"spellbook");
            if (_Container.CanHold(_book))
            {
                _Container.Add(_book);
            }

            VM?.DoChangedContents();
            e.Handled = true;
        }

        private void cmdbndNewScrollItem_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _scroll = new Scroll(@"scroll", PaperMaterial.Static, 1, Size.Miniature);
            if (_Container.CanHold(_scroll))
            {
                _Container.Add(_scroll);
            }

            VM?.DoChangedContents();
            e.Handled = true;
        }

        #region Ring CanExecute
        private void cmdbndNewRing_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // TODO: toggle use wealth to purchase items...
            e.CanExecute = true;
            e.Handled = true;
        }
        #endregion

        #region Ring Executed
        private void cmdbndNewRing_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is Func<MagicAugment>)
            {
                // create a ring using augment factory
                var _augment = (e.Parameter as Func<MagicAugment>)();
                var _id = _augment.Augmentation as IIdentification;
                var _ring = new Ring(string.Concat(@"Ring of ",
                    _id?.IdentificationInfos?.FirstOrDefault()?.Message ?? _augment.MagicPowerActionSource.PowerDef.DisplayName),
                    GoldPlatingMaterial.Static, 2);
                if ((_augment.Augmentation as IItemRequirements)?.RequiresMasterwork ?? true)
                {
                    _ = _ring.AddAdjunct(new Masterwork(typeof(Masterwork)));
                }
                _ = _ring.AddAdjunct(_augment);
                if (_Container.CanHold(_ring))
                {
                    _Container.Add(_ring);
                }

                VM?.DoChangedContents();
            }
            else if (e.Parameter is Func<Ring>)
            {
                // create ring from ring factory
                var _factory = e.Parameter as Func<Ring>;
                var _ring = _factory();
                if (_Container.CanHold(_ring))
                {
                    _Container.Add(_ring);
                }

                VM?.DoChangedContents();
            }
            e.Handled = true;
        }
        #endregion

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

            _ = _item.AddAdjunct(_augment);
            if (_Container.CanHold(_item))
            {
                _Container.Add(_item);
            }

            VM?.DoChangedContents();
            e.Handled = true;
        }

        private void cmdbndNewDevotionalSymbolItem_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is Tuple<string, DevotionalDefinition, Material> _param)
            {
                var _devotion = _param.Item1;

                var _symbol = new DevotionalSymbol($@"Symbol of {_devotion}", _devotion, _param.Item3, 5);
                if (_Container.CanHold(_symbol))
                {
                    _Container.Add(_symbol);
                }

                VM?.DoChangedContents();
                e.Handled = true;
            }
        }
    }
}
