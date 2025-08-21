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
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Universal;
using Uzi.Ikosa.Advancement.CharacterClasses;
using Uzi.Ikosa.Magic.SpellLists;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.UI.MVVM;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for ScrollEditor.xaml
    /// </summary>
    public partial class ScrollEditor : UserControl
    {
        public static RoutedCommand SpellAdd = new();
        public static RoutedCommand SpellRemove = new();
        public static RoutedCommand SpellDecipher = new();

        public ScrollEditor()
        {
            InitializeComponent();

            ClassSpells(@"Wizard", typeof(Wizard), WizardCaster);
            ClassSpells(@"Cleric", typeof(Cleric), ClericCaster);
        }

        #region private void ClassSpells(string name, Type classType, CasterFunction casterFunction)
        private void ClassSpells(string name, Type classType, CasterFunction casterFunction)
        {
            // class spells
            var _spells = new MenuItem
            {
                Header = name
            };
            miAdd.Items.Add(_spells);
            foreach (var _spellLevel in from _csl in Campaign.SystemCampaign.SpellLists[classType.FullName]
                                        orderby _csl.Key
                                        select _csl.Value)
            {
                // by level
                var _levelSpells = new MenuItem
                {
                    Header = string.Format(@"Level {0}", _spellLevel.Level)
                };
                _spells.Items.Add(_levelSpells);

                // and all of the above
                var _casterLevel = _spellLevel.Level * 2 - 1;
                if (_casterLevel < 1)
                {
                    _casterLevel = 1;
                }

                foreach (var _classSpell in _spellLevel)
                {
                    _levelSpells.Items.Add(new MenuItem
                    {
                        Header = _classSpell.SpellDef,
                        Command = SpellAdd,
                        CommandParameter = new SpellSource(casterFunction(_casterLevel), _classSpell.Level, _classSpell.Level, false, _classSpell.SpellDef)
                    });
                }
            }
        }
        #endregion

        private delegate ItemCaster CasterFunction(int level);

        protected ItemCaster ClericCaster(int level)
            => new(MagicType.Divine, level, Alignment.TrueNeutral, 10 + level + (level / 2), Guid.Empty, typeof(Cleric));

        protected ItemCaster WizardCaster(int level)
            => new(MagicType.Arcane, level, Alignment.TrueNeutral, 10 + level + (level / 2), Guid.Empty, typeof(Wizard));

        private Scroll _Scroll => (DataContext as ScrollVM)?.Thing;

        #region Spell Remove
        private void cmdbndSpellRemove_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (lstSpells != null)
            {
                e.CanExecute = lstSpells.SelectedItem != null;
            }
            e.Handled = true;
        }

        private void cmdbndSpellRemove_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            (lstSpells.SelectedItem as SpellCompletion).Eject();
            lstSpells.ItemsSource = null;
            lstSpells.ItemsSource = _Scroll.Spells;
            e.Handled = true;
        }
        #endregion

        #region Spell Add
        private void cmdbndSpellAdd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cmdbndSpellAdd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _Scroll.AddAdjunct(new SpellCompletion(e.Parameter as SpellSource, false));
        }
        #endregion

        private void cmdbndSpellDecipher_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _complete = (lstSpells.SelectedItem as SpellCompletion);
            if (_Scroll.Possessor != null)
            {
                if (!_complete.HasDeciphered(_Scroll.Possessor.ID))
                {
                    _complete.Casters.Add(_Scroll.Possessor.ID);
                    lstSpells.ItemsSource = null;
                    lstSpells.ItemsSource = _Scroll.Spells;
                }
            }
            e.Handled = true;
        }

        private void cmdbndSpellDecipher_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (lstSpells != null)
            {
                if (_Scroll?.Possessor != null)
                {
                    e.CanExecute = lstSpells.SelectedItem != null;
                }
            }
            e.Handled = true;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var _complete = (sender as CheckBox).Tag as SpellCompletion;
            if (_Scroll.Possessor != null)
            {
                if (!_complete.HasDeciphered(_Scroll.Possessor.ID))
                {
                    _complete.Casters.Add(_Scroll.Possessor.ID);
                    lstSpells.ItemsSource = null;
                    lstSpells.ItemsSource = _Scroll.Spells;
                }
            }
            e.Handled = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var _complete = (sender as CheckBox).Tag as SpellCompletion;
            if (_Scroll.Possessor != null)
            {
                if (_complete.HasDeciphered(_Scroll.Possessor.ID))
                {
                    _complete.Casters.Remove(_Scroll.Possessor.ID);
                    lstSpells.ItemsSource = null;
                    lstSpells.ItemsSource = _Scroll.Spells;
                }
            }
            e.Handled = true;
        }
    }
}
