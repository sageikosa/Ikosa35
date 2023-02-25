using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Uzi.Ikosa.Universal;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Advancement.CharacterClasses;
using Uzi.Ikosa.Magic.Spells;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.TypeListers;
using Uzi.Ikosa.Items.Alchemal;
using Uzi.Ikosa.Contracts;
using Uzi.Visualize;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Skills;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Movement;
using Uzi.Visualize.Contracts.Tactical;
using Uzi.Visualize.Packaging;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class AddItemMenu : MenuItem
    {
        private VisualResources VisualResources => DataContext as VisualResources;

        public static RoutedCommand NewItem = new();
        public static RoutedCommand NewPotionItem = new();
        public static RoutedCommand NewWandItem = new();
        public static RoutedCommand NewRingItem = new();
        public static RoutedCommand NewSlottedItem = new();
        public static RoutedCommand NewScrollItem = new();
        public static RoutedCommand NewSpellBook = new();
        public static RoutedCommand NewDevotionalItem = new();
        public static RoutedCommand NewCoins = new();
        public static RoutedCommand NewGem = new();
        public static RoutedCommand NewKeyItem = new();
        public static RoutedCommand NewKeyRing = new();
        public static RoutedCommand NewAmmoSet = new();

        #region GetHeader()
        private Visual GetVisual(string key, IIconReference iconRef)
            => VisualResources?.ResolveIconVisual(key, iconRef) ?? new Canvas { Width = 72, Height = 72 };

        private Visual GetHeader(ItemTypeListItem itemType)
            => GetHeader(itemType.Info.IconKey, new IconReferenceInfo { IconScale = 1 }, itemType.Info.Name, itemType.Info.Description);

        private Visual GetHeader(string iconKey, IIconReference iconRef, string name, string description)
        {
            var _stack = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            // icon
            var _cnt = GetVisual(iconKey, iconRef);
            if ((_cnt as Canvas)?.Tag?.ToString().Contains(@"R") ?? false)
            {
                var _lt = new TransformGroup();
                _lt.Children.Add(new RotateTransform(-22.5, 48, 48));
                _lt.Children.Add(new ScaleTransform(0.5, 0.5));
                _ = _stack.Children.Add(new ContentControl
                {
                    Content = _cnt,
                    LayoutTransform = _lt
                });
            }
            else
            {
                _ = _stack.Children.Add(new ContentControl
                {
                    Content = _cnt,
                    LayoutTransform = new ScaleTransform(0.5, 0.5)
                });
            }

            // info
            var _infoStack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center
            };
            _ = _infoStack.Children.Add(new TextBlock
            {
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Left,
                Text = name
            });
            _ = _infoStack.Children.Add(new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Text = description
            });
            _ = _stack.Children.Add(_infoStack);
            return _stack;
        }
        #endregion

        public AddItemMenu()
        {
            InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(AddItemMenu_DataContextChanged);
        }

        private void AddItemMenu_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var _camp = Campaign.SystemCampaign;

            #region weapons
            // weapons
            miAddWeaponSimple.Items.Clear();
            miAddWeaponMartial.Items.Clear();
            miAddWeaponExotic.Items.Clear();
            foreach (var _wpn in _camp.SimpleWeapons.Where(_w => !typeof(NaturalWeapon).IsAssignableFrom(_w.Value.ItemType)))
            {
                _ = miAddWeaponSimple.Items.Add(new MenuItem
                {
                    Header = GetHeader(_wpn.Value),
                    Command = NewItem,
                    CommandParameter = _wpn.Value
                });
            }
            foreach (var _wpn in _camp.MartialWeapons.Where(_w => !typeof(NaturalWeapon).IsAssignableFrom(_w.Value.ItemType)))
            {
                _ = miAddWeaponMartial.Items.Add(new MenuItem
                {
                    Header = GetHeader(_wpn.Value),
                    Command = NewItem,
                    CommandParameter = _wpn.Value
                });
            }
            foreach (var _wpn in _camp.ExoticWeapons.Where(_w => !typeof(NaturalWeapon).IsAssignableFrom(_w.Value.ItemType)))
            {
                _ = miAddWeaponExotic.Items.Add(new MenuItem
                {
                    Header = GetHeader(_wpn.Value),
                    Command = NewItem,
                    CommandParameter = _wpn.Value
                });
            }
            #endregion

            #region ammo
            // ammunition
            miAddAmmunition.Items.Clear();
            foreach (var _ammo in _camp.AmmunitionTypes)
            {
                _ = miAddAmmunition.Items.Add(new MenuItem
                {
                    Header = GetHeader(_ammo.Value),
                    Command = NewAmmoSet,
                    CommandParameter = _ammo.Value
                });
            }
            #endregion

            // build an ItemTypeListItem
            ItemTypeListItem _itli(Type objType)
            {
                try
                {
                    return new ItemTypeListItem(objType, (ItemInfoAttribute)objType.GetCustomAttributes(typeof(ItemInfoAttribute), true)[0]);
                }
                catch
                {
                    return new ItemTypeListItem(objType, new ItemInfoAttribute(objType.Name, objType.FullName, string.Empty));
                }
            }

            #region ammo containers
            miAddAmmoContainer.Items.Clear();
            var _qtli = _itli(typeof(Quiver));
            _ = miAddAmmoContainer.Items.Add(new MenuItem
            {
                Header = GetHeader(_qtli),
                Command = NewItem,
                CommandParameter = _qtli
            });
            var _bstli = _itli(typeof(BoltSash));
            _ = miAddAmmoContainer.Items.Add(new MenuItem
            {
                Header = GetHeader(_bstli),
                ToolTip = @"Bolt Sash of 10 bolts",
                Command = NewItem,
                CommandParameter = _bstli
            });
            var _sbtli = _itli(typeof(SlingBag));
            _ = miAddAmmoContainer.Items.Add(new MenuItem
            {
                Header = GetHeader(_sbtli),
                ToolTip = @"Bag of 10 sling bullets",
                Command = NewItem,
                CommandParameter = _sbtli
            });
            var _sptli = _itli(typeof(ShurikenPouch));
            _ = miAddAmmoContainer.Items.Add(new MenuItem
            {
                Header = GetHeader(_sptli),
                ToolTip = @"Bag of 20 shuriken",
                Command = NewItem,
                CommandParameter = _sptli
            });
            #endregion

            #region armor/shields
            // armor and shields
            miAddArmor.Items.Clear();
            miAddShield.Items.Clear();
            foreach (var _armor in _camp.ArmorTypes)
            {
                _ = miAddArmor.Items.Add(new MenuItem
                {
                    Header = GetHeader(_armor.Value),
                    Command = NewItem,
                    CommandParameter = _armor.Value
                });
            }
            foreach (var _shield in _camp.ShieldTypes)
            {
                _ = miAddShield.Items.Add(new MenuItem
                {
                    Header = GetHeader(_shield.Value),
                    Command = NewItem,
                    CommandParameter = _shield.Value
                });
            }
            #endregion

            // re-usable item casters
            static ICasterClass _wizard(int level) => Wizard.CreateItemCaster(level);
            static ICasterClass _cleric(int level) => Cleric.CreateItemCaster(level);

            #region potions
            // POTIONS
            ISpellMode _filter(IEnumerable<ISpellMode> modes) => modes.First();
            miAddPotion1.Items.Clear();
            _ = miAddPotion1.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(1), 1, 1, false, new CureLightWounds()), @"", _filter));
            _ = miAddPotion1.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(1), 1, 1, false, new UnknownToUndead()), @"", _filter));
            _ = miAddPotion1.Items.Add(PotionMenuItem(() => new SpellSource(_wizard(1), 1, 1, false, new Jump()), @"", _filter));
            _ = miAddPotion1.Items.Add(PotionMenuItem(() => new SpellSource(_wizard(1), 1, 1, false, new MageArmor()), @"", _filter));
            _ = miAddPotion1.Items.Add(PotionMenuItem(() => new SpellSource(_wizard(1), 1, 1, false, new MagicWeapon()), @"", _filter));
            _ = miAddPotion1.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(1), 1, 1, false, new RemoveFear()), @"", _filter));
            _ = miAddPotion1.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(1), 1, 1, false, new TouchOfSanctuary()), @"", _filter));
            _ = miAddPotion1.Items.Add(new Separator());
            var _miShieldOfGrace = new MenuItem { Header = @"Shield of Grace" };
            _ = miAddPotion2.Items.Add(_miShieldOfGrace);
            _ = _miShieldOfGrace.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(1), 1, 1, false, new ShieldOfGrace()), @"", _filter));
            _ = _miShieldOfGrace.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(6), 1, 1, false, new ShieldOfGrace()), @"", _filter));
            _ = _miShieldOfGrace.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(12), 1, 1, false, new ShieldOfGrace()), @"", _filter));
            _ = _miShieldOfGrace.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(18), 1, 1, false, new ShieldOfGrace()), @"", _filter));
            var _miProtectAlign = new MenuItem { Header = @"Protection Against Alignment" };
            _ = miAddPotion2.Items.Add(_miProtectAlign);
            _ = _miProtectAlign.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(1), 1, 1, false, new ProtectionAgainstGood()), @"", _filter));
            _ = _miProtectAlign.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(1), 1, 1, false, new ProtectionAgainstEvil()), @"", _filter));
            _ = _miProtectAlign.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(1), 1, 1, false, new ProtectionAgainstLaw()), @"", _filter));
            _ = _miProtectAlign.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(1), 1, 1, false, new ProtectionAgainstChaos()), @"", _filter));
            _ = miAddPotion1.Items.Add(PotionMenuItem(() => new SpellSource(_wizard(1), 1, 1, false, new EnlargePerson()), @"", _filter));
            _ = miAddPotion1.Items.Add(PotionMenuItem(() => new SpellSource(_wizard(1), 1, 1, false, new ReducePerson()), @"", _filter));

            miAddPotion2.Items.Clear();
            _ = miAddPotion2.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(3), 2, 2, false, new AidSpell()), @"", _filter));
            _ = miAddPotion2.Items.Add(PotionMenuItem(() => new SpellSource(_wizard(3), 2, 2, false, new Blur()), @"", _filter));
            _ = miAddPotion2.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(3), 2, 2, false, new CureModerateWounds()), @"", _filter));
            _ = miAddPotion2.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(3), 2, 2, false, new Darkness()), @"", _filter));
            _ = miAddPotion2.Items.Add(PotionMenuItem(() => new SpellSource(_wizard(3), 2, 2, false, new DarkvisionSpell()), @"", _filter));
            _ = miAddPotion2.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(3), 2, 2, false, new DelayPoison()), @"", _filter));
            _ = miAddPotion2.Items.Add(PotionMenuItem(() => new SpellSource(_wizard(3), 2, 2, false, new InvisibilitySpell()), @"", _filter));
            _ = miAddPotion2.Items.Add(PotionMenuItem(() => new SpellSource(_wizard(3), 2, 2, false, new ProtectionFromArrows()), @"", _filter));
            _ = miAddPotion2.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(3), 2, 2, false, new RemoveParalysis()), @"", _filter));
            _ = miAddPotion2.Items.Add(PotionMenuItem(() => new SpellSource(_wizard(3), 2, 2, false, new SpiderClimb()), @"", _filter));
            _ = miAddPotion2.Items.Add(new Separator());
            var _miBarkskin = new MenuItem { Header = @"Barkskin" };
            _ = miAddPotion2.Items.Add(_miBarkskin);
            _ = _miBarkskin.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(3), 2, 2, false, new Barkskin()), @"+2", _filter));
            _ = _miBarkskin.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(6), 2, 2, false, new Barkskin()), @"+3", _filter));
            _ = _miBarkskin.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(9), 2, 2, false, new Barkskin()), @"+4", _filter));
            _ = _miBarkskin.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(12), 2, 2, false, new Barkskin()), @"+5", _filter));
            var _miAbilities = new MenuItem { Header = @"Abilities" };
            _ = miAddPotion2.Items.Add(_miAbilities);
            _ = _miAbilities.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(3), 2, 2, false, new BearEndurance()), @"", _filter));
            _ = _miAbilities.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(3), 2, 2, false, new BullStrength()), @"", _filter));
            _ = _miAbilities.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(3), 2, 2, false, new CatGrace()), @"", _filter));
            _ = _miAbilities.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(3), 2, 2, false, new EagleSplendor()), @"", _filter));
            _ = _miAbilities.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(3), 2, 2, false, new FoxCunning()), @"", _filter));
            _ = _miAbilities.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(3), 2, 2, false, new OwlWisdom()), @"", _filter));
            var _miResistEnergy = new MenuItem { Header = @"Resist Energy" };
            _ = miAddPotion2.Items.Add(_miResistEnergy);
            _ = _miResistEnergy.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(3), 2, 2, false, new ResistEnergy()), @"Acid 10", _filter, (@"Energy", EnergyType.Acid.ToString())));
            _ = _miResistEnergy.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(3), 2, 2, false, new ResistEnergy()), @"Cold 10", _filter, (@"Energy", EnergyType.Cold.ToString())));
            _ = _miResistEnergy.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(3), 2, 2, false, new ResistEnergy()), @"Electric 10", _filter, (@"Energy", EnergyType.Electric.ToString())));
            _ = _miResistEnergy.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(3), 2, 2, false, new ResistEnergy()), @"Fire 10", _filter, (@"Energy", EnergyType.Fire.ToString())));
            _ = _miResistEnergy.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(3), 2, 2, false, new ResistEnergy()), @"Sonic 10", _filter, (@"Energy", EnergyType.Sonic.ToString())));
            _ = _miResistEnergy.Items.Add(new Separator());
            _ = _miResistEnergy.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(7), 2, 2, false, new ResistEnergy()), @"Acid 20", _filter, (@"Energy", EnergyType.Acid.ToString())));
            _ = _miResistEnergy.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(7), 2, 2, false, new ResistEnergy()), @"Cold 20", _filter, (@"Energy", EnergyType.Cold.ToString())));
            _ = _miResistEnergy.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(7), 2, 2, false, new ResistEnergy()), @"Electric 20", _filter, (@"Energy", EnergyType.Electric.ToString())));
            _ = _miResistEnergy.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(7), 2, 2, false, new ResistEnergy()), @"Fire 20", _filter, (@"Energy", EnergyType.Fire.ToString())));
            _ = _miResistEnergy.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(7), 2, 2, false, new ResistEnergy()), @"Sonic 20", _filter, (@"Energy", EnergyType.Sonic.ToString())));
            _ = _miResistEnergy.Items.Add(new Separator());
            _ = _miResistEnergy.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(11), 2, 2, false, new ResistEnergy()), @"Acid 30", _filter, (@"Energy", EnergyType.Acid.ToString())));
            _ = _miResistEnergy.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(11), 2, 2, false, new ResistEnergy()), @"Cold 30", _filter, (@"Energy", EnergyType.Cold.ToString())));
            _ = _miResistEnergy.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(11), 2, 2, false, new ResistEnergy()), @"Electric 30", _filter, (@"Energy", EnergyType.Electric.ToString())));
            _ = _miResistEnergy.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(11), 2, 2, false, new ResistEnergy()), @"Fire 30", _filter, (@"Energy", EnergyType.Fire.ToString())));
            _ = _miResistEnergy.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(11), 2, 2, false, new ResistEnergy()), @"Sonic 30", _filter, (@"Energy", EnergyType.Sonic.ToString())));
            _ = miAddPotion2.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(3), 2, 2, false, new RestorationLesser()), @"Dispel Bias", _filter, (@"Ability", @"*"), (@"Mode", @"Dispel")));
            _ = miAddPotion2.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(3), 2, 2, false, new RestorationLesser()), @"Cure Bias", _filter, (@"Ability", @"*"), (@"Mode", @"Cure")));

            miAddPotion3.Items.Clear();
            _ = miAddPotion3.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(5), 3, 3, false, new CureSeriousWounds()), @"", _filter));
            _ = miAddPotion3.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(5), 3, 3, false, new Daylight()), @"", _filter));
            _ = miAddPotion3.Items.Add(PotionMenuItem(() => new SpellSource(_wizard(5), 3, 3, false, new FlameArrow()), @"", _filter));
            _ = miAddPotion3.Items.Add(PotionMenuItem(() => new SpellSource(_wizard(5), 3, 3, false, new Fly()), @"", _filter));
            _ = miAddPotion3.Items.Add(PotionMenuItem(() => new SpellSource(_wizard(5), 3, 3, false, new Haste()), @"", _filter));
            _ = miAddPotion3.Items.Add(PotionMenuItem(() => new SpellSource(_wizard(5), 3, 3, false, new Heroism()), @"", _filter));
            _ = miAddPotion3.Items.Add(PotionMenuItem(() => new SpellSource(_wizard(5), 3, 3, false, new KeenEdge()), @"", _filter));
            _ = miAddPotion3.Items.Add(PotionMenuItem(() => new SpellSource(_wizard(5), 3, 3, false, new Rage()), @"", _filter));
            _ = miAddPotion3.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(5), 3, 3, false, new RemoveCurse()), @"", _filter));
            _ = miAddPotion3.Items.Add(new Separator());
            _ = miAddPotion3.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(5), 3, 3, false, new ProtectionFromEnergy()), @"Acid 60", _filter, (@"Energy", EnergyType.Acid.ToString())));
            _ = miAddPotion3.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(5), 3, 3, false, new ProtectionFromEnergy()), @"Cold 60", _filter, (@"Energy", EnergyType.Cold.ToString())));
            _ = miAddPotion3.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(5), 3, 3, false, new ProtectionFromEnergy()), @"Electric 60", _filter, (@"Energy", EnergyType.Electric.ToString())));
            _ = miAddPotion3.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(5), 3, 3, false, new ProtectionFromEnergy()), @"Fire 60", _filter, (@"Energy", EnergyType.Fire.ToString())));
            _ = miAddPotion3.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(5), 3, 3, false, new ProtectionFromEnergy()), @"Sonic 60", _filter, (@"Energy", EnergyType.Sonic.ToString())));
            _ = miAddPotion3.Items.Add(new Separator());
            var _miMagicWeaponGreater = new MenuItem { Header = @"Greater Magic Weapon" };
            _ = miAddPotion3.Items.Add(_miMagicWeaponGreater);
            _ = _miMagicWeaponGreater.Items.Add(PotionMenuItem(() => new SpellSource(_wizard(5), 3, 3, false, new MagicWeaponGreater()), @"+1", _filter));
            _ = _miMagicWeaponGreater.Items.Add(PotionMenuItem(() => new SpellSource(_wizard(8), 3, 3, false, new MagicWeaponGreater()), @"+2", _filter));
            _ = _miMagicWeaponGreater.Items.Add(PotionMenuItem(() => new SpellSource(_wizard(12), 3, 3, false, new MagicWeaponGreater()), @"+3", _filter));
            _ = _miMagicWeaponGreater.Items.Add(PotionMenuItem(() => new SpellSource(_wizard(16), 3, 3, false, new MagicWeaponGreater()), @"+4", _filter));
            _ = _miMagicWeaponGreater.Items.Add(PotionMenuItem(() => new SpellSource(_wizard(20), 3, 3, false, new MagicWeaponGreater()), @"+5", _filter));
            var _miMagicVestment = new MenuItem { Header = @"Magic Vestment" };
            _ = miAddPotion3.Items.Add(_miMagicVestment);
            _ = _miMagicVestment.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(5), 3, 3, false, new MagicVestment()), @"+1", _filter));
            _ = _miMagicVestment.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(8), 3, 3, false, new MagicVestment()), @"+2", _filter));
            _ = _miMagicVestment.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(12), 3, 3, false, new MagicVestment()), @"+3", _filter));
            _ = _miMagicVestment.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(16), 3, 3, false, new MagicVestment()), @"+4", _filter));
            _ = _miMagicVestment.Items.Add(PotionMenuItem(() => new SpellSource(_cleric(20), 3, 3, false, new MagicVestment()), @"+5", _filter));
            #endregion

            #region wands
            // WANDS
            miAddWand0.Items.Clear();
            _ = miAddWand0.Items.Add(WandMenuItem(() => new SpellSource(_wizard(1), 0, 0, false, new DetectMagic())));
            _ = miAddWand0.Items.Add(WandMenuItem(() => new SpellSource(_wizard(1), 0, 0, false, new Light())));

            miAddWand1.Items.Clear();
            _ = miAddWand1.Items.Add(WandMenuItem(() => new SpellSource(_wizard(1), 1, 1, false, new BurningHands())));
            _ = miAddWand1.Items.Add(WandMenuItem(() => new SpellSource(_wizard(1), 1, 1, false, new CharmPerson())));
            _ = miAddWand1.Items.Add(WandMenuItem(() => new SpellSource(_wizard(1), 1, 1, false, new ColorSpray())));
            _ = miAddWand1.Items.Add(WandMenuItem(() => new SpellSource(_cleric(1), 1, 1, false, new CureLightWounds())));
            _ = miAddWand1.Items.Add(WandMenuItem(() => new SpellSource(_wizard(1), 1, 1, false, new DetectSecretDoors())));
            _ = miAddWand1.Items.Add(WandMenuItem(() => new SpellSource(_wizard(1), 1, 1, false, new EnlargePerson())));
            _ = miAddWand1.Items.Add(WandMenuItem(() => new SpellSource(_wizard(1), 1, 1, false, new ShockingTouch())));
            _ = miAddWand1.Items.Add(new Separator());
            _ = miAddWand1.Items.Add(WandMenuItem(() => new SpellSource(_wizard(1), 1, 1, false, new MagicForceMissile())));
            _ = miAddWand1.Items.Add(WandMenuItem(() => new SpellSource(_wizard(3), 1, 1, false, new MagicForceMissile())));
            _ = miAddWand1.Items.Add(WandMenuItem(() => new SpellSource(_wizard(5), 1, 1, false, new MagicForceMissile())));
            _ = miAddWand1.Items.Add(WandMenuItem(() => new SpellSource(_wizard(7), 1, 1, false, new MagicForceMissile())));
            _ = miAddWand1.Items.Add(WandMenuItem(() => new SpellSource(_wizard(9), 1, 1, false, new MagicForceMissile())));

            miAddWand2.Items.Clear();
            _ = miAddWand2.Items.Add(WandMenuItem(() => new SpellSource(_cleric(3), 2, 2, false, new CureModerateWounds())));
            _ = miAddWand2.Items.Add(WandMenuItem(() => new SpellSource(_cleric(3), 2, 2, false, new Darkness())));
            _ = miAddWand2.Items.Add(WandMenuItem(() => new SpellSource(_cleric(3), 2, 2, false, new DelayPoison())));
            _ = miAddWand2.Items.Add(WandMenuItem(() => new SpellSource(_cleric(3), 2, 2, false, new Silence())));
            _ = miAddWand2.Items.Add(WandMenuItem(() => new SpellSource(_wizard(3), 2, 2, false, new AcidArrow())));
            _ = miAddWand2.Items.Add(WandMenuItem(() => new SpellSource(_wizard(3), 2, 2, false, new FalseLife())));
            _ = miAddWand2.Items.Add(WandMenuItem(() => new SpellSource(_wizard(3), 2, 2, false, new HoldPerson())));
            _ = miAddWand2.Items.Add(WandMenuItem(() => new SpellSource(_wizard(3), 2, 2, false, new InvisibilitySpell())));
            _ = miAddWand2.Items.Add(WandMenuItem(() => new SpellSource(_wizard(3), 2, 2, false, new Knock())));
            _ = miAddWand2.Items.Add(new Separator());
            _ = miAddWand2.Items.Add(WandMenuItem(() => new SpellSource(_cleric(3), 2, 2, false, new BearEndurance())));
            _ = miAddWand2.Items.Add(WandMenuItem(() => new SpellSource(_cleric(3), 2, 2, false, new BullStrength())));
            _ = miAddWand2.Items.Add(WandMenuItem(() => new SpellSource(_cleric(3), 2, 2, false, new CatGrace())));
            _ = miAddWand2.Items.Add(WandMenuItem(() => new SpellSource(_cleric(3), 2, 2, false, new EagleSplendor())));
            _ = miAddWand2.Items.Add(WandMenuItem(() => new SpellSource(_cleric(3), 2, 2, false, new FoxCunning())));
            _ = miAddWand2.Items.Add(WandMenuItem(() => new SpellSource(_cleric(3), 2, 2, false, new OwlWisdom())));

            miAddWand3.Items.Clear();
            _ = miAddWand3.Items.Add(WandMenuItem(() => new SpellSource(_cleric(5), 3, 3, false, new CureSeriousWounds())));
            _ = miAddWand3.Items.Add(WandMenuItem(() => new SpellSource(_cleric(5), 3, 3, false, new Daylight())));  // 3rd level spell
            _ = miAddWand3.Items.Add(WandMenuItem(() => new SpellSource(_wizard(5), 3, 3, false, new KeenEdge())));  // 3rd level spell
            _ = miAddWand3.Items.Add(WandMenuItem(() => new SpellSource(_cleric(6), 3, 3, false, new SearingLight())));
            _ = miAddWand3.Items.Add(WandMenuItem(() => new SpellSource(_wizard(5), 3, 3, false, new Slow())));
            _ = miAddWand3.Items.Add(new Separator());
            _ = miAddWand3.Items.Add(WandMenuItem(() => new SpellSource(_wizard(5), 3, 3, false, new LightningBolt())));
            _ = miAddWand3.Items.Add(WandMenuItem(() => new SpellSource(_wizard(6), 3, 3, false, new LightningBolt())));
            _ = miAddWand3.Items.Add(WandMenuItem(() => new SpellSource(_wizard(7), 3, 3, false, new LightningBolt())));
            _ = miAddWand3.Items.Add(WandMenuItem(() => new SpellSource(_wizard(6), 3, 3, false, new LightningBolt())));
            _ = miAddWand3.Items.Add(WandMenuItem(() => new SpellSource(_wizard(9), 3, 3, false, new LightningBolt())));
            _ = miAddWand3.Items.Add(WandMenuItem(() => new SpellSource(_wizard(10), 3, 3, false, new LightningBolt())));

            miAddWand4.Items.Clear();
            _ = miAddWand4.Items.Add(WandMenuItem(() => new SpellSource(_cleric(7), 4, 4, false, new CureCriticalWounds())));
            _ = miAddWand4.Items.Add(WandMenuItem(() => new SpellSource(_cleric(7), 4, 4, false, new InflictCriticalWounds())));
            #endregion

            #region devotional items
            // devotional items
            miAddDevotionalSymbolWooden.Items.Clear();
            miAddDevotionalSymbolSilver.Items.Clear();
            foreach (var _dev in Campaign.SystemCampaign.Devotions)
            {
                _ = miAddDevotionalSymbolWooden.Items.Add(DevotionalSymbolMenuItem(new Tuple<string, DevotionalDefinition, Material>(_dev.Key, _dev.Value, WoodMaterial.Static)));
                _ = miAddDevotionalSymbolSilver.Items.Add(DevotionalSymbolMenuItem(new Tuple<string, DevotionalDefinition, Material>(_dev.Key, _dev.Value, Silver.Static)));
            }
            #endregion

            miAddComponentPouch.CommandParameter = _itli(typeof(ComponentPouch));

            #region Rings
            miAddDeflectorRing.Items.Clear();
            _ = miAddDeflectorRing.Items.Add(AugmentRingMenuItem(() => DeflectionSlotActivation.CreateDeflectionAugment(_cleric(3), 1, true)));
            _ = miAddDeflectorRing.Items.Add(AugmentRingMenuItem(() => DeflectionSlotActivation.CreateDeflectionAugment(_cleric(6), 2, true)));
            _ = miAddDeflectorRing.Items.Add(AugmentRingMenuItem(() => DeflectionSlotActivation.CreateDeflectionAugment(_cleric(9), 3, true)));
            _ = miAddDeflectorRing.Items.Add(AugmentRingMenuItem(() => DeflectionSlotActivation.CreateDeflectionAugment(_cleric(12), 4, true)));
            _ = miAddDeflectorRing.Items.Add(AugmentRingMenuItem(() => DeflectionSlotActivation.CreateDeflectionAugment(_cleric(15), 5, true)));

            // climbing
            _ = miAddSkillBonusRing.Items.Add(AugmentRingMenuItem(() => SkillBonusSlotActivation<ClimbSkill>.CreateSkillBonusAugment(_cleric(5), 5, true)));
            _ = miAddSkillBonusRing.Items.Add(AugmentRingMenuItem(() => SkillBonusSlotActivation<ClimbSkill>.CreateSkillBonusAugment(_cleric(10), 10, true)));

            // swimming
            _ = miAddSkillBonusRing.Items.Add(AugmentRingMenuItem(() => SkillBonusSlotActivation<SwimSkill>.CreateSkillBonusAugment(_cleric(5), 5, true)));
            _ = miAddSkillBonusRing.Items.Add(AugmentRingMenuItem(() => SkillBonusSlotActivation<SwimSkill>.CreateSkillBonusAugment(_cleric(10), 10, true)));

            // jumping
            _ = miAddSkillBonusRing.Items.Add(AugmentRingMenuItem(() => SkillBonusSlotActivation<JumpSkill>.CreateSkillBonusAugment(_cleric(5), 5, true)));
            _ = miAddSkillBonusRing.Items.Add(AugmentRingMenuItem(() => SkillBonusSlotActivation<JumpSkill>.CreateSkillBonusAugment(_cleric(10), 10, true)));

            // low resistors
            _ = miAddEnergyResistorRing.Items.Add(AugmentRingMenuItem(() => EnergyResistorSlotActivation.CreateEnergyResistorAugment(_cleric(3), EnergyType.Fire, 10, true)));
            _ = miAddEnergyResistorRing.Items.Add(AugmentRingMenuItem(() => EnergyResistorSlotActivation.CreateEnergyResistorAugment(_cleric(3), EnergyType.Electric, 10, true)));
            _ = miAddEnergyResistorRing.Items.Add(AugmentRingMenuItem(() => EnergyResistorSlotActivation.CreateEnergyResistorAugment(_cleric(3), EnergyType.Sonic, 10, true)));
            _ = miAddEnergyResistorRing.Items.Add(AugmentRingMenuItem(() => EnergyResistorSlotActivation.CreateEnergyResistorAugment(_cleric(3), EnergyType.Cold, 10, true)));
            _ = miAddEnergyResistorRing.Items.Add(AugmentRingMenuItem(() => EnergyResistorSlotActivation.CreateEnergyResistorAugment(_cleric(3), EnergyType.Acid, 10, true)));

            // medium resistors
            _ = miAddEnergyResistorRing.Items.Add(AugmentRingMenuItem(() => EnergyResistorSlotActivation.CreateEnergyResistorAugment(_cleric(7), EnergyType.Fire, 20, true)));
            _ = miAddEnergyResistorRing.Items.Add(AugmentRingMenuItem(() => EnergyResistorSlotActivation.CreateEnergyResistorAugment(_cleric(7), EnergyType.Electric, 20, true)));
            _ = miAddEnergyResistorRing.Items.Add(AugmentRingMenuItem(() => EnergyResistorSlotActivation.CreateEnergyResistorAugment(_cleric(7), EnergyType.Sonic, 20, true)));
            _ = miAddEnergyResistorRing.Items.Add(AugmentRingMenuItem(() => EnergyResistorSlotActivation.CreateEnergyResistorAugment(_cleric(7), EnergyType.Cold, 20, true)));
            _ = miAddEnergyResistorRing.Items.Add(AugmentRingMenuItem(() => EnergyResistorSlotActivation.CreateEnergyResistorAugment(_cleric(7), EnergyType.Acid, 20, true)));

            // high resistors
            _ = miAddEnergyResistorRing.Items.Add(AugmentRingMenuItem(() => EnergyResistorSlotActivation.CreateEnergyResistorAugment(_cleric(11), EnergyType.Fire, 30, true)));
            _ = miAddEnergyResistorRing.Items.Add(AugmentRingMenuItem(() => EnergyResistorSlotActivation.CreateEnergyResistorAugment(_cleric(11), EnergyType.Electric, 30, true)));
            _ = miAddEnergyResistorRing.Items.Add(AugmentRingMenuItem(() => EnergyResistorSlotActivation.CreateEnergyResistorAugment(_cleric(11), EnergyType.Sonic, 30, true)));
            _ = miAddEnergyResistorRing.Items.Add(AugmentRingMenuItem(() => EnergyResistorSlotActivation.CreateEnergyResistorAugment(_cleric(11), EnergyType.Cold, 30, true)));
            _ = miAddEnergyResistorRing.Items.Add(AugmentRingMenuItem(() => EnergyResistorSlotActivation.CreateEnergyResistorAugment(_cleric(11), EnergyType.Acid, 30, true)));

            // invisibility
            _ = miAddRing.Items.Add(AugmentRingMenuItem(
                () =>
                {
                    var _source = new SpellSource(_wizard(3), 2, 2, false, new InvisibilitySpell());
                    return new MagicAugment(_source,
                        new SlottedItemSpellActivation(
                            new SpellCommandWord(_source, new InvisibilitySpellPersonalMode(),
                            new PowerCapacity(_source), 1, new CustomSpellActivationCost(20000), true, null), true, true)
                        );
                }));

            // etherealness (test item)
            _ = miAddRing.Items.Add(AugmentRingMenuItem(
                () =>
                {
                    var _source = new SpellSource(_wizard(13), 7, 7, false, new EtherealJaunt());
                    return new MagicAugment(_source,
                        new SlottedItemSpellActivation(
                            new SpellCommandWord(_source, new EtherealJaunt(),
                            new PowerCapacity(_source), 1, new CustomSpellActivationCost(55000), true, null), true, true)
                        );
                }));

            // feather fall
            _ = miAddRing.Items.Add(CreateRingMenuItem(@"Ring of Feather Falling", @"Fall more slowly to avoid damage",
                () =>
                {
                    var _ring = new Ring(@"Ring of Feather Falling", GoldPlatingMaterial.Static, 2);
                    _ = _ring.AddAdjunct(AdjunctSlotActivation.CreateAdjunctAugment(_wizard(1), new FeatherFall(),
                        1, 1, new SlowFallEffect(typeof(FeatherFall)), new Info { Message = @"Fall slowly" },
                         2200m, true, false));
                    return _ring;
                }));
            #endregion

            // slotted items

            #region Belt
            miAddBelt.Items.Clear();
            SlottedItemBase _beltMaker(string name) => new Belt($@"Belt of {name}", 5);
            _ = miAddBelt.Items.Add(SlottedItemMenuItem(
                () => AbilityEnhanceSlotActivation.CreateAbilityEnhanceAugment(_cleric(6), MnemonicCode.Str, () => new BullStrength(), 2, true),
                _beltMaker));
            _ = miAddBelt.Items.Add(SlottedItemMenuItem(
                () => AbilityEnhanceSlotActivation.CreateAbilityEnhanceAugment(_cleric(8), MnemonicCode.Str, () => new BullStrength(), 4, true),
                _beltMaker));
            _ = miAddBelt.Items.Add(SlottedItemMenuItem(
                () => AbilityEnhanceSlotActivation.CreateAbilityEnhanceAugment(_cleric(10), MnemonicCode.Str, () => new BullStrength(), 6, true),
                _beltMaker));
            #endregion

            #region head
            miAddHead.Items.Clear();
            SlottedItemBase _headbandMaker(string name) => new Headband($@"Headband of {name}", LeatherMaterial.Static, 2);
            _ = miAddHead.Items.Add(SlottedItemMenuItem(
                () => AbilityEnhanceSlotActivation.CreateAbilityEnhanceAugment(_cleric(6), MnemonicCode.Int, () => new FoxCunning(), 2, true),
                _headbandMaker));
            _ = miAddHead.Items.Add(SlottedItemMenuItem(
                () => AbilityEnhanceSlotActivation.CreateAbilityEnhanceAugment(_cleric(8), MnemonicCode.Int, () => new FoxCunning(), 4, true),
                _headbandMaker));
            _ = miAddHead.Items.Add(SlottedItemMenuItem(
                () => AbilityEnhanceSlotActivation.CreateAbilityEnhanceAugment(_cleric(10), MnemonicCode.Int, () => new FoxCunning(), 6, true),
                _headbandMaker));
            #endregion

            #region neck
            miAddNeck.Items.Clear();
            SlottedItemBase _amuletMaker(string name) => new Amulet($@"Amulet of {name}", IronMaterial.Static, 5);
            // TODO: periapt (just like an amulet, different icon)
            _ = miAddNeck.Items.Add(SlottedItemMenuItem(
                () => NaturalArmorSlotActivation.CreateNaturalArmorAugment(_cleric(3), () => new Barkskin(), 1, true),
                _amuletMaker));
            _ = miAddNeck.Items.Add(SlottedItemMenuItem(
                () => NaturalArmorSlotActivation.CreateNaturalArmorAugment(_cleric(6), () => new Barkskin(), 2, true),
                _amuletMaker));
            _ = miAddNeck.Items.Add(SlottedItemMenuItem(
                () => NaturalArmorSlotActivation.CreateNaturalArmorAugment(_cleric(9), () => new Barkskin(), 3, true),
                _amuletMaker));
            _ = miAddNeck.Items.Add(SlottedItemMenuItem(
                () => NaturalArmorSlotActivation.CreateNaturalArmorAugment(_cleric(12), () => new Barkskin(), 4, true),
                _amuletMaker));
            _ = miAddNeck.Items.Add(SlottedItemMenuItem(
                () => NaturalArmorSlotActivation.CreateNaturalArmorAugment(_cleric(15), () => new Barkskin(), 5, true),
                _amuletMaker));
            _ = miAddNeck.Items.Add(SlottedItemMenuItem(
                () => AbilityEnhanceSlotActivation.CreateAbilityEnhanceAugment(_cleric(6), MnemonicCode.Con, () => new BearEndurance(), 2, true),
                _amuletMaker));
            _ = miAddNeck.Items.Add(SlottedItemMenuItem(
                () => AbilityEnhanceSlotActivation.CreateAbilityEnhanceAugment(_cleric(8), MnemonicCode.Con, () => new BearEndurance(), 4, true),
                _amuletMaker));
            _ = miAddNeck.Items.Add(SlottedItemMenuItem(
                () => AbilityEnhanceSlotActivation.CreateAbilityEnhanceAugment(_cleric(10), MnemonicCode.Con, () => new BearEndurance(), 6, true),
                _amuletMaker));
            #endregion

            #region shoulders
            miAddShoulders.Items.Clear();
            SlottedItemBase _cloakMaker(string name) => new Cloak($@"Cloak of {name}", LeatherMaterial.Static, 10);
            _ = miAddShoulders.Items.Add(SlottedItemMenuItem(
                () => ResistanceSlotActivation.CreateResistanceAugment(_cleric(3), 1, true),
                _cloakMaker));
            _ = miAddShoulders.Items.Add(SlottedItemMenuItem(
                () => ResistanceSlotActivation.CreateResistanceAugment(_cleric(6), 2, true),
                _cloakMaker));
            _ = miAddShoulders.Items.Add(SlottedItemMenuItem(
                () => ResistanceSlotActivation.CreateResistanceAugment(_cleric(9), 3, true),
                _cloakMaker));
            _ = miAddShoulders.Items.Add(SlottedItemMenuItem(
                () => ResistanceSlotActivation.CreateResistanceAugment(_cleric(12), 4, true),
                _cloakMaker));
            _ = miAddShoulders.Items.Add(SlottedItemMenuItem(
                () => ResistanceSlotActivation.CreateResistanceAugment(_cleric(15), 5, true),
                _cloakMaker));
            _ = miAddShoulders.Items.Add(SlottedItemMenuItem(
                () => AbilityEnhanceSlotActivation.CreateAbilityEnhanceAugment(_cleric(6), MnemonicCode.Cha, () => new EagleSplendor(), 2, true),
                _cloakMaker));
            _ = miAddShoulders.Items.Add(SlottedItemMenuItem(
                () => AbilityEnhanceSlotActivation.CreateAbilityEnhanceAugment(_cleric(8), MnemonicCode.Cha, () => new EagleSplendor(), 4, true),
                _cloakMaker));
            _ = miAddShoulders.Items.Add(SlottedItemMenuItem(
                () => AbilityEnhanceSlotActivation.CreateAbilityEnhanceAugment(_cleric(10), MnemonicCode.Cha, () => new EagleSplendor(), 6, true),
                _cloakMaker));
            #endregion

            #region arms
            miAddArms.Items.Clear();

            static SlottedItemBase _bracerMaker(string name) => new Bracers($@"Bracers of {name}", MetalMaterial.CommonStatic, 5);

            _ = miAddArms.Items.Add(SlottedItemMenuItem(
                () => ArmorSlotActivation.CreateArmorAugment(_wizard(2), () => new MageArmor(), 1, true, true),
                _bracerMaker));
            _ = miAddArms.Items.Add(SlottedItemMenuItem(
                () => ArmorSlotActivation.CreateArmorAugment(_wizard(4), () => new MageArmor(), 2, true, true),
                _bracerMaker));
            _ = miAddArms.Items.Add(SlottedItemMenuItem(
                () => ArmorSlotActivation.CreateArmorAugment(_wizard(6), () => new MageArmor(), 3, true, true),
                _bracerMaker));
            _ = miAddArms.Items.Add(SlottedItemMenuItem(
                () => ArmorSlotActivation.CreateArmorAugment(_wizard(8), () => new MageArmor(), 4, true, true),
                _bracerMaker));
            _ = miAddArms.Items.Add(SlottedItemMenuItem(
                () => ArmorSlotActivation.CreateArmorAugment(_wizard(10), () => new MageArmor(), 5, true, true),
                _bracerMaker));
            _ = miAddArms.Items.Add(SlottedItemMenuItem(
                () => ArmorSlotActivation.CreateArmorAugment(_wizard(12), () => new MageArmor(), 6, true, true),
                _bracerMaker));
            _ = miAddArms.Items.Add(SlottedItemMenuItem(
                () => ArmorSlotActivation.CreateArmorAugment(_wizard(14), () => new MageArmor(), 7, true, true),
                _bracerMaker));
            _ = miAddArms.Items.Add(SlottedItemMenuItem(
                () => ArmorSlotActivation.CreateArmorAugment(_wizard(16), () => new MageArmor(), 8, true, true),
                _bracerMaker));
            #endregion

            #region hands
            miAddHands.Items.Clear();
            SlottedItemBase _glovesMaker(string name) => new Gloves($@"Gloves of {name}", 2);
            _ = miAddHands.Items.Add(SlottedItemMenuItem(
                () => AbilityEnhanceSlotActivation.CreateAbilityEnhanceAugment(_cleric(6), MnemonicCode.Dex, () => new CatGrace(), 2, true),
                _glovesMaker));
            _ = miAddHands.Items.Add(SlottedItemMenuItem(
                () => AbilityEnhanceSlotActivation.CreateAbilityEnhanceAugment(_cleric(8), MnemonicCode.Dex, () => new CatGrace(), 4, true),
                _glovesMaker));
            _ = miAddHands.Items.Add(SlottedItemMenuItem(
                () => AbilityEnhanceSlotActivation.CreateAbilityEnhanceAugment(_cleric(10), MnemonicCode.Dex, () => new CatGrace(), 6, true),
                _glovesMaker));
            _ = miAddHands.Items.Add(SlottedItemMenuItem(
                () => SkillBonusSlotActivation<HealSkill>.CreateSkillBonusAugment(_cleric(5), 5, true),
                _glovesMaker));
            _ = miAddHands.Items.Add(SlottedItemMenuItem(
                () => SkillBonusSlotActivation<QuickFingersSkill>.CreateSkillBonusAugment(_cleric(5), 5, true),
                _glovesMaker));
            #endregion

            #region feet
            miAddFeet.Items.Clear();
            SlottedItemBase _bootsMaker(string name) => new Boots($@"Boots of {name}", 5);
            _ = miAddFeet.Items.Add(SlottedItemMenuItem(
                () => SkillBonusSlotActivation<SilentStealthSkill>.CreateSkillBonusAugment(_cleric(5), 5, true),
                _bootsMaker));
            #endregion

            // TODO: poison

            // wealth treasure

            #region gems
            // GEMS
            miAddGems.Items.Clear();
            _ = miAddGems.Items.Add(GemMenuItem(OrnamentalGemMaterial.Static));
            _ = miAddGems.Items.Add(new Separator());
            _ = miAddGems.Items.Add(GemMenuItem(SemiPreciousGemMaterial.Static));
            _ = miAddGems.Items.Add(GemMenuItem(OnyxMaterial.Static));
            _ = miAddGems.Items.Add(GemMenuItem(BlackOnyxMaterial.Static));
            _ = miAddGems.Items.Add(new Separator());
            _ = miAddGems.Items.Add(GemMenuItem(FancyGemMaterial.Static));
            _ = miAddGems.Items.Add(GemMenuItem(AmberMaterial.Static));
            _ = miAddGems.Items.Add(GemMenuItem(JadeMaterial.Static));
            _ = miAddGems.Items.Add(GemMenuItem(PearlMaterial.Static));
            _ = miAddGems.Items.Add(GemMenuItem(BlackPearlMaterial.Static));
            _ = miAddGems.Items.Add(new Separator());
            _ = miAddGems.Items.Add(GemMenuItem(PreciousGemMaterial.Static));
            _ = miAddGems.Items.Add(new Separator());
            _ = miAddGems.Items.Add(GemMenuItem(ExquisiteGemMaterial.Static));
            _ = miAddGems.Items.Add(GemMenuItem(RubyMaterial.Static));
            _ = miAddGems.Items.Add(GemMenuItem(OpalMaterial.Static));
            _ = miAddGems.Items.Add(new Separator());
            _ = miAddGems.Items.Add(GemMenuItem(ExaltedGemMaterial.Static));
            _ = miAddGems.Items.Add(GemMenuItem(DiamondMaterial.Static));
            _ = miAddGems.Items.Add(GemMenuItem(JacinthMaterial.Static));
            #endregion

            // CONTAINERS
            miNewBag.CommandParameter = _itli(typeof(Bag));
            miNewChest.CommandParameter = _itli(typeof(Chest));
            miNewBackpack.CommandParameter = _itli(typeof(BackPack));

            // light sources
            miNewGlowRod.CommandParameter = _itli(typeof(GlowRod));
            miNewMagicTorch.CommandParameter = _itli(typeof(MagicTorch));
        }

        public MenuItem CreateRingMenuItem(string header, string toolTip, Func<Ring> ringMaker)
            => new()
            {
                Header = header,
                ToolTip = toolTip,
                Command = NewRingItem,
                CommandParameter = ringMaker
            };

        public MenuItem AugmentRingMenuItem(Func<MagicAugment> factory)
        {
            var _source = factory();
            var _id = _source.Augmentation as IIdentification;
            return new MenuItem
            {
                Header = _id?.IdentificationInfos?.FirstOrDefault()?.Message
                    ?? $@"{_source.MagicPowerActionSource.PowerDef.DisplayName} ({_source.MagicPowerActionSource.CasterLevel})",
                ToolTip = _source.MagicPowerActionSource.PowerDef.Description,
                Command = NewRingItem,
                CommandParameter = factory
            };
        }

        public MenuItem SlottedItemMenuItem(Func<MagicAugment> augmentFactory, Func<string, SlottedItemBase> itemFactory)
        {
            var _augment = augmentFactory();
            var _id = _augment.Augmentation as IIdentification;
            var _augInfo = _id?.IdentificationInfos?.FirstOrDefault()?.Message
                ?? $@"{_augment.MagicPowerActionSource.PowerDef.DisplayName} ({_augment.MagicPowerActionSource.CasterLevel})";

            var _item = itemFactory(_augInfo);
            return new MenuItem
            {
                Header = GetHeader(_item.IconKeys.FirstOrDefault(), new IconReferenceInfo { IconScale = 1 }, _item.Name, string.Empty),
                ToolTip = _augment.MagicPowerActionSource.PowerDef.Description,
                Command = NewSlottedItem,
                CommandParameter = new Tuple<Func<string, SlottedItemBase>, Func<MagicAugment>>(itemFactory, augmentFactory)
            };
        }

        public MenuItem WandMenuItem(Func<SpellSource> factory)
        {
            var _source = factory();
            return new MenuItem
            {
                Header = $@"{_source.SpellDef.DisplayName} ({_source.CasterLevel})",
                ToolTip = _source.SpellDef.Description,
                Command = NewWandItem,
                CommandParameter = factory
            };
        }

        public MenuItem PotionMenuItem(Func<SpellSource> factory, string extra, Func<IEnumerable<ISpellMode>, ISpellMode> filter,
            params (string optKey, string optValue)[] options)
        {
            var _source = factory?.Invoke();
            var _mode = filter(_source?.SpellDef.SpellModes);
            return new MenuItem
            {
                Header = $@"{_source?.SpellDef.DisplayName} {extra}({_source?.CasterLevel ?? 1})",
                ToolTip = _mode.Description,
                Command = NewPotionItem,
                CommandParameter = (factory, extra, filter, options)
            };
        }

        public MenuItem DevotionalSymbolMenuItem(Tuple<string, DevotionalDefinition, Material> devotionalDef)
            => new()
            {
                Header = $@"{devotionalDef.Item1} ({string.Join(@",", devotionalDef.Item2.Influences.Select(_i => _i.Description).ToArray())})",
                ToolTip = devotionalDef.Item2.Alignment,
                Command = NewDevotionalItem,
                CommandParameter = devotionalDef
            };

        public MenuItem GemMenuItem(GemMaterial gemMaterial)
            => new()
            {
                Header = string.Format(@"{0} (Price:{1})", gemMaterial.Name, gemMaterial.ValueRandomizer),
                Command = NewGem,
                CommandParameter = gemMaterial
            };
    }
}
