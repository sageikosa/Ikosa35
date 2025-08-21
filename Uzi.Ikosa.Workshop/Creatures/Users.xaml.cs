using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Uzi.Ikosa.Services;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for Users.xaml
    /// </summary>
    public partial class Users : UserControl
    {
        public static RoutedCommand AddUser = new RoutedCommand();
        public static RoutedCommand RemoveUser = new RoutedCommand();
        public static RoutedCommand RemoveAll = new RoutedCommand();

        public Users()
        {
            InitializeComponent();
            DataContextChanged += Users_DataContextChanged;

            // initialize
            _Users = [];
            _Available = [];

            // bind
            lstUsers.ItemsSource = _Users;
            lstAvailable.ItemsSource = _Available;
        }

        #region data
        private ObservableCollection<string> _Users;
        private ObservableCollection<string> _Available;
        #endregion

        private Creature _Critter => DataContext as Creature;

        #region private void Users_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        private void Users_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var _critter = _Critter;
            if (_critter != null)
            {
                // fill lists
                _Users.Clear();
                _Available.Clear();

                // users bound ...
                var _users = _critter.GetUsers().OrderBy(_u => _u).ToList();
                foreach (var _u in _users)
                {
                    _Users.Add(_u);
                }

                // available
                if (UserValidator.UserDefinitions != null)
                {
                    foreach (var _userDef in from _ud in UserValidator.UserDefinitions.GetList()
                                             where !_users.Any(_u => _u.Equals(_ud.UserName, StringComparison.OrdinalIgnoreCase))
                                             orderby _ud.UserName
                                             select _ud)
                    {
                        _Available.Add(_userDef.UserName);
                    }
                }
            }
        }
        #endregion

        #region cbAddUser
        private void cbAddUser_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (lstAvailable?.SelectedItem != null);
            e.Handled = true;
        }

        private void cbAddUser_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _critter = _Critter;
            if (_critter != null)
            {
                // remove from available list
                var _user = lstAvailable.SelectedItem.ToString();
                _Available.Remove(_user);
                if (!_Users.Any(_u => _u.Equals(_user, StringComparison.OrdinalIgnoreCase)))
                {
                    // add to users list
                    _Users.Add(_user);
                    if (!_critter.CanUserControl(_user))
                    {
                        // and effect the control
                        _critter.AddAdjunct(new UserController(_user));
                    }
                }
            }
        }
        #endregion

        #region cbRemoveUser
        private void cbRemoveUser_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (lstUsers?.SelectedItem != null);
            e.Handled = true;
        }

        private void cbRemoveUser_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _critter = _Critter;
            if (_critter != null)
            {
                // remove from creature
                var _user = lstUsers.SelectedItem.ToString();
                _Users.Remove(_user);
                if (_critter.CanUserControl(_user))
                {
                    // effect the control removal
                    _critter.Adjuncts.OfType<UserController>()
                        .FirstOrDefault(_uc => _uc.UserName.Equals(_user, StringComparison.OrdinalIgnoreCase))
                        ?.Eject();
                }

                // add to available
                if (!_Available.Any(_u => _u.Equals(_user, StringComparison.OrdinalIgnoreCase)))
                {
                    _Available.Add(_user);
                }
            }
        }
        #endregion

        #region cbRemoveAll
        private void cbRemoveAll_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = lstUsers?.Items.Count > 0;
            e.Handled = true;
        }

        private void cbRemoveAll_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _critter = _Critter;
            if (_critter != null)
            {
                // clear
                foreach (var _uc in _critter.Adjuncts.OfType<UserController>().ToList())
                {
                    _uc.Eject();
                }

                _Users.Clear();
                _Available.Clear();

                // available
                if (UserValidator.UserDefinitions != null)
                {
                    foreach (var _userDef in from _ud in UserValidator.UserDefinitions.GetList()
                                             orderby _ud.UserName
                                             select _ud)
                    {
                        _Available.Add(_userDef.UserName);
                    }
                }
            }
        }
        #endregion
    }
}
