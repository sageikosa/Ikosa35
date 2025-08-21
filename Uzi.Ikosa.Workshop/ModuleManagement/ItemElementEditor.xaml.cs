using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Items.Wealth;
using Uzi.Ikosa.Items.Weapons.Ranged;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.TypeListers;
using Uzi.Ikosa.UI;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for ItemElementEditor.xaml
    /// </summary>
    public partial class ItemElementEditor : UserControl
    {
        public ItemElementEditor()
        {
            InitializeComponent();
        }

        public IHostTabControl HostTabControl
        {
            get => (IHostTabControl)GetValue(HostedTabControlProperty);
            set => SetValue(HostedTabControlProperty, value);
        }

        // Using a DependencyProperty as the backing store for HostedTabControl.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HostedTabControlProperty =
            DependencyProperty.Register(nameof(HostTabControl), typeof(IHostTabControl), typeof(ItemElementEditor),
                new PropertyMetadata(null));

        public ItemElementFolderVM ItemElementFolder => DataContext as ItemElementFolderVM;

        #region Open Item (Containers and Key Rings)
        private void cbOpen_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // suppress open containers
            e.CanExecute = (e.Parameter is PresentableContainerItemVM)
                || (e.Parameter is PresentableSlottedContainerItemVM)
                || (e.Parameter is KeyRingVM)
                || typeof(PresentableAmmunitionBundle<,,>).IsAssignableFrom(e.Parameter.GetType());
            e.Handled = true;
        }

        private void cbOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _edit = new ObjectEditorWindow(e.Parameter as PresentableContext)
            {
                Title = @"Edit Object",
                Owner = Window.GetWindow(this),
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            _ = _edit.ShowDialog();
            e.Handled = true;
        }
        #endregion

        #region New (General)
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
                var _item = Activator.CreateInstance(_itli.ItemType) as IItemBase;
                ItemElementFolder.AddItem(_item);
                ItemElementFolder.Commands = ItemElementFolder.GetDefaultCommands();
            }
            e.Handled = true;
        }
        #endregion

        #region New Wand
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
            ItemElementFolder.AddItem(_wand);
            ItemElementFolder.Commands = ItemElementFolder.GetDefaultCommands();
            e.Handled = true;
        }
        #endregion

        #region New Potion
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
            ItemElementFolder.AddItem(_potion);
            ItemElementFolder.Commands = ItemElementFolder.GetDefaultCommands();
            e.Handled = true;
        }
        #endregion

        #region New CoinSet
        private void cmdbndNewCoin_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _coin = new CoinSet(new CoinCount { Count = 100, CoinType = GoldPiece.Static });
            ItemElementFolder.AddItem(_coin);
            ItemElementFolder.Commands = ItemElementFolder.GetDefaultCommands();
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
                ItemElementFolder.AddItem(_gem);
                ItemElementFolder.Commands = ItemElementFolder.GetDefaultCommands();
            }
            e.Handled = true;
        }
        #endregion

        #region New KeyItem and KeyRing
        private void cmdbndNewKeyItem_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _keyItem = new KeyItem(@"key", new Guid[] { });
            ItemElementFolder.AddItem(_keyItem);
            ItemElementFolder.Commands = ItemElementFolder.GetDefaultCommands();
            e.Handled = true;
        }

        private void cmdbndNewKeyRing_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _keyRing = new KeyRing(@"keyring");
            ItemElementFolder.AddItem(_keyRing);
            ItemElementFolder.Commands = ItemElementFolder.GetDefaultCommands();
            e.Handled = true;
        }
        #endregion

        #region New AmmoSet
        private void cmdbndNewAmmoSet_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is ItemTypeListItem)
            {
                var _itli = e.Parameter as ItemTypeListItem;

                // get ammunition
                var _ammo = Activator.CreateInstance(_itli.ItemType) as IAmmunitionBase;
                var _bundle = _ammo.ToAmmunitionBundle($@"{_ammo.Name} bundle");

                ItemElementFolder.AddItem(_bundle);
                ItemElementFolder.Commands = ItemElementFolder.GetDefaultCommands();
            }
            e.Handled = true;
        }
        #endregion

        #region New SpellBook
        private void cmdbndNewSpellBook_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _book = new SpellBook(@"spellbook");
            ItemElementFolder.AddItem(_book);
            ItemElementFolder.Commands = ItemElementFolder.GetDefaultCommands();
            e.Handled = true;
        }
        #endregion

        #region New Scroll
        private void cmdbndNewScrollItem_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _scroll = new Scroll(@"scroll", PaperMaterial.Static, 1, Size.Miniature);
            ItemElementFolder.AddItem(_scroll);
            ItemElementFolder.Commands = ItemElementFolder.GetDefaultCommands();
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
                ItemElementFolder.AddItem(_ring);
                ItemElementFolder.Commands = ItemElementFolder.GetDefaultCommands();
            }
            else if (e.Parameter is Func<Ring>)
            {
                // create ring from ring factory
                var _factory = e.Parameter as Func<Ring>;
                var _ring = _factory();
                ItemElementFolder.AddItem(_ring);
                ItemElementFolder.Commands = ItemElementFolder.GetDefaultCommands();
            }
            e.Handled = true;
        }
        #endregion

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

            _ = _item.AddAdjunct(_augment);
            ItemElementFolder.AddItem(_item);
            ItemElementFolder.Commands = ItemElementFolder.GetDefaultCommands();
            e.Handled = true;
        }
        #endregion

        private void cmdbndNewDevotionalSymbolItem_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is Tuple<string, DevotionalDefinition, Material> _param)
            {
                var _devotion = _param.Item1;

                var _symbol = new DevotionalSymbol($@"Symbol of {_devotion}", _devotion, _param.Item3, 5);
                ItemElementFolder.AddItem(_symbol);
                ItemElementFolder.Commands = ItemElementFolder.GetDefaultCommands();
                e.Handled = true;
            }
        }

        private void btnIconEdit_Click(object sender, RoutedEventArgs e)
        {
            var _dlg = new IconEditor(ItemElementFolder?.SelectedItemElement.ObjectPresentation)
            {
                Owner = Window.GetWindow(this)
            };
            _ = _dlg.ShowDialog();

            // rebind context
            var _ctx = DataContext;
            DataContext = null;
            DataContext = _ctx;

            e.Handled = true;
        }
    }
}
