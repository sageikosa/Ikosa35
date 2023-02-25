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
using System.Collections.ObjectModel;
using Uzi.Ikosa.Contracts.Host;

namespace Uzi.Ikosa.Host
{
    /// <summary>
    /// Interaction logic for Chat.xaml
    /// </summary>
    public partial class Chat : UserControl
    {
        // messaging on chat tab
        public static RoutedCommand RefreshMessagesCommand = new RoutedCommand();
        public static RoutedCommand ClearMessagesCommand = new RoutedCommand();
        public static RoutedCommand RefreshUsersCommand = new RoutedCommand();
        public static RoutedCommand SendMessageCommand = new RoutedCommand();

        public Chat()
        {
            InitializeComponent();
            _Messages = new Collection<UserMessage>();
        }

        #region private void cbRefreshMessages_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cbRefreshMessages_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }
        #endregion

        #region private void cbRefreshMessages_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cbRefreshMessages_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RefreshMessages();
            e.Handled = true;
        }
        #endregion

        #region public void RefreshUserList()
        public void RefreshUserList()
        {
            try
            {
                var _userList = LoginService.GetLoggedInUserList();

                // add all and host
                _userList.Insert(0, new UserInfo { UserName = string.Empty });

                // remove self
                cboUsers.ItemsSource = _userList;
                cboUsers.SelectedIndex = 0;
            }
            catch (Exception _ex)
            {
                MessageBox.Show(_ex.Message, @"Ikosa Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region private void cbRefreshUsers_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cbRefreshUsers_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }
        #endregion

        private void cbRefreshUsers_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RefreshUserList();
            e.Handled = true;
        }

        #region private void cbSendMessage_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cbSendMessage_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if ((txtChatMessage != null) && (cboUsers != null))
            {
                if (!string.IsNullOrWhiteSpace(txtChatMessage.Text) && (cboUsers.SelectedItem != null))
                {
                    e.CanExecute = true;
                }
            }
            e.Handled = true;
        }
        #endregion

        #region private void cbSendMessage_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cbSendMessage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var _user = (cboUsers.SelectedItem as UserInfo);
                LoginService.SendUserMessage(_user.UserName, txtChatMessage.Text);
                _Messages.Add(new UserMessage
                {
                    ToUser = _user.UserName,
                    FromUser = @"(Host)",
                    Created = DateTime.Now,
                    IsPublic = string.IsNullOrWhiteSpace(_user.UserName),
                    Message = txtChatMessage.Text
                });
                RefreshMessages();
                txtChatMessage.Text = string.Empty;
                txtChatMessage.Focus();
            }
            catch (Exception _ex)
            {
                MessageBox.Show(_ex.Message, @"Ikosa Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }
        #endregion

        #region private void cbClearChatMessages_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cbClearChatMessages_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if ((lstMessages != null) && (lstMessages.Items != null) && (lstMessages.Items.Count > 0))
            {
                e.CanExecute = true;
            }
            e.Handled = true;
        }
        #endregion

        private Collection<UserMessage> _Messages;

        #region public void RefreshMessages()
        public void RefreshMessages()
        {
            try
            {
                var _scroll = (lstMessages.SelectedItem == null)
                    || (lstMessages.SelectedIndex == (lstMessages.Items.Count - 1));

                // add any new messages (for host)
                foreach (var _msg in LoginService.GetMessages(@"(Host)"))
                {
                    _Messages.Add(_msg);
                }

                // show in list
                lstMessages.ItemsSource = _Messages.OrderBy(_um => _um.Created).ToList();
                if (_scroll && (lstMessages.Items.Count > 0))
                {
                    lstMessages.ScrollIntoView(lstMessages.Items[lstMessages.Items.Count - 1]);
                    lstMessages.SelectedItem = null;
                }
            }
            catch (Exception _ex)
            {
                MessageBox.Show(_ex.Message, @"Ikosa Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region private void cbClearChatMessages_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cbClearChatMessages_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _Messages.Clear();
            RefreshMessages();
        }
        #endregion
    }
}
