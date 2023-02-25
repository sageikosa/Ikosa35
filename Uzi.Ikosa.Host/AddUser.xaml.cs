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
using Uzi.Ikosa.Services;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Host
{
    /// <summary>
    /// Interaction logic for AddUser.xaml
    /// </summary>
    public partial class AddUser : Window
    {
        public AddUser(UserDefinitionCollection userDefs)
        {
            _Users = userDefs;
            InitializeComponent();
        }

        private UserDefinitionCollection _Users;

        private void cbEditUserOK_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if ((txtUserName != null) && (pwOne != null) && (pwTwo != null))
            {
                // must be unique (non-empty) userName
                if (!string.IsNullOrEmpty(txtUserName.Text)
                    && !_Users.DoesUserExist(txtUserName.Text))
                {
                    // must have a non-empty password and both password fields must match
                    if (!string.IsNullOrEmpty(pwOne.Password)
                        && pwOne.Password.Equals(pwTwo.Password))
                    {
                        e.CanExecute = true;
                    }
                }
            }
            e.Handled = true;
        }

        private void cbEditUserOK_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _Users.AddLocked(new UserDefinition
            {
                UserName = txtUserName.Text,
                Password = pwOne.Password
            });
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
