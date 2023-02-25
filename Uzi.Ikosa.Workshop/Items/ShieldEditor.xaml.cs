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
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Advancement.CharacterClasses;
using Uzi.Ikosa.Items.Armor;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items.Shields;
using Uzi.Ikosa.Magic.Spells;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for ShieldEditor.xaml
    /// </summary>
    public partial class ShieldEditor : UserControl
    {
        public ShieldEditor()
        {
            InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(ShieldEditor_DataContextChanged);

            miAdd.Items.Add(new MenuItem
            {
                Header = @"Acid Resist 10",
                Command = ArmorEditor.AdjunctAdd,
                CommandParameter = new MagicAugment(new SpellSource(DivineCaster(3), 2, 2, false, new ResistEnergy()), new AcidResistorLow())
            });
            miAdd.Items.Add(new MenuItem
            {
                Header = @"Cold Resist 10",
                Command = ArmorEditor.AdjunctAdd,
                CommandParameter = new MagicAugment(new SpellSource(DivineCaster(3), 2, 2, false, new ResistEnergy()), new ColdResistorLow())
            });
            miAdd.Items.Add(new MenuItem
            {
                Header = @"Electric Resist 10",
                Command = ArmorEditor.AdjunctAdd,
                CommandParameter = new MagicAugment(new SpellSource(DivineCaster(3), 2, 2, false, new ResistEnergy()), new ElectricResistorLow())
            });
            miAdd.Items.Add(new MenuItem
            {
                Header = @"Fire Resist 10",
                Command = ArmorEditor.AdjunctAdd,
                CommandParameter = new MagicAugment(new SpellSource(DivineCaster(3), 2, 2, false, new ResistEnergy()), new FireResistorLow())
            });
            miAdd.Items.Add(new MenuItem
            {
                Header = @"Sonic Resist 10",
                Command = ArmorEditor.AdjunctAdd,
                CommandParameter = new MagicAugment(new SpellSource(DivineCaster(3), 2, 2, false, new ResistEnergy()), new SonicResistorLow())
            });
        }

        protected ItemCaster DivineCaster(int level)
            => new(MagicType.Divine, level, Alignment.TrueNeutral, 10 + level + (level / 2), Guid.Empty, typeof(Cleric));

        protected ItemCaster ArcaneCaster(int level)
            => new(MagicType.Arcane, level, Alignment.TrueNeutral, 10 + level + (level / 2), Guid.Empty, typeof(Wizard));

        private ShieldVM ShieldVM => DataContext as ShieldVM;
        private ShieldBase Shield => ShieldVM?.Thing;

        void ShieldEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Shield != null)
            {
                var _shield = Shield;
                cboSize.SelectedIndex = _shield.ItemSizer.ExpectedCreatureSize.Order + 4;

                // Masterwork and Enhancement
                RefreshEnhance(_shield);
            }
        }

        #region private void RefreshEnhance(ShieldBase _shield)
        private void RefreshEnhance(ShieldBase shield)
        {
            if (shield.IsMasterwork())
            {
                if (shield.IsEnhanced())
                {
                    if (cboEnhancement.SelectedIndex != shield.ListedEnhancement + 1)
                        cboEnhancement.SelectedIndex = shield.ListedEnhancement + 1;
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
            ShieldVM?.DoAugmentationsChanged();
        }
        #endregion

        private void cboSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Shield != null)
            {
                var _cbItem = cboSize.SelectedItem as ComboBoxItem;
                Shield.ItemSizer.ExpectedCreatureSize = _cbItem.Tag as Size;
            }
        }

        #region private void cboEnhancement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        private void cboEnhancement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Shield != null)
            {
                var _level = cboEnhancement.SelectedIndex - 1;
                if (_level == -1)
                {
                    // remove
                    RemoveEnhancement();

                    // remove masterwork
                    if (Shield.IsMasterwork())
                    {
                        var _mw = Shield.Adjuncts.Where(_a => _a is Masterwork).ToList();
                        foreach (var _i in _mw)
                            Shield.RemoveAdjunct(_i);
                    }
                    RefreshEnhance(Shield);
                }
                else if (_level == 0)
                {
                    // remove
                    RemoveEnhancement();

                    // ensure masterwork
                    if (!Shield.IsMasterwork())
                    {
                        Shield.AddAdjunct(new Masterwork(typeof(Masterwork)));
                    }

                    RefreshEnhance(Shield);
                }
                else
                {
                    if (!Shield.IsMasterwork())
                    {
                        Shield.AddAdjunct(new Masterwork(typeof(Masterwork)));
                    }

                    if (Shield.IsEnhanced())
                    {
                        if (Shield.ListedEnhancement != _level)
                        {
                            // get existing enhancements
                            var _exist = (from _aug in Shield.Adjuncts.OfType<MagicAugment>()
                                          where (_aug.Augmentation is Enhanced)
                                          select _aug).ToList();

                            // add new one
                            Shield.AddAdjunct(EnhancedExtension.GetEnhancedAugment(_level, new Abjuration()));

                            // remove all old ones
                            foreach (var _a in _exist)
                                Shield.RemoveAdjunct(_a);
                        }
                    }
                    else
                    {
                        Shield.AddAdjunct(EnhancedExtension.GetEnhancedAugment(_level, new Abjuration()));
                    }
                }
                ShieldVM?.DoAugmentationsChanged();
            }
        }
        #endregion

        private void RemoveEnhancement()
        {
            // snapshot (so we don't iterate over a set we're modifying)
            var _list = (from _aug in Shield.Adjuncts.OfType<MagicAugment>()
                         where _aug.Augmentation is Enhanced
                         select _aug).ToList();

            // remove
            foreach (var _a in _list)
                Shield.RemoveAdjunct(_a);
            ShieldVM?.DoAugmentationsChanged();
        }

        #region Adjunct Add
        private void cmdbndAdjunctAdd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter is MagicAugment _augment)
            {
                Type _aType = _augment.Augmentation.GetType();

                // make sure no other adjunct of this type is already anchored
                if (!Shield.Adjuncts.OfType<MagicAugment>()
                    .Any(_a => (_a.Augmentation.GetType().Equals(_aType))))
                {
                    if (_augment.CanAnchor(Shield))
                        e.CanExecute = true;
                }
            }
            e.Handled = true;
        }

        private void cmdbndAdjunctAdd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is Adjunct _adjunct)
            {
                Shield.AddAdjunct(_adjunct);
                ShieldVM?.DoAugmentationsChanged();
            }
        }
        #endregion

        #region Adjunct Remove
        private void cmdbndAdjunctRemove_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if ((lstAdjuncts != null) && (lstAdjuncts.SelectedItem != null))
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
                    Shield.RemoveAdjunct(_adjunct);
                    ShieldVM?.DoAugmentationsChanged();
                }
            }
            e.Handled = true;
        }
        #endregion
    }
}
