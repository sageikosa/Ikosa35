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
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Advancement.CharacterClasses;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Core;
using Uzi.Ikosa.Magic.Spells;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.UI;
using Uzi.Ikosa.Items.Weapons.Ranged;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for AmmoEditor.xaml
    /// </summary>
    public partial class AmmoEditor : UserControl
    {
        public AmmoEditor()
        {
            InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(AmmoEditor_DataContextChanged);
        }

        protected ItemCaster DivineCaster(int level)
            => new(MagicType.Divine, level, Alignment.TrueNeutral, 10 + level + (level / 2), Guid.Empty, typeof(Cleric));

        protected ItemCaster ArcaneCaster(int level)
            => new(MagicType.Arcane, level, Alignment.TrueNeutral, 10 + level + (level / 2), Guid.Empty, typeof(Wizard));

        private AmmoEditSet AmmoSet => DataContext as AmmoEditSet;

        void AmmoEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (AmmoSet != null)
            {
                // size sync
                var _ammoSet = AmmoSet;

                // Masterwork and Enhancement
                RefreshEnhance(_ammoSet);

                // independent adjuncts
                RefreshAdjuncts();

                // MENU
                miAdd.Items.Clear();
                miAdd.Items.Add(new MenuItem
                {
                    Header = @"Flaming",
                    Command = ArmorEditor.AdjunctAdd,
                    CommandParameter = new MagicAugment(new SpellSource(ArcaneCaster(10), 3, 3, false, new Fireball()), new Flaming())
                });
                miAdd.Items.Add(new MenuItem
                {
                    Header = @"Shock",
                    Command = ArmorEditor.AdjunctAdd,
                    CommandParameter = new MagicAugment(new SpellSource(ArcaneCaster(8), 3, 3, false, new LightningBolt()), new Shock())
                });
            }
        }

        #region private void RefreshEnhance(AmmoVM weapon)
        private void RefreshEnhance(AmmoEditSet ammoSet)
        {
            if (ammoSet.Ammunition.IsMasterwork())
            {
                if (ammoSet.Ammunition.IsEnhanced())
                {
                    if (cboEnhancement.SelectedIndex != ammoSet.Ammunition.ListedEnhancement + 1)
                        cboEnhancement.SelectedIndex = ammoSet.Ammunition.ListedEnhancement + 1;
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
        }
        #endregion

        private void RefreshAdjuncts()
        {
            BindingOperations.GetBindingExpressionBase(tbPrice, TextBlock.TextProperty).UpdateTarget();
            BindingOperations.GetBindingExpressionBase(tbWeight, TextBlock.TextProperty).UpdateTarget();
            AmmoSet.Bundle.SyncSets();
            lstAdjuncts.ItemsSource = AmmoSet.Ammunition.Adjuncts;
        }

        #region private void cboEnhancement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        private void cboEnhancement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AmmoSet != null)
            {
                var _proto = AmmoSet.Ammunition;
                var _level = cboEnhancement.SelectedIndex - 1;
                if (_level == -1)
                {
                    // remove
                    RemoveEnhancement();

                    // remove masterwork
                    if (_proto.IsMasterwork())
                    {
                        var _mw = _proto.Adjuncts.Where(_a => _a is Masterwork).ToList();
                        foreach (var _i in _mw)
                        {
                            _proto.RemoveAdjunct(_i);
                        }
                    }
                    RefreshEnhance(AmmoSet);
                }
                else if (_level == 0)
                {
                    // remove
                    RemoveEnhancement();

                    // ensure masterwork
                    if (!_proto.IsMasterwork())
                    {
                        _proto.AddAdjunct(new Masterwork(typeof(Masterwork)));
                    }

                    RefreshEnhance(AmmoSet);
                }
                else
                {
                    if (!_proto.IsMasterwork())
                    {
                        _proto.AddAdjunct(new Masterwork(typeof(Masterwork)));
                    }

                    if (_proto.IsEnhanced())
                    {
                        if (_proto.ListedEnhancement != _level)
                        {
                            // get existing enhancements
                            var _exist = (from _aug in _proto.Adjuncts.OfType<MagicAugment>()
                                          where (_aug.Augmentation is Enhanced)
                                          select _aug).ToList();

                            // add new one
                            _proto.AddAdjunct(EnhancedExtension.GetEnhancedAugment(_level, new Evocation()));

                            // remove all old ones
                            foreach (var _a in _exist)
                                _proto.RemoveAdjunct(_a);
                        }
                    }
                    else
                    {
                        _proto.AddAdjunct(EnhancedExtension.GetEnhancedAugment(_level, new Evocation()));
                    }
                }
            }
            BindingOperations.GetBindingExpressionBase(tbPrice, TextBlock.TextProperty).UpdateTarget();
            BindingOperations.GetBindingExpressionBase(tbWeight, TextBlock.TextProperty).UpdateTarget();
            AmmoSet.Bundle.SyncSets();
        }
        #endregion

        private void RemoveEnhancement()
        {
            // snapshot (so we don't iterate over a set we're modifying)
            var _proto = AmmoSet?.Ammunition;
            var _list = (from _aug in _proto?.Adjuncts.OfType<MagicAugment>()
                         where _aug.Augmentation is Enhanced
                         select _aug).ToList();

            // remove
            foreach (var _a in _list)
                _proto.RemoveAdjunct(_a);
        }

        #region private void cmdbndAdjunctAdd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cmdbndAdjunctAdd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter is MagicAugment _augment)
            {
                Type _aType = _augment.Augmentation.GetType();

                // make sure no other adjunct of this type is already anchored
                if (!AmmoSet.Ammunition.Adjuncts.OfType<MagicAugment>()
                    .Any(_a => (_a.Augmentation.GetType().Equals(_aType))))
                {
                    if (_augment.CanAnchor(AmmoSet.Ammunition))
                        e.CanExecute = true;
                }
            }
            e.Handled = true;
        }
        #endregion

        private void cmdbndAdjunctAdd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is Adjunct _adjunct)
            {
                AmmoSet.Ammunition.AddAdjunct(_adjunct);
                RefreshAdjuncts();
            }
        }

        #region private void cmdbndAdjunctRemove_CanExecute(object sender, CanExecuteRoutedEventArgs e)
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
        #endregion

        #region private void cmdbndAdjunctRemove_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cmdbndAdjunctRemove_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (lstAdjuncts.SelectedItem != null)
            {
                var _adjunct = lstAdjuncts.SelectedItem as Adjunct;
                if (_adjunct.CanUnAnchor())
                {
                    AmmoSet.Ammunition.RemoveAdjunct(_adjunct);
                    RefreshAdjuncts();
                }
            }
            e.Handled = true;
        }
        #endregion
    }
}
