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
using System.Windows.Shapes;
using Uzi.Ikosa.TypeListers;
using Uzi.Ikosa.Languages;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for LanguageEditor.xaml
    /// </summary>
    public partial class LanguageEditor : Window
    {
        public static RoutedCommand AddINT = new RoutedCommand();
        public static RoutedCommand AddSkill = new RoutedCommand();

        public LanguageEditor()
        {
            InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(LanguageEditor_DataContextChanged);
        }

        void LanguageEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SyncAvailable();
        }

        private void SyncAvailable()
        {
            var _critter = DataContext as Creature;
            var _available = LanguageLister.AvailableLanguages(_critter).ToList();
            lstAvailable.Items.Clear();
            foreach (var _avail in _available)
            {
                var _lang = Activator.CreateInstance(_avail.ListedType, (object)null) as Language;
                if (_lang.CanProject)
                {
                    lstAvailable.Items.Add(_lang);
                }
            }
        }

        private void cmdbndAddINT_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter != null)
            {
                if (DataContext is Creature _critter)
                {
                    var _remaining = _critter.Abilities.Intelligence.BonusLanguages
                        - _critter.Languages.Count(_l => _l.Source is Intelligence);
                    if (_remaining > 0)
                    {
                        e.CanExecute = true;
                    }
                }
            }
            e.Handled = true;
        }

        private void cmdbndAddINT_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (DataContext is Creature _critter)
            {
                var _lang = e.Parameter as Language;
                _lang = Activator.CreateInstance(_lang.GetType(), _critter.Abilities.Intelligence) as Language;
                _critter.Languages.Add(_lang);
                SyncAvailable();
                lstLanguages.Items.Refresh();
            }
            e.Handled = true;
        }

        private void cmdbndAddSkill_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter != null)
            {
                if (DataContext is Creature _critter)
                {
                    var _langSkill = _critter.Skills.Skill<LanguageSkill>();
                    if (_langSkill != null)
                    {
                        var _remaining = _langSkill.BaseValue
                            - _critter.Languages.Count(_l => _l.Source is LanguageSkill);
                        if (_remaining > 0)
                        {
                            e.CanExecute = true;
                        }
                    }
                }
            }
            e.Handled = true;
        }

        private void cmdbndAddSkill_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (DataContext is Creature _critter)
            {
                var _lang = e.Parameter as Language;
                _lang = Activator.CreateInstance(_lang.GetType(), _critter.Skills.Skill<LanguageSkill>()) as Language;
                _critter.Languages.Add(_lang);
                SyncAvailable();
                lstLanguages.Items.Refresh();
            }
            e.Handled = true;
        }

        private void cmdbndDelete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter != null)
            {
                var _lang = e.Parameter as Language;
                if ((_lang.Source is Intelligence) || (_lang.Source is LanguageSkill))
                {
                    e.CanExecute = true;
                }
            }
            e.Handled = true;
        }

        private void cmdbndDelete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _lang = e.Parameter as Language;
            var _critter = DataContext as Creature;
            _critter.Languages.Remove(_lang);
            SyncAvailable();
            lstLanguages.Items.Refresh();
            e.Handled = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
