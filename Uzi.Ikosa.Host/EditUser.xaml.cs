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
    /// Interaction logic for EditUser.xaml
    /// </summary>
    public partial class EditUser : Window
    {
        public static RoutedCommand EditUserOK = new RoutedCommand();

        public EditUser(UserDefinition userDef, UserDefinitionCollection allUsers)
        {
            InitializeComponent();
            _UserDef = userDef;
            _AllUsers = allUsers;
            txtUserName.Text = _UserDef.UserName;
            pwOne.Password = _UserDef.Password;
            pwTwo.Password = _UserDef.Password;
            chkMaster.IsChecked = _UserDef.IsMasterUser;
            chkDisabled.IsChecked = _UserDef.IsDisabled;
        }

        private UserDefinition _UserDef = null;
        private UserDefinitionCollection _AllUsers;

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void cbEditUserOK_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if ((txtUserName != null) && (pwOne != null) && (pwTwo != null))
            {
                // must be unique (non-empty) userName
                if (!string.IsNullOrEmpty(txtUserName.Text)
                    && !_AllUsers.DoesOtherUserExist(txtUserName.Text, _UserDef))
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
            DialogResult = true;
            _UserDef.UserName = txtUserName.Text;
            _UserDef.Password = pwOne.Password;
            _UserDef.IsMasterUser = chkMaster.IsChecked ?? false;
            _UserDef.IsDisabled = chkDisabled.IsChecked ?? false;
            Close();
        }
    }
}
