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
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.TypeListers;
using System.Windows.Controls.Primitives;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Universal;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>Interaction logic for AdvancementSheet.xaml</summary>
    public partial class AdvancementSheet : UserControl
    {
        public static RoutedCommand AddLevel = new RoutedCommand();

        public AdvancementSheet()
        {
            InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(AdvancementSheet_DataContextChanged);
            _Refresh = new RelayCommand(() => SyncClasses());
        }

        private RelayCommand _Refresh;
        public RelayCommand Refresh => _Refresh;

        protected PresentableCreatureVM PresentableCreature 
            => DataContext as PresentableCreatureVM;

        private Creature Creature 
            => PresentableCreature.Thing;

        #region void AdvancementSheet_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        void AdvancementSheet_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SyncClasses();
        }
        #endregion

        #region private void SyncClasses()
        private void SyncClasses()
        {
            cboAddClass.Items.Clear();
            cboAddLevels.Items.Clear();
            var _critter = Creature;
            if (_critter != null)
            {
                // has monster classes already?
                foreach (var _bmc in _critter.Classes.OfType<BaseMonsterClass>())
                {
                    var _cbi = new ComboBoxItem
                    {
                        Content = _bmc.ClassName,
                        Tag = _bmc.GetType()
                    };
                    cboAddClass.Items.Add(_cbi);
                }

                // can creature advance by class?
                if (_critter.Species.IsCharacterCapable)
                {
                    foreach (var _ccKvp in Campaign.SystemCampaign.ClassLists[@"Character"])
                    {
                        var _cbi = new ComboBoxItem
                        {
                            Content = _ccKvp.Value.ListedType.Name,
                            Tag = _ccKvp.Value.ListedType
                        };
                        cboAddClass.Items.Add(_cbi);
                    }
                }
                if (cboAddClass.Items.Count == 1)
                {
                    cboAddClass.SelectedIndex = 0;
                }
            }
        }
        #endregion

        #region private void SyncLevels()
        private void SyncLevels()
        {
            if ((cboAddClass != null) && (cboAddClass.SelectedItem != null))
            {
                var _type = (cboAddClass.SelectedItem as ComboBoxItem).Tag as Type;
                if (typeof(BaseMonsterClass).IsAssignableFrom(_type))
                {
                    #region monster class
                    // if monster class, find maximum levels available, and current level
                    var _bmc = Creature.Classes.Get<BaseMonsterClass>();
                    cboAddLevels.Items.Clear();
                    if (_bmc != null)
                    {
                        for (var _level = _bmc.CurrentLevel + 1; _level <= _bmc.MaxLevel; _level++)
                        {
                            cboAddLevels.Items.Add(new ComboBoxItem
                            {
                                Content = _level.ToString(),
                                Tag = _level
                            });
                        }
                        if (cboAddLevels.Items.Count > 0)
                        {
                            cboAddLevels.SelectedIndex = 0;
                        }
                    }
                    #endregion
                }
                else if (typeof(CharacterClass).IsAssignableFrom(_type))
                {
                    #region character class
                    var _charLevel = Creature.Classes.CharacterLevel;
                    var _cc = Creature.Classes.Get<CharacterClass>(_type);
                    cboAddLevels.Items.Clear();
                    if (_cc != null)
                    {
                        // already has the class, so start at highest level +1
                        for (var _level = _cc.CurrentLevel + 1; _level <= _cc.MaxLevel; _level++)
                        {
                            if ((_charLevel + (_level - _cc.CurrentLevel)) <= 20)
                            {
                                cboAddLevels.Items.Add(new ComboBoxItem
                                {
                                    Content = _level.ToString(),
                                    Tag = _level
                                });
                            }
                        }
                        if (cboAddLevels.Items.Count > 0)
                        {
                            cboAddLevels.SelectedIndex = 0;
                        }
                    }
                    else
                    {
                        // start at 1
                        _cc = Activator.CreateInstance(_type) as CharacterClass;
                        for (var _level = 1; _level <= _cc.MaxLevel; _level++)
                        {
                            if ((_charLevel + _level) <= 20)
                            {
                                cboAddLevels.Items.Add(new ComboBoxItem
                                {
                                    Content = _level.ToString(),
                                    Tag = _level
                                });
                            }
                        }
                        if (cboAddLevels.Items.Count > 0)
                        {
                            cboAddLevels.SelectedIndex = 0;
                        }
                    }
                    #endregion
                }
            }
        }
        #endregion

        #region private void cboAddClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
        private void cboAddClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SyncLevels();
        }
        #endregion

        #region private void cmdbndAddLevel_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cmdbndAddLevel_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (cboAddClass != null) && (cboAddLevels != null) &&
                (cboAddClass.SelectedItem != null) && (cboAddLevels.SelectedItem != null);
            e.Handled = true;
        }
        #endregion

        #region private void cmdbndAddLevel_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cmdbndAddLevel_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if ((cboAddClass != null) && (cboAddLevels != null) &&
                (cboAddClass.SelectedItem != null) && (cboAddLevels.SelectedItem != null))
            {
                // find selected level
                var _topLevel = (int)(cboAddLevels.SelectedItem as ComboBoxItem).Tag;
                var _type = (cboAddClass.SelectedItem as ComboBoxItem).Tag as Type;
                if (typeof(BaseMonsterClass).IsAssignableFrom(_type))
                {
                    // if monster class, should already have levels in it
                    var _bmc = Creature.Classes.Get<BaseMonsterClass>();
                    if (_bmc != null)
                    {
                        for (var _level = _bmc.CurrentLevel + 1; _level <= _topLevel; _level++)
                        {
                            Creature.IsInSystemEditMode = true;
                            _bmc.IncreaseLevel(PowerDieCalcMethod.Average);
                            Creature.IsInSystemEditMode = false;
                        }
                    }
                }
                else if (typeof(CharacterClass).IsAssignableFrom(_type))
                {
                    // character class
                    var _cc = Creature.Classes.Get<CharacterClass>(_type);
                    Creature.IsInSystemEditMode = true;
                    if (_cc == null)
                    {
                        _cc = Activator.CreateInstance(_type) as CharacterClass;
                        _cc.BindTo(Creature);
                    }
                    for (var _level = _cc.CurrentLevel + 1; _level <= _topLevel; _level++)
                    {
                        _cc.IncreaseLevel(PowerDieCalcMethod.Average);
                    }
                    Creature.IsInSystemEditMode = false;
                }
                lvAdvLog.Items.Refresh();
                PresentableCreature.Rebind();
            }
            e.Handled = true;
        }
        #endregion

        private void gridMultiRequirements_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var _multiReq = new MultiRequirementsEditor()
            {
                DataContext = (sender as Grid).DataContext,
                Owner = Window.GetWindow(this),
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            _multiReq.ShowDialog();
            lvAdvLog.Items.Refresh();
            PresentableCreature.Rebind();
        }
    }
}
