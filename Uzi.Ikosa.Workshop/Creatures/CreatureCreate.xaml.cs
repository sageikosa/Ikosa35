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
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Universal;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Fidelity;
using Uzi.Ikosa.TypeListers;
using Uzi.Core.Dice;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for CreatureCreate.xaml
    /// </summary>
    public partial class CreatureCreate : UserControl
    {
        public CreatureCreate()
        {
            InitializeComponent();
            cboSpecies.ItemsSource = SpeciesLister.AllSpecies().OrderBy(_s => _s.Description);
        }

        private Point _RollsPt = new Point();
        private void lstValues_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _RollsPt = e.GetPosition(null);
        }

        public bool CanCreate
        {
            get
            {
                if ((cboSpecies != null) && (cboSpecies.SelectedValue != null))
                {
                    if ((ccSTR != null) && ((ccSTR.Content is int) || (ccSTR.Content is AbilityBase))
                        && (ccDEX != null) && ((ccDEX.Content is int) || (ccDEX.Content is AbilityBase))
                        && (ccCON != null) && ((ccCON.Content is int) || (ccCON.Content is AbilityBase))
                        && (ccINT != null) && ((ccINT.Content is int) || (ccINT.Content is AbilityBase))
                        && (ccWIS != null) && ((ccWIS.Content is int) || (ccWIS.Content is AbilityBase))
                        && (ccCHA != null) && ((ccCHA.Content is int) || (ccCHA.Content is AbilityBase))
                        )
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        #region private void lstValues_PreviewMouseMove(object sender, MouseEventArgs e)
        private void lstValues_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);
            Vector diff = _RollsPt - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                // Get the dragged ListViewItem
                var _rollList = sender as ListBox;
                if (_rollList.SelectedItem != null)
                {
                    // Find the data behind the ListViewItem
                    var _rollVal = (int)((FrameworkElement)_rollList.SelectedItem).Tag;

                    // TODO: fake out .NET adorner by allowing entire control to be a drop target
                    // TODO: so it can get drag move events for mouse position

                    // Initialize the drag & drop operation
                    var dragData = new DataObject(@"Roll", _rollVal);
                    var _effect = DragDrop.DoDragDrop(_rollList, dragData, DragDropEffects.Move);
                    if (_effect == DragDropEffects.Move)
                    {
                        _rollList.Items.Remove(_rollList.SelectedItem);
                        e.Handled = true;
                    }
                }
            }
        }
        #endregion

        #region private void Border_DragEnter(object sender, DragEventArgs e)
        private void Border_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(@"Roll"))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }
        #endregion

        #region private void Border_DragOver(object sender, DragEventArgs e)
        private void Border_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(@"Roll"))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }
        #endregion

        #region private void Border_Drop(object sender, DragEventArgs e)
        private void Border_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(@"Roll"))
            {
                var _content = sender as ContentControl;
                var _prev = _content.Content;
                _content.Content = e.Data.GetData(@"Roll");
                e.Handled = true;

                if ((_prev != null) && _prev.GetType().Equals(typeof(int)))
                {
                    var _lbl = new Label
                    {
                        Content = _prev,
                        Tag = _prev,
                        ToolTip = _prev
                    };
                    lstValues.Items.Add(_lbl);
                }
            }
        }
        #endregion

        #region protected void ButtonClick(object sender, RoutedEventArgs args)
        protected void ButtonClick(object sender, RoutedEventArgs args)
        {
            if (sender is Button _btn)
            {
                var _parent = _btn.TemplatedParent as ContentControl;
                if (_parent.Content is int _score)
                {
                    if (((string)_btn.Tag).Equals(@"+"))
                    {
                        _score++;
                    }
                    else
                    {
                        if (_score > 0)
                        {
                            _score--;
                        }
                    }
                    _parent.Content = _score;
                }
                else if (_parent.Content is AbilityBase _ability)
                {
                    if (((string)_btn.Tag).Equals(@"+"))
                    {
                        if (_ability.IsNonAbility)
                        {
                            _ability.IsNonAbility = false;
                            _ability.BaseValue = 1;
                        }
                        else
                        {
                            _ability.BaseValue++;
                        }
                    }
                    else
                    {
                        if (_ability.BaseValue > 1)
                        {
                            _ability.BaseValue--;
                        }
                        else
                        {
                            _ability.IsNonAbility = true;
                        }
                    }
                    _parent.Content = _ability;
                }
            }
        }
        #endregion

        #region private void RollCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        // ExecutedRoutedEventHandler for the custom color command.
        private void RollCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var _rollType = e.Parameter as string;
            lstValues.Items.Clear();
            ccCHA.Content = string.Empty;
            ccCON.Content = string.Empty;
            ccDEX.Content = string.Empty;
            ccINT.Content = string.Empty;
            ccSTR.Content = string.Empty;
            ccWIS.Content = string.Empty;
            var _list = new List<int>();
            switch (_rollType)
            {
                case @"Default":
                    _list.Add(15);
                    _list.Add(14);
                    _list.Add(13);
                    _list.Add(12);
                    _list.Add(10);
                    _list.Add(8);
                    break;

                case @"3d6":
                    for (var _dx = 0; _dx < 6; _dx++)
                    {
                        _list.Add(DiceRoller.RollDice(Guid.Empty, 3, 6, @"Abilities", @"3d6"));
                    }
                    break;

                case @"4d6":
                    for (var _dx = 0; _dx < 6; _dx++)
                    {
                        var _val = 0;
                        var _low = 7;
                        for (var _rx = 0; _rx < 4; _rx++)
                        {
                            var _roll = DiceRoller.RollDie(Guid.Empty, 6, @"Abilities", @"4d6 (best 3)");
                            if (_roll < _low)
                            {
                                _low = _roll;
                            }

                            _val += _roll;
                        }
                        _val -= _low;
                        _list.Add(_val);
                    }
                    break;

                case @"5d6":
                    for (var _dx = 0; _dx < 6; _dx++)
                    {
                        var _val = 0;
                        var _low1 = 7;
                        var _low2 = 7;
                        for (var _rx = 0; _rx < 5; _rx++)
                        {
                            var _roll = DiceRoller.RollDie(Guid.Empty, 6, @"Abilities", @"5d6 (best 3)");
                            if (_roll <= _low1)
                            {
                                _low2 = _low1;
                                _low1 = _roll;
                            }
                            else if (_roll <= _low2)
                            {
                                _low2 = _roll;
                            }
                            _val += _roll;
                        }
                        _val -= (_low1 + _low2);
                        _list.Add(_val);
                    }
                    break;

                case @"Own":
                    ccSTR.Content = new Strength(10);
                    ccDEX.Content = new Dexterity(10);
                    ccCON.Content = new Constitution(10);
                    ccINT.Content = new Intelligence(10);
                    ccWIS.Content = new Wisdom(10);
                    ccCHA.Content = new Charisma(10);
                    return;

                case @"Species":
                    var _speciesItem = cboSpecies.SelectedItem as TypeListItem;
                    var _species = (Species)Activator.CreateInstance(_speciesItem.ListedType);
                    var _abilities = _species.DefaultAbilities();
                    ccSTR.Content = _abilities.Strength;
                    ccDEX.Content = _abilities.Dexterity;
                    ccCON.Content = _abilities.Constitution;
                    ccINT.Content = _abilities.Intelligence;
                    ccWIS.Content = _abilities.Wisdom;
                    ccCHA.Content = _abilities.Charisma;
                    break;
            }

            // sort items into list box
            _list.Sort();
            foreach (var _rollVal in _list)
            {
                var _lbl = new Label
                {
                    Content = _rollVal,
                    Tag = _rollVal,
                    ToolTip = _rollVal
                };
                lstValues.Items.Insert(0, _lbl);
            }
        }
        #endregion

        #region private void RollCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void RollCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter.ToString().Equals(@"Species"))
            {
                if (cboSpecies != null)
                {
                    e.CanExecute = cboSpecies.SelectedItem != null;
                }
            }
            else
            {
                e.CanExecute = true;
            }

            e.Handled = true;
        }
        #endregion

        public Creature GetCreature(string name)
        {
            if (cboSpecies.SelectedItem != null)
            {
                var _str = (ccSTR.Content is Strength)
                    ? ccSTR.Content as Strength
                    : new Strength(Convert.ToInt32(ccSTR.Content.ToString()));

                var _dex = (ccDEX.Content is Dexterity)
                    ? ccDEX.Content as Dexterity
                    : new Dexterity(Convert.ToInt32(ccDEX.Content.ToString()));

                var _con = (ccCON.Content is Constitution)
                    ? ccCON.Content as Constitution
                    : new Constitution(Convert.ToInt32(ccCON.Content.ToString()));

                var _int = (ccINT.Content is Intelligence)
                    ? ccINT.Content as Intelligence
                    : new Intelligence(Convert.ToInt32(ccINT.Content.ToString()));

                var _wis = (ccWIS.Content is Wisdom)
                    ? ccWIS.Content as Wisdom
                    : new Wisdom(Convert.ToInt32(ccWIS.Content.ToString()));

                var _cha = (ccCHA.Content is Charisma)
                    ? ccCHA.Content as Charisma
                    : new Charisma(Convert.ToInt32(ccCHA.Content.ToString()));

                var _abilities = new AbilitySet(_str, _dex, _con, _int, _wis, _cha);
                var _critter = new Creature(name, _abilities);

                var _specItem = (TypeListItem)cboSpecies.SelectedItem;
                var _species = (Species)Activator.CreateInstance(_specItem.ListedType);
                _species.BindTo(_critter);
                _critter.Devotion = new Devotion(@"Nature");

                return _critter;
            }
            return null;
        }

        private void cboSpecies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((cboSpecies?.SelectedIndex ?? -1) >= 0)
            {
                var _speciesItem = cboSpecies.SelectedItem as TypeListItem;
                var _species = (Species)Activator.CreateInstance(_speciesItem.ListedType);
                var _abilities = _species.DefaultAbilities();
                ccSTR.Content = _abilities.Strength;
                ccDEX.Content = _abilities.Dexterity;
                ccCON.Content = _abilities.Constitution;
                ccINT.Content = _abilities.Intelligence;
                ccWIS.Content = _abilities.Wisdom;
                ccCHA.Content = _abilities.Charisma;
            }
        }
    }
}
