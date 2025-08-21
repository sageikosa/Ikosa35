using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Core;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Advancement.CharacterClasses;
using Uzi.Ikosa.Magic.Spells;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for DoubleWeaponEditor.xaml
    /// </summary>
    public partial class DoubleWeaponEditor : UserControl
    {
        public DoubleWeaponEditor()
        {
            InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(DoubleWeaponEditor_DataContextChanged);
        }

        private DoubleMeleeWeaponVM DoubleMeleeWeapon => DataContext as DoubleMeleeWeaponVM;
        private DoubleMeleeWeaponBase Weapon => DoubleMeleeWeapon?.Thing;

        void DoubleWeaponEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Weapon != null)
            {
                var _weapon = Weapon;
                cboSize.SelectedIndex = _weapon.ItemSizer.ExpectedCreatureSize.Order + 4;

                // Masterwork and Enhancement
                if (_weapon.IsMasterwork())
                {
                    chkMasterwork.IsChecked = true;
                }

                RefreshEnhance(_weapon.Head[0], cboEnhancement0);
                RefreshEnhance(_weapon.Head[1], cboEnhancement1);

                // MENU
                CreateAddMenuItems(miAdd);
                CreateAddMenuItems(miAdd1);
            }
        }

        private void CreateAddMenuItems(MenuItem add)
        {
            add.Items.Clear();
            add.Items.Add(new MenuItem
            {
                Header = @"Defending",
                Command = ArmorEditor.AdjunctAdd,
                CommandParameter = new MagicAugment(new SpellSource(DivineCaster(8), 1, 1, false, new ShieldOfGrace()), new Defending())
            });
            add.Items.Add(new MenuItem
            {
                Header = @"Flaming",
                Command = ArmorEditor.AdjunctAdd,
                CommandParameter = new MagicAugment(new SpellSource(ArcaneCaster(10), 3, 3, false, new Fireball()), new Flaming())
            });
            add.Items.Add(new MenuItem
            {
                Header = @"Merciful",
                Command = ArmorEditor.AdjunctAdd,
                CommandParameter = new MagicAugment(new SpellSource(DivineCaster(5), 1, 1, false, new CureLightWounds()), new Merciful())
            });
            add.Items.Add(new MenuItem
            {
                Header = @"Shock",
                Command = ArmorEditor.AdjunctAdd,
                CommandParameter = new MagicAugment(new SpellSource(ArcaneCaster(8), 3, 3, false, new LightningBolt()), new Shock())
            });
            add.Items.Add(new MenuItem
            {
                Header = @"Speed",
                Command = ArmorEditor.AdjunctAdd,
                CommandParameter = new MagicAugment(new SpellSource(ArcaneCaster(7), 3, 3, false, new Haste()), new Speed())
            });
        }

        protected ItemCaster DivineCaster(int level)
            => new(MagicType.Divine, level, Alignment.TrueNeutral, 10 + level + (level / 2), Guid.Empty, typeof(Cleric));

        protected ItemCaster ArcaneCaster(int level)
            => new(MagicType.Arcane, level, Alignment.TrueNeutral, 10 + level + (level / 2), Guid.Empty, typeof(Wizard));

        #region private void RefreshEnhance(IMeleeWeapon weapon)
        private void RefreshEnhance(IWeaponHead head, ComboBox combo)
        {
            if (head.IsEnhanced())
            {
                if (combo.SelectedIndex != head.ListedEnhancement)
                {
                    combo.SelectedIndex = head.ListedEnhancement;
                }
            }
            else
            {
                if (combo.SelectedIndex != 0)
                {
                    combo.SelectedIndex = 0;
                }
            }
            DoubleMeleeWeapon.DoAugmentationsChanged();
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
            if (Weapon != null)
            {
                var _level = 0;
                IWeaponHead _head = null;
                ComboBox _combo = null;

                if (e.Source == cboEnhancement0)
                {
                    _level = cboEnhancement0.SelectedIndex;
                    _head = Weapon.Head[0];
                    _combo = cboEnhancement0;
                }
                if (e.Source == cboEnhancement1)
                {
                    _level = cboEnhancement1.SelectedIndex;
                    _head = Weapon.Head[1];
                    _combo = cboEnhancement1;
                }

                if (_level == 0)
                {
                    // remove
                    RemoveEnhancement(_head);
                    RefreshEnhance(_head, _combo);
                }
                else
                {
                    if (!Weapon.IsMasterwork())
                    {
                        chkMasterwork.IsChecked = true;
                    }

                    if (_head.IsEnhanced())
                    {
                        if (_head.ListedEnhancement != _level)
                        {
                            // get existing enhancements
                            var _exist = (from _aug in _head.Adjuncts.OfType<MagicAugment>()
                                          where (_aug.Augmentation is Enhanced)
                                          select _aug).ToList();

                            // add new one
                            _head.AddAdjunct(EnhancedExtension.GetEnhancedAugment(_level, new Evocation()));

                            // remove all old ones
                            foreach (var _a in _exist)
                            {
                                _head.RemoveAdjunct(_a);
                            }
                        }
                    }
                    else
                    {
                        _head.AddAdjunct(EnhancedExtension.GetEnhancedAugment(_level, new Evocation()));
                    }
                }
                DoubleMeleeWeapon.DoAugmentationsChanged();
            }
        }
        #endregion

        private void RemoveEnhancement(IWeaponHead head)
        {
            // snapshot (so we don't iterate over a set we're modifying)
            var _list = (from _aug in head.Adjuncts.OfType<MagicAugment>()
                         where _aug.Augmentation is Enhanced
                         select _aug).ToList();

            // remove
            foreach (var _a in _list)
            {
                _a.Anchor.RemoveAdjunct(_a);
            }

            DoubleMeleeWeapon.DoAugmentationsChanged();
        }

        #region Masterwork
        private void chkMasterwork_Checked(object sender, RoutedEventArgs e)
        {
            // ensure masterwork
            if (!Weapon.IsMasterwork())
            {
                Weapon.AddAdjunct(new Masterwork(typeof(Masterwork)));
            }
            if (!Weapon.IsMasterwork())
            {
                chkMasterwork.IsChecked = false;
            }
            DoubleMeleeWeapon.DoAugmentationsChanged();
        }

        private void chkMasterwork_Unchecked(object sender, RoutedEventArgs e)
        {
            // remove masterwork
            if (Weapon.IsMasterwork())
            {
                var _mw = Weapon.Adjuncts.Where(_a => _a is Masterwork).ToList();
                foreach (var _i in _mw)
                {
                    Weapon.RemoveAdjunct(_i);
                }
            }
            if (Weapon.IsMasterwork())
            {
                chkMasterwork.IsChecked = true;
            }
            DoubleMeleeWeapon.DoAugmentationsChanged();
        }
        #endregion

        #region Adjunct Add
        private void cmdbndAdjunctAdd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter is MagicAugment _augment)
            {
                Type _aType = _augment.Augmentation.GetType();

                IWeaponHead _head = (sender == miAdd ? Weapon.Head[0] : Weapon.Head[1]);

                // make sure no other adjunct of this type is already anchored
                if (!_head.Adjuncts.OfType<MagicAugment>()
                    .Any(_a => (_a.Augmentation.GetType().Equals(_aType))))
                {
                    if (_augment.CanAnchor(_head))
                    {
                        e.CanExecute = true;
                    }
                }
            }
            e.Handled = true;
        }

        private void cmdbndAdjunctAdd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is Adjunct _adjunct)
            {
                IWeaponHead _head = (sender == miAdd ? Weapon.Head[0] : Weapon.Head[1]);
                _head.AddAdjunct(_adjunct);
                DoubleMeleeWeapon.DoAugmentationsChanged();
            }
        }
        #endregion

        #region Adjunct Remove
        private void cmdbndAdjunctRemove_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                var _list = (e.Source == miRemove ? lstAdjuncts : lstAdjuncts1);
                if ((_list != null) && (_list.SelectedItem != null))
                {
                    var _adjunct = _list.SelectedItem as Adjunct;
                    if (_adjunct.CanUnAnchor())
                    {
                        e.CanExecute = true;
                    }
                }
                e.Handled = true;
            }
            catch
            {
            }
        }

        private void cmdbndAdjunctRemove_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _list = (e.Source == miRemove ? lstAdjuncts : lstAdjuncts1);
            if (_list.SelectedItem != null)
            {
                var _adjunct = _list.SelectedItem as Adjunct;
                if (_adjunct.CanUnAnchor())
                {
                    _adjunct.Anchor.RemoveAdjunct(_adjunct);
                    DoubleMeleeWeapon.DoAugmentationsChanged();
                }
            }
            e.Handled = true;
        }
        #endregion
    }
}
