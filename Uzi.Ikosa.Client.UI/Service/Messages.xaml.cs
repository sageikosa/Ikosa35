using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Uzi.Ikosa.Contracts.Host;
using Uzi.Ikosa.Proxy.ViewModel;

namespace Uzi.Ikosa.Client.UI
{
    /// <summary>
    /// Interaction logic for Messages.xaml
    /// </summary>
    public partial class Messages : UserControl
    {
        public static RoutedCommand RefreshMessagesCommand = new RoutedCommand();
        public static RoutedCommand ClearMessagesCommand = new RoutedCommand();
        public static RoutedCommand RefreshUsersCommand = new RoutedCommand();
        public static RoutedCommand SendMessageCommand = new RoutedCommand();

        public Messages()
        {
            try { InitializeComponent(); } catch { }
        }

        #region public ProxyModel Proxies { get; set; } (DEPENDENCY)
        public ProxyModel Proxies
        {
            get { return (ProxyModel)GetValue(ProxiesProperty); }
            set { SetValue(ProxiesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Proxies.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProxiesProperty =
            DependencyProperty.Register(@"Proxies", typeof(ProxyModel), typeof(Messages), new PropertyMetadata(null));
        #endregion

        #region public string UserName { get; set; } (DEPENDENCY)
        public string UserName
        {
            get { return (string)GetValue(UserNameProperty); }
            set { SetValue(UserNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UserName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UserNameProperty =
            DependencyProperty.Register(@"UserName", typeof(string), typeof(Messages), new PropertyMetadata(string.Empty, UserName_Changed));

        private static void UserName_Changed(DependencyObject depends, DependencyPropertyChangedEventArgs args)
        {
            var _msgs = depends as Messages;
            if (_msgs != null)
            {
                _msgs.dtsMessageTemplates.UserName = args.NewValue.ToString() ?? string.Empty;
            }
        }
        #endregion

        #region public bool MessagesVisible { get; set; } (DEPENDENCY)
        public bool MessagesVisible
        {
            get { return (bool)GetValue(MessagesVisibleProperty); }
            set { SetValue(MessagesVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MessagesVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MessagesVisibleProperty =
            DependencyProperty.Register(@"MessagesVisible", typeof(bool), typeof(Messages), new PropertyMetadata(false, MessagesVisible_Changed));

        private static void MessagesVisible_Changed(DependencyObject depends, DependencyPropertyChangedEventArgs args)
        {
            var _messages = depends as Messages;
            if (_messages != null)
            {
                _messages.Visibility = (bool)args.NewValue ? Visibility.Visible : Visibility.Hidden;
            }
        }
        #endregion

        private bool IsLoggedIn { get { return ((Proxies != null) && Proxies.IsLoggedIn); } }

        #region private void cbSendMessage_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cbSendMessage_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (IsLoggedIn && (txtMessage != null) && (lstUsers != null))
            {
                if (!string.IsNullOrWhiteSpace(txtMessage.Text) && (lstUsers.SelectedItem != null))
                {
                    e.CanExecute = IsLoggedIn;
                }
            }
            e.Handled = true;
        }
        #endregion

        #region private void RefreshMessages()
        private void RefreshMessages()
        {
            // ensure visible
            // TODO: notify visibility
            //tglMessages.IsChecked = true;

            if (IsLoggedIn)
            {
                try
                {
                    Proxies.NewMessage();
                }
                catch (Exception _ex)
                {
                    Debug.WriteLine(_ex);
                    MessageBox.Show(_ex.Message, @"Ikosa Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        #endregion

        #region private void RefreshUserList()
        private void RefreshUserList()
        {
            if (IsLoggedIn)
            {
                try
                {
                    Proxies.UserListChanged();
                }
                catch (Exception _ex)
                {
                    Debug.WriteLine(_ex);
                    MessageBox.Show(_ex.Message, @"Ikosa Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        #endregion

        #region private void cbSendMessage_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cbSendMessage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var _user = (lstUsers.SelectedItem as UserInfo)?.UserName ?? string.Empty;
                if (_user == @"(All)")
                    _user = string.Empty;
                Proxies.SendMessage(_user, txtMessage.Text);
                RefreshMessages();
                txtMessage.Text = string.Empty;
                txtMessage.Focus();
            }
            catch (Exception _ex)
            {
                Debug.WriteLine(_ex);
                MessageBox.Show(_ex.Message, @"Ikosa Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }
        #endregion

        #region private void cbRefreshUsers_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cbRefreshUsers_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsLoggedIn;
            e.Handled = true;
        }
        #endregion

        #region private void cbRefreshUsers_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cbRefreshUsers_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RefreshUserList();
            e.Handled = true;
        }
        #endregion

        #region private void cbRefreshMessages_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cbRefreshMessages_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsLoggedIn;
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

        #region private void cbClearMessages_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cbClearMessages_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if ((lstMessages != null) && (lstMessages.ItemsSource != null) && (lstMessages.Items.Count > 0))
            {
                e.CanExecute = true;
            }
            e.Handled = true;
        }
        #endregion

        #region private void cbClearMessages_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cbClearMessages_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (IsLoggedIn)
            {
                Proxies.ClearMessages();
                RefreshMessages();
            }
        }
        #endregion
    }
}
