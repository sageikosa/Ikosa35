using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Advancement.CharacterClasses;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Magic.Spells;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for MeleeWeaponEditor.xaml
    /// </summary>
    public partial class MeleeWeaponEditor : UserControl
    {
        public MeleeWeaponEditor()
        {
            InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(MeleeWeaponEditor_DataContextChanged);
        }

        protected ItemCaster DivineCaster(int level)
            => new(MagicType.Divine, level, Alignment.TrueNeutral, 10 + level + (level / 2), Guid.Empty, typeof(Cleric));

        protected ItemCaster ArcaneCaster(int level)
            => new(MagicType.Arcane, level, Alignment.TrueNeutral, 10 + level + (level / 2), Guid.Empty, typeof(Wizard));

        private MeleeWeaponVM MeleeWeapon => DataContext as MeleeWeaponVM;
        private IMeleeWeapon Weapon => MeleeWeapon?.Thing;

        void MeleeWeaponEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Weapon != null)
            {
                var _weapon = Weapon;
                cboSize.SelectedIndex = _weapon.ItemSizer.ExpectedCreatureSize.Order + 4;

                // Masterwork and Enhancement
                RefreshEnhance(_weapon);

                // MENU
                miAdd.Items.Clear();
                miAdd.Items.Add(new MenuItem
                {
                    Header = @"Defending",
                    Command = ArmorEditor.AdjunctAdd,
                    CommandParameter = new MagicAugment(new SpellSource(DivineCaster(8), 1, 1, false, new ShieldOfGrace()), new Defending())
                });
                miAdd.Items.Add(new MenuItem
                {
                    Header = @"Flaming",
                    Command = ArmorEditor.AdjunctAdd,
                    CommandParameter = new MagicAugment(new SpellSource(ArcaneCaster(10), 3, 3, false, new Fireball()), new Flaming())
                });
                miAdd.Items.Add(new MenuItem
                {
                    Header = @"Merciful",
                    Command = ArmorEditor.AdjunctAdd,
                    CommandParameter = new MagicAugment(new SpellSource(DivineCaster(5), 1, 1, false, new CureLightWounds()), new Merciful())
                });
                miAdd.Items.Add(new MenuItem
                {
                    Header = @"Shock",
                    Command = ArmorEditor.AdjunctAdd,
                    CommandParameter = new MagicAugment(new SpellSource(ArcaneCaster(8), 3, 3, false, new LightningBolt()), new Shock())
                });
                miAdd.Items.Add(new MenuItem
                {
                    Header = @"Speed",
                    Command = ArmorEditor.AdjunctAdd,
                    CommandParameter = new MagicAugment(new SpellSource(ArcaneCaster(7), 3, 3, false, new Haste()), new Speed())
                });
            }
        }

        #region private void RefreshEnhance(IMeleeWeapon weapon)
        private void RefreshEnhance(IMeleeWeapon weapon)
        {
            if (weapon.IsMasterwork())
            {
                if (weapon.MainHead.IsEnhanced())
                {
                    if (cboEnhancement.SelectedIndex != weapon.MainHead.ListedEnhancement + 1)
                        cboEnhancement.SelectedIndex = weapon.MainHead.ListedEnhancement + 1;
                }
                else
                {
                    if (cboEnhancement.SelectedIndex != 1)
                        cboEnhancement.SelectedIndex = 1;
                }
            }
            else
            {
                if (cboEnhancement.SelectedIndex != 0)
                    cboEnhancement.SelectedIndex = 0;
            }
            MeleeWeapon.DoAugmentationsChanged();
        }
        #endregion

        private void cboSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Weapon != null)
            {
                var _cbItem = cboSize.SelectedItem as ComboBoxItem;
                Weapon.ItemSizer.ExpectedCreatureSize = _cbItem.Tag as Size;
            }
        }

        #region private void cboEnhancement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        private void cboEnhancement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((Weapon != null) && !(Weapon is UnarmedWeapon))
            {
                var _level = cboEnhancement.SelectedIndex - 1;
                if (_level == -1)
                {
                    // remove
                    RemoveEnhancement();

                    // remove masterwork
                    if (Weapon.IsMasterwork())
                    {
                        var _mw = Weapon.Adjuncts.Where(_a => _a is Masterwork).ToList();
                        foreach (var _i in _mw)
                        {
                            Weapon.RemoveAdjunct(_i);
                        }
                    }
                    RefreshEnhance(Weapon);
                }
                else if (_level == 0)
                {
                    // remove
                    RemoveEnhancement();

                    // ensure masterwork
                    if (!Weapon.IsMasterwork())
                    {
                        Weapon.AddAdjunct(new Masterwork(typeof(Masterwork)));
                    }

                    RefreshEnhance(Weapon);
                }
                else
                {
                    if (!Weapon.IsMasterwork())
                    {
                        Weapon.AddAdjunct(new Masterwork(typeof(Masterwork)));
                    }

                    if (Weapon.MainHead.IsEnhanced())
                    {
                        if (Weapon.MainHead.ListedEnhancement != _level)
                        {
                            // get existing enhancements
                            var _exist = (from _aug in Weapon.MainHead.Adjuncts.OfType<MagicAugment>()
                                          where (_aug.Augmentation is Enhanced)
                                          select _aug).ToList();

                            // add new one
                            Weapon.MainHead.AddAdjunct(EnhancedExtension.GetEnhancedAugment(_level, new Evocation()));

                            // remove all old ones
                            foreach (var _a in _exist)
                                Weapon.MainHead.RemoveAdjunct(_a);
                        }
                    }
                    else
                    {
                        Weapon.MainHead.AddAdjunct(EnhancedExtension.GetEnhancedAugment(_level, new Evocation()));
                    }
                }
                MeleeWeapon.DoAugmentationsChanged();
            }
        }
        #endregion

        private void RemoveEnhancement()
        {
            // snapshot (so we don't iterate over a set we're modifying)
            var _list = (from _aug in Weapon.MainHead.Adjuncts.OfType<MagicAugment>()
                         where _aug.Augmentation is Enhanced
                         select _aug).ToList();

            // remove
            foreach (var _a in _list)
                Weapon.MainHead.RemoveAdjunct(_a);
            MeleeWeapon.DoAugmentationsChanged();
        }

        #region Adjunct Add
        private void cmdbndAdjunctAdd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!(Weapon is UnarmedWeapon) && (e.Parameter is MagicAugment _augment))
            {
                var _aType = _augment.Augmentation.GetType();

                // make sure no other adjunct of this type is already anchored
                if (!Weapon.MainHead.Adjuncts.OfType<MagicAugment>()
                    .Any(_a => (_a.Augmentation.GetType().Equals(_aType))))
                {
                    if (_augment.CanAnchor(Weapon.MainHead))
                        e.CanExecute = true;
                }
            }
            e.Handled = true;
        }

        private void cmdbndAdjunctAdd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is Adjunct _adjunct)
            {
                Weapon.MainHead.AddAdjunct(_adjunct);
                MeleeWeapon.DoAugmentationsChanged();
            }
        }
        #endregion

        #region Adjunct Remove
        private void cmdbndAdjunctRemove_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!(Weapon is UnarmedWeapon) && (lstAdjuncts?.SelectedItem != null))
            {
                var _adjunct = lstAdjuncts.SelectedItem as Adjunct;
                if (_adjunct.CanUnAnchor())
                    e.CanExecute = true;
            }
            e.Handled = true;
        }

        private void cmdbndAdjunctRemove_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (lstAdjuncts.SelectedItem != null)
            {
                var _adjunct = lstAdjuncts.SelectedItem as Adjunct;
                if (_adjunct.CanUnAnchor())
                {
                    Weapon.MainHead.RemoveAdjunct(_adjunct);
                    MeleeWeapon.DoAugmentationsChanged();
                }
            }
            e.Handled = true;
        }
        #endregion
    }
}
