using System;
using System.Collections.Generic;
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
using Uzi.Core.Dice;
using Uzi.Ikosa.UI;
using System.Collections.ObjectModel;
using System.Collections;
using Uzi.Ikosa.TypeListers;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Creatures.Types;
using System.ComponentModel;
using Uzi.Ikosa.Advancement.CharacterClasses;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for AbilityScores.xaml
    /// </summary>

    public partial class CreateCharacter : Window
    {
        Point _DownPoint = new Point();
        bool _Dragging = false;
        bool _ClickIn = false;
        DragLabelAdorner _Overlay = null;

        #region Construction
        public CreateCharacter()
        {
            InitializeComponent();
            lstValues.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(lstValues_PreviewMouseLeftButtonDown);
            lstValues.PreviewMouseMove += new MouseEventHandler(lstValues_PreviewMouseMove);
            PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this_PreviewMouseLeftButtonUp);
            PreviewKeyDown += new KeyEventHandler(this_PreviewKeyDown);
            PreviewMouseMove += new MouseEventHandler(this_PreviewMouseMove);
            cboSpecies.ItemsSource = SpeciesLister.AllSpecies();
            // TODO: indicate allowed to drop?
        }
        #endregion

        private Creature _CreatedCreature;
        public Creature CreatedCreature { get { return _CreatedCreature; } }

        #region void this_PreviewMouseMove(object sender, MouseEventArgs e)
        void this_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_Dragging)
            {
                DragMoved();
            }
        }
        #endregion

        #region void this_PreviewKeyDown(object sender, KeyEventArgs e)
        void this_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Escape) && _Dragging)
            {
                DragFinished(true);
            }
        }
        #endregion

        #region void this_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        void this_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_Dragging)
            {
                DragFinished(false);
                e.Handled = true;
            }
            _ClickIn = false;
        }
        #endregion

        #region void lstValues_PreviewMouseMove(object sender, MouseEventArgs e)
        void lstValues_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if ((e.LeftButton == MouseButtonState.Pressed) && _ClickIn && !_Dragging)
            {
                Point _pt = e.GetPosition(lstValues);
                if ((Math.Abs(_pt.X - _DownPoint.X) > SystemParameters.MinimumHorizontalDragDistance) ||
                    (Math.Abs(_pt.Y - _DownPoint.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    // start drag
                    DragStarted();
                    e.Handled = true;
                }
            }
        }
        #endregion

        #region void lstValues_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        void lstValues_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            _DownPoint = e.GetPosition(lstValues);
            _ClickIn = true;
        }
        #endregion

        public static RoutedCommand RollCmd = new RoutedCommand();

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
            var _list = new ArrayList();
            switch (_rollType)
            {
                case "Default":
                    _list.Add(15);
                    _list.Add(14);
                    _list.Add(13);
                    _list.Add(12);
                    _list.Add(10);
                    _list.Add(8);
                    break;

                case "3d6":
                    for (var _dx = 0; _dx < 6; _dx++)
                    {
                        _list.Add(DiceRoller.RollDice(Guid.Empty, 3, 6, @"Abilities", @"3d6"));
                    }
                    break;

                case "4d6":
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

                case "5d6":
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

                case "Own":
                    ccSTR.Content = "10";
                    ccDEX.Content = "10";
                    ccCON.Content = "10";
                    ccINT.Content = "10";
                    ccWIS.Content = "10";
                    ccCHA.Content = "10";
                    return;
            }

            // sort items into list box
            _list.Sort();
            foreach (int _rollVal in _list)
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

        // CanExecuteRoutedEventHandler for the custom color command.
        private void RollCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        #region protected void ButtonClick(object sender, RoutedEventArgs args)
        protected void ButtonClick(object sender, RoutedEventArgs args)
        {
            if (sender is Button _btn)
            {
                var _parent = _btn.TemplatedParent as ContentControl;
                if ((_parent.Content is string _content) && !_content.Equals(string.Empty))
                {
                    var _val = Convert.ToInt32(_content);
                    if (((string)_btn.Tag).Equals("+"))
                    {
                        _val++;
                    }
                    else
                    {
                        if (_val > 3)
                        {
                            _val--;
                        }
                    }
                    _parent.Content = _val.ToString();
                }
            }
        }
        #endregion

        #region Dragging Ability Scores
        private void DragStarted()
        {
            _Dragging = true;
            Mouse.Capture(this, CaptureMode.SubTree);
            _Overlay = new DragLabelAdorner(this, ((Label)lstValues.SelectedItem).Content.ToString());
            AdornerLayer.GetAdornerLayer(this).Add(_Overlay);
            DragMoved();
        }

        private void DragMoved()
        {
            Point _curr = Mouse.GetPosition(this);
            _Overlay.Left = _curr.X;
            _Overlay.Top = _curr.Y + 16;
        }

        private void DragFinished(bool cancel)
        {
            Mouse.Capture(null);
            if (_Dragging)
            {
                AdornerLayer.GetAdornerLayer(this).Remove(_Overlay);
                if (!cancel)
                {
                    object _prev = null;
                    if (ccSTR.IsMouseOver)
                    {
                        _prev = ccSTR.Content;
                        ccSTR.Content = ((Label)lstValues.SelectedItem).Tag;
                        lstValues.Items.Remove(lstValues.SelectedItem);
                    }
                    else if (ccDEX.IsMouseOver)
                    {
                        _prev = ccDEX.Content;
                        ccDEX.Content = ((Label)lstValues.SelectedItem).Tag;
                        lstValues.Items.Remove(lstValues.SelectedItem);
                    }
                    else if (ccCON.IsMouseOver)
                    {
                        _prev = ccCON.Content;
                        ccCON.Content = ((Label)lstValues.SelectedItem).Tag;
                        lstValues.Items.Remove(lstValues.SelectedItem);
                    }
                    else if (ccINT.IsMouseOver)
                    {
                        _prev = ccINT.Content;
                        ccINT.Content = ((Label)lstValues.SelectedItem).Tag;
                        lstValues.Items.Remove(lstValues.SelectedItem);
                    }
                    else if (ccWIS.IsMouseOver)
                    {
                        _prev = ccWIS.Content;
                        ccWIS.Content = ((Label)lstValues.SelectedItem).Tag;
                        lstValues.Items.Remove(lstValues.SelectedItem);
                    }
                    else if (ccCHA.IsMouseOver)
                    {
                        _prev = ccCHA.Content;
                        ccCHA.Content = ((Label)lstValues.SelectedItem).Tag;
                        lstValues.Items.Remove(lstValues.SelectedItem);
                    }
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
            _Dragging = false;
            _ClickIn = false;
        }
        #endregion

        #region create by roll
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (cboSpecies.SelectedItem != null)
            {
                var _str = new Strength(Convert.ToInt32(ccSTR.Content.ToString()));
                var _dex = new Dexterity(Convert.ToInt32(ccDEX.Content.ToString()));
                var _con = new Constitution(Convert.ToInt32(ccCON.Content.ToString()));
                var _int = new Intelligence(Convert.ToInt32(ccINT.Content.ToString()));
                var _wis = new Wisdom(Convert.ToInt32(ccWIS.Content.ToString()));
                var _cha = new Charisma(Convert.ToInt32(ccCHA.Content.ToString()));
                var _abilities = new AbilitySet(_str, _dex, _con, _int, _wis, _cha);
                _CreatedCreature = new Creature("New Guy", _abilities);

                var _specItem = (TypeListItem)cboSpecies.SelectedItem;
                var _species = (Species)Activator.CreateInstance(_specItem.ListedType);
                _species.BindTo(_CreatedCreature);

                // TODO: apply selected templates
            }
        }
        #endregion

        #region create by default scores
        private void Default_Click(object sender, RoutedEventArgs e)
        {
            if (cboSpecies.SelectedItem != null)
            {
                // default construction
                var _specItem = (TypeListItem)cboSpecies.SelectedItem;
                var _species = (Species)Activator.CreateInstance(_specItem.ListedType);

                // create creature of a specific species
                _CreatedCreature = new Creature("New Guy", _species.DefaultAbilities());
                _species.BindTo(_CreatedCreature);

                // TODO: apply selected templates
            }
        }
        #endregion
    }
}