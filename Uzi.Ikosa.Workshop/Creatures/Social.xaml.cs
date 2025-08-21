using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for Social.xaml
    /// </summary>
    public partial class Social : UserControl
    {
        public static RoutedCommand NewTeam = new();
        public static RoutedCommand AddTeam = new();
        public static RoutedCommand RemoveTeam = new();
        public static RoutedCommand RemoveAll = new();

        public Social()
        {
            InitializeComponent();
            DataContextChanged += Users_DataContextChanged;
        }

        public PresentableCreatureVM PresentableCreature => DataContext as PresentableCreatureVM;
        public Creature Creature => PresentableCreature.Thing;

        private void Users_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
            => ResyncLists();

        private void ResyncLists()
        {
            // TODO: get available teams from module...(and directly references modules)
            lstAvailable.ItemsSource = Creature?.GetAvailableTeams().ToList();
            lstTeams.ItemsSource = Creature?.Adjuncts.OfType<TeamMember>().ToList();
        }

        #region cbNewTeam
        private void cbNewTeam_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cbNewTeam_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // new Team
            var _dlg = new NewTeam()
            {
                Owner = Window.GetWindow(this)
            };
            if ((_dlg.ShowDialog() ?? false) && !string.IsNullOrWhiteSpace(_dlg.TeamName))
            {
                TeamMember.SetTeamMember(Creature, _dlg.TeamName);
                ResyncLists();
            }
            e.Handled = true;
        }
        #endregion

        #region cbAddTeam
        private void cbAddTeam_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = lstAvailable?.SelectedItem != null;
            e.Handled = true;
        }

        private void cbAddTeam_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _team = lstAvailable.SelectedItem as TeamGroup;
            TeamMember.SetTeamMember(Creature, _team.Name);
            ResyncLists();
            e.Handled = true;
        }
        #endregion

        #region cbRemoveTeam
        private void cbRemoveTeam_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = lstTeams?.SelectedItem != null;
            e.Handled = true;
        }

        private void cbRemoveTeam_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            (lstTeams.SelectedItem as TeamMember)?.Eject();
            ResyncLists();
        }
        #endregion

        #region cbRemoveAll
        private void cbRemoveAll_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = lstTeams?.Items.Count > 0;
            e.Handled = true;
        }

        private void cbRemoveAll_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (var _team in Creature?.Adjuncts.OfType<TeamMember>().ToList())
            {
                _team.Eject();
            }

            ResyncLists();
        }
        #endregion

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ResyncLists();
        }
    }
}
