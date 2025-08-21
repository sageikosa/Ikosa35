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
using Uzi.Ikosa.Abilities;

namespace Uzi.Ikosa.UI
{
    /// <summary>
    /// Interaction logic for EditAbility.xaml
    /// </summary>
    public partial class EditAbility : UserControl
    {
        public EditAbility()
        {
            InitializeComponent();
        }

        private void txtAbility_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var _pd = (this.DataContext as AdvancementLogItem).PowerDie;
            if (_pd.IsAbilityBoostPowerDie)
            {
                ContextMenu _ctx = new ContextMenu
                {
                    ItemsSource = _pd.Creature.Abilities.AllAbilities,
                    Tag = _pd
                };
                _ctx.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(mnuAbility_Click));
                _ctx.IsOpen = true;
                e.Handled = true;
            }
        }

        #region private void mnuAbility_Click(object sender, RoutedEventArgs e)
        private void mnuAbility_Click(object sender, RoutedEventArgs e)
        {
            var _pd = (sender as ContextMenu).Tag as PowerDie;
            var _ability = (e.OriginalSource as MenuItem).Header as AbilityBase;
            if (_pd.IsAbilityBoostPowerDie && !_pd.IsLocked)
            {
                _pd.AbilityBoost = _ability;
            }
        }
        #endregion
    }
}
