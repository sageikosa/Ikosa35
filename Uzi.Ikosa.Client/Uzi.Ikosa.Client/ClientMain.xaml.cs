using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Client.UI;
using Uzi.Ikosa.Proxy.ViewModel;

namespace Uzi.Ikosa.Client
{
    /// <summary>
    /// Interaction logic for ClientMain.xaml
    /// </summary>
    public partial class ClientMain : Window
    {
        // login/logout
        public static RoutedCommand LoginCommand = new RoutedCommand();
        public static RoutedCommand LogoutCommand = new RoutedCommand();
        public static RoutedCommand ExitCommand = new RoutedCommand();
        public static RoutedCommand AboutCommand = new RoutedCommand();
        public static RoutedCommand ShowToolWindow = new RoutedCommand();

        public ClientMain()
        {
            InitializeComponent();
        }

        #region private data
        private ProxyModel _Proxies;
        #endregion

        private IEnumerable<ActorView> GetActorWindows()
        {
            return Application.Current.Windows.OfType<ActorView>();
        }

        // TODO: master prerquisites

        private bool IsLoggedIn
            => _Proxies?.IsLoggedIn ?? false;

        #region private void ShowException(Exception exception)
        private void ShowException(Exception exception)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                Debug.WriteLine(exception);
                MessageBox.Show(exception.Message, @"Ikosa Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }));
        }
        #endregion

        #region private void cbLogin_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cbLogin_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!IsLoggedIn)
            {
                e.CanExecute = e.Parameter.ToString().Equals(@"All");
            }
            else
            {
                e.CanExecute = e.Parameter.ToString().Equals(@"Extra");
            }
            e.Handled = true;
        }
        #endregion

        #region private void cbLogin_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cbLogin_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter.ToString().Equals(@"All"))
            {
                var _dlg = new LoginDialog(Resources,
                    (actor) => ActorView.StartActorView(actor),
                    (master) => MasterLogView.StartMasterLog(master),
                    (action) => Dispatcher?.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, action))
                {
                    Owner = this
                };
                if (_dlg.ShowDialog() ?? false)
                {
                    // get credentials
                    _Proxies = _dlg.ProxyModel;
                    DataContext = _Proxies;
                    _Proxies.OnLogOut = () => { _Proxies = null; DataContext = null; };
                    _Proxies.AddAnimatingLog = (status) => AddAniLog(status);
                    _Proxies.ObserveException = (except) => ShowException(except);
                    _Proxies.NeedsAttention =
                        () => Dispatcher.BeginInvoke((Action)(() =>
                        {
                            Activate();
                        }));
                }
            }
            else
            {
                var _dlg = new AddActor(Resources, _Proxies)
                {
                    Owner = this
                };
                if (_dlg.ShowDialog() ?? false)
                {
                    // get credentials
                    //_Proxies.AddAnimatingLog = (status) => AddAniLog(status);
                }
            }
        }
        #endregion

        #region private void cbLogout_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cbLogout_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (IsLoggedIn)
            {
                e.CanExecute = true;
            }
            e.Handled = true;
        }
        #endregion

        #region private void cbLogout_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cbLogout_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (IsLoggedIn)
                {
                    if (MessageBox.Show(@"Logout", @"Ikosa", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No)
                        != MessageBoxResult.Yes)
                        return;
                }

                foreach (var _actor in _Proxies.Actors)
                {
                    _actor.ObservableActor?.DoShutdown();
                    _actor.ClearActions();
                }

                _Proxies.Logout();
            }
            catch (Exception _ex)
            {
                Debug.WriteLine(_ex);
                MessageBox.Show(_ex.Message, @"Ikosa Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;

        }
        #endregion

        #region cbAbout
        private void cbAbout_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cbAbout_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _about = new AboutClient(this);
            _about.ShowDialog();
        }
        #endregion

        #region actors menu
        private void mnuActors_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is MenuItem _menuItem)
            {
                (_menuItem.Header as ActorModel)?.ShowObservableActor();
            }
        }
        #endregion

        #region private void mnuDropActor_Click(object sender, RoutedEventArgs e)
        private void mnuDropActor_Click(object sender, RoutedEventArgs e)
        {
            ((e.OriginalSource as MenuItem)?.Header as ActorModel)?.LogOut.Execute(null);
            e.Handled = true;
        }
        #endregion

        #region private void AddAniLog(SysNotify notify)
        private void AddAniLog(SysNotify notify)
        {
            if (lstAniLog != null)
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    var _count = lstAniLog.Children.Count;
                    var _item = new ContentControl
                    {
                        Content = notify
                    };

                    // events
                    _item.IsVisibleChanged += new DependencyPropertyChangedEventHandler(AniLog_IsVisibleChanged);
                    _item.MouseDown += new MouseButtonEventHandler(AniLog_MouseDown);

                    // animation
                    var _ani = new ObjectAnimationUsingKeyFrames();
                    _ani.KeyFrames.Add(new DiscreteObjectKeyFrame(Visibility.Visible, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.0))));
                    _ani.KeyFrames.Add(new DiscreteObjectKeyFrame(Visibility.Collapsed, KeyTime.FromTimeSpan(TimeSpan.FromSeconds((double)_count + 4))));
                    _ani.Duration = TimeSpan.FromSeconds((double)_count + 5);
                    _item.BeginAnimation(UIElement.VisibilityProperty, _ani);

                    // add to stack panel
                    lstAniLog.Children.Add(_item);
                }));
            }
        }
        #endregion

        #region void AniLog_MouseDown(object sender, MouseButtonEventArgs e)
        void AniLog_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ContentControl _item)
            {
                // remove item and drop handlers
                lstAniLog.Children.Remove(_item);
                _item.MouseDown -= new MouseButtonEventHandler(AniLog_MouseDown);
                _item.IsVisibleChanged -= new DependencyPropertyChangedEventHandler(AniLog_IsVisibleChanged);
            }
            e.Handled = true;
        }
        #endregion

        #region void AniLog_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        void AniLog_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                if (sender is ContentControl _item)
                {
                    // remove item and drop handlers
                    lstAniLog.Children.Remove(_item);
                    _item.MouseDown -= new MouseButtonEventHandler(AniLog_MouseDown);
                    _item.IsVisibleChanged -= new DependencyPropertyChangedEventHandler(AniLog_IsVisibleChanged);
                }
            }
        }
        #endregion

        #region cbExit
        private void cbExit_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cbExit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (IsLoggedIn)
            {
                // shutdown all actor views
                foreach (var _actor in _Proxies?.Actors)
                {
                    _actor.ObservableActor?.DoShutdown();
                    _actor.ClearActions();
                }

                _Proxies?.Logout();
                _Proxies = null;
            }
            Close();
            Application.Current.Shutdown();
            e.Handled = true;
        }
        #endregion

        private void lstActors_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            (lstActors.SelectedItem as ActorModel)?.ShowObservableActor();
            e.Handled = true;
        }
    }
}
