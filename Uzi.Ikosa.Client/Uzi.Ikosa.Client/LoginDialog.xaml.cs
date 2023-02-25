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
using System.ServiceModel;
using Uzi.Ikosa.Proxy;
using Uzi.Ikosa.Proxy.ViewModel;
using Uzi.Ikosa.Contracts.Host;
using Uzi.Visualize;
using System.Windows.Threading;
using System.Diagnostics;

namespace Uzi.Ikosa.Client
{
    /// <summary>
    /// Interaction logic for LoginDialog.xaml
    /// </summary>
    public partial class LoginDialog : Window
    {
        // NOTE: added root cert, signing cert for service

        // NOTE: also, added root cert to trusted people to MMC Certificates...
        // NOTE: ...and changed client endpoint service certificate behavior to use peerOrChainTrust

        public static RoutedCommand NextCommand = new RoutedCommand();
        public static RoutedCommand LoginCommand = new RoutedCommand();

        private ProxyModel _Proxies = null;
        private readonly ResourceDictionary _Resources = null;
        private readonly Action<ActorModel> _GenerateView;
        private readonly Action<IsMasterModel> _GenerateLog;
        private readonly Action<Action> _Dispatcher;

        public LoginDialog(ResourceDictionary resources, Action<ActorModel> generateView, Action<IsMasterModel> generateLog, Action<Action> actionDispatcher)
        {
            InitializeComponent();
            _Resources = resources;
            _Dispatcher = actionDispatcher;
            txtUserName.Focus();
            _GenerateView = generateView;
            _GenerateLog = generateLog;
        }

        #region private void cbNext_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cbNext_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if ((txtUserName != null) && (pbPassword != null))
                e.CanExecute = !string.IsNullOrWhiteSpace(txtUserName.Text)
                    && !string.IsNullOrWhiteSpace(txtHost.Text)
                    && !string.IsNullOrWhiteSpace(txtPort.Text)
                    && !string.IsNullOrWhiteSpace(pbPassword.Password);
        }
        #endregion

        #region private void cbNext_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cbNext_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                // get current users
                var _uName = txtUserName.Text;
                _Proxies = new ProxyModel(_uName, pbPassword.Password, txtHost.Text, txtPort.Text, _GenerateView, _Dispatcher, _GenerateLog);
                var _users = _Proxies.Users;
                var _myUser = _users.FirstOrDefault(_u => _u.UserName.Equals(_uName, StringComparison.OrdinalIgnoreCase));
                if (_myUser != null)
                {
                    if (_myUser.CreatureInfos.Any())
                    {
                        // login again to get refreshed proxy
                        _Proxies.Logout();
                        foreach (var _myCritter in _myUser.CreatureInfos)
                        {
                            _Proxies.LoginCreature(_myCritter,
                                (found) => ActorView.StartActorView(new ActorModel(_Proxies, found, true)),
                                (master) => MasterLogView.StartMasterLog(master));
                        }

                        DialogResult = true;
                        Close();
                    }
                }
                else
                {
                    // get available
                    var _critters = _Proxies.GetAvailableCreatures();
                    lstCritters.ItemsSource = _critters;
                    lstCritters.Focus();

                    // if only one creature, login automatically (no further actions)
                    if (_critters.Count() == 1)
                    {
                        _Proxies.LoginCreature(_critters.First(),
                            (found) => ActorView.StartActorView(new ActorModel(_Proxies, found, true)),
                            (master) => MasterLogView.StartMasterLog(master));
                        DialogResult = true;
                        Close();
                    }
                }
            }
            catch (Exception _ex)
            {
                Debug.WriteLine(_ex);
                MessageBox.Show(_ex.Message, @"Ikosa Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                _Proxies = null;
            }
        }
        #endregion

        #region private void cbLogin_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cbLogin_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (lstCritters != null)
                e.CanExecute = lstCritters.SelectedItem != null;
            e.Handled = true;
        }
        #endregion

        #region private void cbLogin_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cbLogin_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (lstCritters.SelectedItem is CreatureLoginInfo _critter)
                {
                    _Proxies.LoginCreature(_critter,
                        (found) => ActorView.StartActorView(new ActorModel(_Proxies, found, true)),
                        (master) => MasterLogView.StartMasterLog(master));
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception _ex)
            {
                Debug.WriteLine(_ex);
                MessageBox.Show(_ex.Message, @"Ikosa Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                _Proxies = null;
            }
        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // dialog return properties
        public ProxyModel ProxyModel { get { return _Proxies; } }
    }
}
