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
using Uzi.Ikosa.Services;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.UI
{
    /// <summary>
    /// Interaction logic for EditUsers.xaml
    /// </summary>
    public partial class EditUsers : UserControl
    {
        public static RoutedCommand AddUser = new RoutedCommand();
        public static RoutedCommand DeleteUser = new RoutedCommand();
        public static RoutedCommand EditUser = new RoutedCommand();
        
        public EditUsers()
        {
            InitializeComponent();
        }

        private UserDefinitionCollection _UserDefinitions
        {
            get
            {
                return lstUsers.ItemsSource as UserDefinitionCollection;
            }
        }

        #region cbAddUser
        private void cbAddUser_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (DataContext != null) && (_UserDefinitions != null);
            e.Handled = true;
        }

        private void cbAddUser_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _add = new AddUser(_UserDefinitions);
            if (_add.ShowDialog() ?? false)
            {
                lstUsers.Items.Refresh();
            }
        }
        #endregion

        #region cdDeleteUser
        private void cbDeleteUser_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (lstUsers != null) && (lstUsers.SelectedItem != null);
            e.Handled = true;
        }

        private void cbDeleteUser_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _userDef = lstUsers.SelectedItem as UserDefinition;
            _UserDefinitions.Remove(_userDef);
            lstUsers.Items.Refresh();
        }
        #endregion

        #region private void cbEditUser_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cbEditUser_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (lstUsers != null) && (lstUsers.SelectedItem != null);
            e.Handled = true;
        }
        #endregion

        #region private void cbEditUser_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cbEditUser_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _edit = new EditUser(lstUsers.SelectedItem as UserDefinition, _UserDefinitions);
            if (_edit.ShowDialog() ?? false)
            {
            }
        }
        #endregion
    }
}
