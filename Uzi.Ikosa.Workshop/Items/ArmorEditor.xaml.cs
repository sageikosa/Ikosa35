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
using Uzi.Ikosa.Items.Armor;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Advancement.CharacterClasses;
using Uzi.Ikosa.Magic.Spells;
using Uzi.Core;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for ArmorEditor.xaml
    /// </summary>
    public partial class ArmorEditor : UserControl
    {
        public static RoutedCommand AdjunctAdd = new();
        public static RoutedCommand AdjunctRemove = new();

        public ArmorEditor()
        {
            InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(ArmorEditor_DataContextChanged);

            // TODO: make stuff below a common function that can be (partially) shared with shields
            miAdd.Items.Add(new MenuItem
            {
                Header = @"Acid Resist 10",
                Command = AdjunctAdd,
                CommandParameter = new MagicAugment(new SpellSource(ClericCaster(3), 2, 2, false, new ResistEnergy()), new AcidResistorLow())
            });
            miAdd.Items.Add(new MenuItem
            {
                Header = @"Cold Resist 10",
                Command = AdjunctAdd,
                CommandParameter = new MagicAugment(new SpellSource(ClericCaster(3), 2, 2, false, new ResistEnergy()), new ColdResistorLow())
            });
            miAdd.Items.Add(new MenuItem
            {
                Header = @"Electric Resist 10",
                Command = AdjunctAdd,
                CommandParameter = new MagicAugment(new SpellSource(ClericCaster(3), 2, 2, false, new ResistEnergy()), new ElectricResistorLow())
            });
            miAdd.Items.Add(new MenuItem
            {
                Header = @"Fire Resist 10",
                Command = AdjunctAdd,
                CommandParameter = new MagicAugment(new SpellSource(ClericCaster(3), 2, 2, false, new ResistEnergy()), new FireResistorLow())
            });
            miAdd.Items.Add(new MenuItem
            {
                Header = @"Sonic Resist 10",
                Command = AdjunctAdd,
                CommandParameter = new MagicAugment(new SpellSource(ClericCaster(3), 2, 2, false, new ResistEnergy()), new SonicResistorLow())
            });
            miAdd.Items.Add(new MenuItem
            {
                Header = @"Slippery",
                Command = AdjunctAdd,
                CommandParameter = new MagicAugment(new SpellSource(WizardCaster(5), 2, 2, false, new SlipperySurface()), new SlipperyArmorLow())
            });
        }

        protected ItemCaster ClericCaster(int level)
            => new(MagicType.Divine, level, Alignment.TrueNeutral, 10 + level + (level / 2), Guid.Empty, typeof(Cleric));

        protected ItemCaster WizardCaster(int level)
            => new(MagicType.Arcane, level, Alignment.TrueNeutral, 10 + level + (level / 2), Guid.Empty, typeof(Wizard));

        private ArmorVM ArmorVM => DataContext as ArmorVM;
        private ArmorBase Armor => ArmorVM?.Thing;

        void ArmorEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Armor != null)
            {
                var _armor = Armor;
                cboSize.SelectedIndex = _armor.ItemSizer.ExpectedCreatureSize.Order + 4;

                // Masterwork and Enhancement
                RefreshEnhance(_armor);
            }
        }

        #region private void RefreshEnhance(ArmorBase _armor)
        private void RefreshEnhance(ArmorBase _armor)
        {
            if (_armor.IsMasterwork())
            {
                if (_armor.IsEnhanced())
                {
                    if (cboEnhancement.SelectedIndex != _armor.ListedEnhancement + 1)
                        cboEnhancement.SelectedIndex = _armor.ListedEnhancement + 1;
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
            ArmorVM?.DoAugmentationsChanged();
        }
        #endregion

        private void cboSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Armor != null)
            {
                var _cbItem = cboSize.SelectedItem as ComboBoxItem;
                Armor.ItemSizer.ExpectedCreatureSize = _cbItem.Tag as Size;
            }
        }

        #region private void cboEnhancement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        private void cboEnhancement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Armor != null)
            {
                int _level = cboEnhancement.SelectedIndex - 1;
                if (_level == -1)
                {
                    // remove
                    RemoveEnhancement();

                    // remove masterwork
                    if (Armor.IsMasterwork())
                    {
                        var _mw = Armor.Adjuncts.Where(_a => _a is Masterwork).ToList();
                        foreach (var _i in _mw)
                            Armor.RemoveAdjunct(_i);
                    }
                    RefreshEnhance(Armor);
                }
                else if (_level == 0)
                {
                    // remove
                    RemoveEnhancement();

                    // ensure masterwork
                    if (!Armor.IsMasterwork())
                    {
                        Armor.AddAdjunct(new Masterwork(typeof(Masterwork)));
                    }

                    RefreshEnhance(Armor);
                }
                else
                {
                    if (!Armor.IsMasterwork())
                    {
                        Armor.AddAdjunct(new Masterwork(typeof(Masterwork)));
                    }

                    if (Armor.IsEnhanced())
                    {
                        if (Armor.ListedEnhancement != _level)
                        {
                            // get existing enhancements
                            var _exist = (from _aug in Armor.Adjuncts.OfType<MagicAugment>()
                                          where (_aug.Augmentation is Enhanced)
                                          select _aug).ToList();

                            // add new one
                            Armor.AddAdjunct(EnhancedExtension.GetEnhancedAugment(_level, new Abjuration()));

                            // remove all old ones
                            foreach (var _a in _exist)
                                Armor.RemoveAdjunct(_a);
                        }
                    }
                    else
                    {
                        Armor.AddAdjunct(EnhancedExtension.GetEnhancedAugment(_level, new Abjuration()));
                    }
                }
                ArmorVM?.DoAugmentationsChanged();
            }
        }
        #endregion

        private void RemoveEnhancement()
        {
            // snapshot (so we don't iterate over a set we're modifying)
            var _list = (from _aug in Armor.Adjuncts.OfType<MagicAugment>()
                         where _aug.Augmentation is Enhanced
                         select _aug).ToList();

            // remove
            foreach (var _a in _list)
                Armor.RemoveAdjunct(_a);
            ArmorVM?.DoAugmentationsChanged();
        }

        #region Adjunct Add
        private void cmdbndAdjunctAdd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter is MagicAugment _augment)
            {
                Type _aType = _augment.Augmentation.GetType();

                // make sure no other adjunct of this type is already anchored
                if (!Armor.Adjuncts.OfType<MagicAugment>()
                    .Any(_a => (_a.Augmentation.GetType().Equals(_aType))))
                {
                    if (_augment.CanAnchor(Armor))
                        e.CanExecute = true;
                }
            }
            e.Handled = true;
        }

        private void cmdbndAdjunctAdd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is Adjunct _adjunct)
            {
                Armor.AddAdjunct(_adjunct);
                ArmorVM?.DoAugmentationsChanged();
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
                    Armor.RemoveAdjunct(_adjunct);
                    ArmorVM?.DoAugmentationsChanged();
                }
            }
            e.Handled = true;
        }
        #endregion
    }
}
