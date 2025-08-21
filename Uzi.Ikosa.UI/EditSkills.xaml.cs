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
using System.Windows.Controls.Primitives;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.UI
{
    /// <summary>
    /// Interaction logic for EditSkills.xaml
    /// </summary>
    public partial class EditSkills : UserControl
    {
        public EditSkills()
        {
            InitializeComponent();
        }

        public ItemsControl AdvancementLogItemsControl
        {
            get => (ItemsControl)GetValue(AdvancementLogItemsControlProperty);
            set => SetValue(AdvancementLogItemsControlProperty, value);
        }

        // Using a DependencyProperty as the backing store for AdvancementLogItemsControl.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AdvancementLogItemsControlProperty =
            DependencyProperty.Register(@"AdvancementLogItemsControl", typeof(ItemsControl), typeof(EditSkills),
            new UIPropertyMetadata(null));


        #region private void itemsSkills_MouseUp(object sender, MouseButtonEventArgs e)
        private void itemsSkills_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var _pop = (sender as Grid).Tag as Popup;
                _pop.IsOpen = true;
                _pop.Child.Focus();
                e.Handled = true;
            }
        }
        #endregion

        private ItemsControl _SkillGridMenu = null;

        #region private void mnuSkillHeaderClick(object sender, RoutedEventArgs e)
        private void mnuSkillHeaderClick(object sender, RoutedEventArgs e)
        {
            // Add (a skill not already on the skill buy list) or Clear All
            var _item = e.OriginalSource as MenuItem;
            var _sender = sender as FrameworkElement;
            var _pd = _sender.Tag as PowerDie;
            if ((_item.Tag != null) && (_item.Tag.ToString().Equals(@"Clear All", StringComparison.OrdinalIgnoreCase)))
            {
                // clear all
                _pd.RollbackSkillPoints();
                if (_SkillGridMenu != null)
                {
                    _SkillGridMenu.ItemsSource = _pd.SkillsAssigned;
                }
            }
            else
            {
                // add a skill
                if (_item.Tag is SkillMenuItem _skill)
                {
                    _pd.AssignSkillPoints(_skill.SkillBase, _pd.CurrentSkillPointAssignment(_skill.SkillBase) + 1);
                    if (_SkillGridMenu != null)
                    {
                        _SkillGridMenu.ItemsSource = _pd.SkillsAssigned;
                    }
                }
            }
            e.Handled = true;
        }
        #endregion

        private void popSkillEditor_Closed(object sender, EventArgs e)
        {
            if (AdvancementLogItemsControl != null)
            {
                AdvancementLogItemsControl.Items.Refresh();
            }
        }

        #region private void mnuSkillHeaderOpening(object sender, ContextMenuEventArgs e)
        private void mnuSkillHeaderOpening(object sender, ContextMenuEventArgs e)
        {
            var _row = sender as GridViewHeaderRowPresenter;
            _SkillGridMenu = (_row.Parent as HeaderedContentControl).Content as ItemsControl;
            var _pd = _row.Tag as PowerDie;
            var _mnuAdd = _row.ContextMenu.Items[0] as MenuItem;

            _mnuAdd.ItemsSource = from _skill in _pd.Creature.Skills
                                  select new SkillMenuItem
                                  {
                                      SkillBase = _skill,
                                      IsClassSkill = _pd.AdvancementClass.IsClassSkill(_skill)
                                  };
        }
        #endregion

        #region private void mnuSkillGridOpening(object sender, ContextMenuEventArgs e)
        private void mnuSkillGridOpening(object sender, ContextMenuEventArgs e)
        {
            var _grid = sender as Grid;
            _SkillGridMenu = null;
            ContextMenu _ctx = _grid.ContextMenu;
            PowerDie _pd = (_grid.DataContext as AdvancementLogItem).PowerDie;
            var _mnuAdd = _ctx.Items[0] as MenuItem;
            _mnuAdd.ItemsSource = from _skill in _pd.Creature.Skills
                                  select new SkillMenuItem
                                  {
                                      SkillBase = _skill,
                                      IsClassSkill = _pd.AdvancementClass.IsClassSkill(_skill)
                                  };
        }
        #endregion

        #region private void btnSkillClick(object sender, RoutedEventArgs e)
        private void btnSkillClick(object sender, RoutedEventArgs e)
        {
            // one of the +, - buttons was clicked on a skill row
            var _row = sender as GridViewRowPresenter;
            var _skill = _row.Tag as SkillBuy;
            var _btn = e.OriginalSource as Button;
            var _cmd = _btn.Tag.ToString();
            switch (_cmd)
            {
                case @"+":
                    _skill.PowerDie.AssignSkillPoints(_skill.Skill, _skill.PointsUsed + 1);
                    break;

                case @"-":
                    _skill.PowerDie.AssignSkillPoints(_skill.Skill, _skill.PointsUsed - 1);
                    break;
            }

            // find items control for GridViewRow presenter
            DependencyObject _dep = VisualTreeHelper.GetParent(_row);
            while ((_dep != null) && !(_dep is ItemsControl))
            {
                _dep = VisualTreeHelper.GetParent(_dep);
            }
            if (_dep != null)
            {
                var _items = _dep as ItemsControl;
                _items.ItemsSource = null;
                _items.ItemsSource = _skill.PowerDie.SkillsAssigned;
            }
        }
        #endregion
    }
}
