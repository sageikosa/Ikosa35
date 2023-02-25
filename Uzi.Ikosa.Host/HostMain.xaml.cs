using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.IO.Packaging;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Services;
using System.ComponentModel;
using System.ServiceModel;
using System.Collections.ObjectModel;
using Uzi.Core;
using System.Threading;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Host.Prerequisites;
using System.Deployment.Application;
using Uzi.Packaging;
using Uzi.Visualize.Packaging;
using Uzi.Core.Packaging;
using System.Diagnostics;
using Uzi.Ikosa.Contracts.Host;
using Uzi.Ikosa.Contracts;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using System.Windows.Threading;
using Uzi.Core.Dice;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Host
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class HostMain : Window, ILoginCallback
    {
        // NOTE: added root cert, signing cert for service

        // NOTE: also, added root cert to trusted people to MMC Certificates...
        // NOTE: ...and changed client endpoint (service certificate) behavior to use peerOrChainTrust

        public static RoutedCommand OpenAndGo = new RoutedCommand();

        // user management
        public static RoutedCommand AddUser = new RoutedCommand();
        public static RoutedCommand DeleteUser = new RoutedCommand();
        public static RoutedCommand EditUser = new RoutedCommand();
        public static RoutedCommand LogoutUser = new RoutedCommand();

        // service
        public static RoutedCommand StartService = new RoutedCommand();
        public static RoutedCommand StopService = new RoutedCommand();

        // messaging on user tab
        public static RoutedCommand SendMessage = new RoutedCommand();
        public static RoutedCommand ClearMessages = new RoutedCommand();

        // tracker
        public static RoutedCommand StartTracker = new RoutedCommand();
        public static RoutedCommand StopTracker = new RoutedCommand();

        // prerequisite
        public static RoutedCommand EditPrerequisite = new RoutedCommand();

        #region construction
        public HostMain()
        {
            InitializeComponent();
            BasePartFactory.RegisterFactory();
            VisualizeBasePartFactory.RegisterFactory();
            CoreBasePartFactory.RegisterFactory();
            IkosaBasePartFactory.RegisterFactory();
        }
        #endregion

        #region host checkbox changes
        void _LoginHost_Closed(object sender, EventArgs e)
        {
            chkLoginService.IsChecked = false;
        }

        void _LoginHost_Opened(object sender, EventArgs e)
        {
            chkLoginService.IsChecked = true;
        }

        void _IkosaHost_Closed(object sender, EventArgs e)
        {
            chkIkosaService.IsChecked = false;
        }

        void _IkosaHost_Opened(object sender, EventArgs e)
        {
            chkIkosaService.IsChecked = true;
        }

        void _ViewerHost_Closed(object sender, EventArgs e)
        {
            chkViewService.IsChecked = false;
        }

        void _ViewerHost_Opened(object sender, EventArgs e)
        {
            chkViewService.IsChecked = true;
        }

        void _MasterHost_Closed(object sender, EventArgs e)
        {
            chkMasterService.IsChecked = false;
        }

        void _MasterHost_Opened(object sender, EventArgs e)
        {
            chkMasterService.IsChecked = true;
        }
        #endregion

        #region data
        private CorePackage _Package = null;
        private ServiceHost _LoginHost = null;
        private ServiceHost _IkosaHost = null;
        private ServiceHost _ViewerHost = null;
        private ServiceHost _MasterHost = null;
        #endregion

        public LocalMap Map { get { return gridHost.DataContext as LocalMap; } }

        #region cmdOpen
        private void cmdOpen_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _Package == null;
            e.Handled = true;
        }

        private void cmdOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // use a common dialog
            var _opener = new System.Windows.Forms.OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = @"Ikosa Files (*.Ikosa)|*.Ikosa",
                Title = @"Open File...",
                ValidateNames = true
            };
            if (!Debugger.IsAttached)
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    _opener.InitialDirectory = ApplicationDeployment.CurrentDeployment.DataDirectory;
                }
            }
            System.Windows.Forms.DialogResult _rslt = System.Windows.Forms.DialogResult.Cancel;
            try { _rslt = _opener.ShowDialog(); }
            catch (Win32Exception) { }

            // process results
            if (_rslt == System.Windows.Forms.DialogResult.OK)
            {
                var _fName = _opener.FileName;
                var _pathParts = _fName.Split('\\');
                var _path = string.Concat(string.Join("\\", _pathParts, 0, _pathParts.Length - 1), "\\");
                var _dots = _fName.Split('.');

                switch (_dots[_dots.Length - 1].ToLower())
                {
                    case @"ikosa":
                        // if *.Ikosa, assume its an ikosa file
                        try
                        {
                            LoadStatus.StartLoadStatusWindow();
                            var _fInfo = new FileInfo(_fName);
                            _Package = new CorePackage(_fInfo, Package.Open(_fName, FileMode.Open, FileAccess.ReadWrite, FileShare.None));
                        }
                        catch (Exception _except)
                        {
                            MessageBox.Show(_except.Message, @"Host Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        finally
                        {
                            LoadStatus.StopLoadStatusWindow();
                        }

                        SetMap();
                        break;
                }
                e.Handled = true;
            }
        }

        private void SetMap()
        {
            // user synchronizer continuance
            ReaderWriterLockSlim _userLock = null;
            if (UserValidator.UserDefinitions != null)
            {
                _userLock = UserValidator.UserDefinitions.Synchronizer;
            }

            // map synchronizer continuance
            ReaderWriterLockSlim _mapLock = null;
            var _oldMap = Map;
            if (_oldMap != null)
            {
                // map synchronizer continuance
                _mapLock = _oldMap.Synchronizer;

                //_oldMap.ContextSet.ProcessManager.CurrentCoreProcess
                //    -= new EventHandler<ProcessEventArgs>(ProcessManager_CurrentCoreProcess);
                _oldMap.ContextSet.ProcessManager.CurrentCoreStep
                    -= new EventHandler<StepEventArgs>(ProcessManager_CurrentCoreStep);
                LoginService.MapContext = null;
                MasterServices.MapContext = null;
            }

            // load package-level map
            gridHost.DataContext = _Package.Relationships.OfType<LocalMap>().FirstOrDefault();

            // load package-level user definitions
            var _userDefinitions = _Package.Relationships.OfType<UserDefinitionsPart>().FirstOrDefault();
            if (_userDefinitions == null)
            {
                _userDefinitions = new UserDefinitionsPart(_Package, @"Users");
                _Package.Add(_userDefinitions);
            }
            gridUsers.DataContext = _userDefinitions;

            // set as UserValidator users
            if (UserValidator.UserDefinitions == null)
                UserValidator.UserDefinitions = _userDefinitions.UserDefinitions;

            // user synchronizer continuance
            UserValidator.UserDefinitions.Synchronizer = _userLock ?? new ReaderWriterLockSlim();

            ctrlChat.RefreshUserList();

            // load map stored data
            var _newMap = Map;
            if (_newMap != null)
            {
                // map synchronizer continuance
                _newMap.Synchronizer = _mapLock ?? new ReaderWriterLockSlim();

                IkosaServices.MapContext = _newMap.MapContext;

                // visualization services (NOTE: map has Synchronizer member, so don't need to set separately)
                VisualizationService.Map = _newMap;
                //mapXLocal.LoadMap(_newMap);

                // action services
                IkosaServices.InteractProvider = _newMap;
                IkosaServices.ProcessManager = _newMap.IkosaProcessManager;

                // process management
                stackProcesses.DataContext = _newMap.ContextSet.ProcessManager;
                gridBudgets.DataContext = _newMap.ContextSet.ProcessManager;
                //_newMap.ContextSet.ProcessManager.CurrentCoreProcess
                //    += new EventHandler<ProcessEventArgs>(ProcessManager_CurrentCoreProcess);
                _newMap.ContextSet.ProcessManager.CurrentCoreStep
                    += new EventHandler<StepEventArgs>(ProcessManager_CurrentCoreStep);
                LoginService.MapContext = _newMap.MapContext;
                MasterServices.MapContext = _newMap.MapContext;
            }
        }

        void ProcessManager_CurrentCoreStep(object sender, StepEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (Action)(() =>
                {
                    RefreshProcessManager();
                }));
        }

        void ProcessManager_CurrentCoreProcess(object sender, ProcessEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (Action)(() =>
                {
                    RefreshProcessManager();
                }));
        }
        #endregion

        #region private void cmdOpenAndGo_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cmdOpenAndGo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // open
            cmdOpen_Executed(sender, e);

            try
            {
                Map?.Synchronizer.EnterWriteLock();
                if (e.Handled)
                {
                    // start time tracker
                    var _ikosaManager = Map.IkosaProcessManager;
                    if (StartTracker.CanExecute(null, grdHost))
                    {
                        var _critters = Map.MapContext.CreatureLoginsInfos
                            .Select(_cli => Map.GetCreature(_cli.ID));
                        new LocalTurnTracker(_critters, Map.ContextSet, false);
                        _ikosaManager.DoProcessAll();
                    }

                    // start host
                    if (StartService.CanExecute(null, grdHost))
                    {
                        StartService.Execute(null, grdHost);
                    }
                }
            }
            finally
            {
                if (Map?.Synchronizer.IsWriteLockHeld ?? false)
                    Map?.Synchronizer.ExitWriteLock();
            }
        }
        #endregion

        #region private void RefreshProcessManager()
        private void RefreshProcessManager()
        {
            try
            {
                Map.Synchronizer.EnterReadLock();
                stackProcesses.DataContext = null;
                gridBudgets.DataContext = null;
                stackProcesses.DataContext = Map.ContextSet.ProcessManager;
                gridBudgets.DataContext = Map.ContextSet.ProcessManager;
            }
            finally
            {
                if (Map.Synchronizer.IsReadLockHeld)
                    Map.Synchronizer.ExitReadLock();
            }
        }
        #endregion

        #region cmdSave
        private void cmdSave_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _Package != null;
            e.Handled = true;
        }

        private void cmdSave_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (UserValidator.UserDefinitions.Synchronizer.TryEnterWriteLock(5000))
            {
                try
                {
                    LoadStatus.StartLoadStatusWindow();
                    Map.Synchronizer.EnterWriteLock();
                    _Package.Save();
                }
                catch (Exception _except)
                {
                    BasePartHelper.LoadMessage?.Invoke(@"Saving...");
                    MessageBox.Show(_except.Message, @"Host Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    if (Map.Synchronizer.IsWriteLockHeld)
                        Map.Synchronizer.ExitWriteLock();
                    LoadStatus.StopLoadStatusWindow();
                    if (UserValidator.UserDefinitions.Synchronizer.IsWriteLockHeld)
                        UserValidator.UserDefinitions.Synchronizer.ExitWriteLock();
                }
            }
            else
            {
                MessageBox.Show(@"Unable to lock user collection after 5 seconds", @"Ikosa Host", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }
        #endregion

        #region cmdSaveAs
        private void cmdSaveAs_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _Package != null;
            e.Handled = true;
        }

        private void cmdSaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (UserValidator.UserDefinitions.Synchronizer.TryEnterWriteLock(5000))
            {
                try
                {
                    Map.Synchronizer.EnterWriteLock();
                    var _saver = new System.Windows.Forms.SaveFileDialog()
                    {
                        Filter = @"Ikosa Package Files|*.ikosa",
                        DefaultExt = @"ikosa"
                    };
                    if (_saver.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        _Package.SaveAs(_saver.FileName);
                    }
                    SetMap();
                }
                finally
                {
                    if (Map.Synchronizer.IsWriteLockHeld)
                        Map.Synchronizer.ExitWriteLock();
                    if (UserValidator.UserDefinitions.Synchronizer.IsWriteLockHeld)
                        UserValidator.UserDefinitions.Synchronizer.ExitWriteLock();
                }
            }
            else
            {
                MessageBox.Show(@"Unable to lock user collection after 5 seconds", @"Ikosa Host", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }
        #endregion

        #region cbAddUser
        private void cbAddUser_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (_Package != null) && (gridUsers != null) && (gridUsers.DataContext != null);
            e.Handled = true;
        }

        private void cbAddUser_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _add = new AddUser(UserValidator.UserDefinitions);
            if (_add.ShowDialog() ?? false)
            {
                lstUsers.Items.Refresh();
            }
        }
        #endregion

        #region cdDeleteUser
        private void cbDeleteUser_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (_Package != null) && (lstUsers != null) && (lstUsers.SelectedItem != null);
            e.Handled = true;
        }

        private void cbDeleteUser_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _userDef = lstUsers.SelectedItem as UserDefinition;
            UserValidator.UserDefinitions.RemoveLocked(_userDef);
            lstUsers.Items.Refresh();
        }
        #endregion

        #region cbEditUser
        private void cbEditUser_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (_Package != null) && (lstUsers != null) && (lstUsers.SelectedItem != null);
            e.Handled = true;
        }

        private void cbEditUser_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _edit = new EditUser(lstUsers.SelectedItem as UserDefinition, UserValidator.UserDefinitions);
            if (_edit.ShowDialog() ?? false)
            {
            }
        }
        #endregion

        #region private void lstUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        private void lstUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstMessageLog != null)
            {
                if (lstUsers.SelectedItem != null)
                {
                    var _userDef = lstUsers.SelectedItem as UserDefinition;
                    lstMessageLog.ItemsSource = LoginService.GetUserMessages(_userDef.UserName);
                    var _userInfo = LoginService.GetUserInfo(_userDef.UserName);
                    if (_userInfo != null)
                    {
                        // TODO: items control
                        contCreatureInfo.Content = _userInfo.CreatureInfos.FirstOrDefault();
                    }
                    else
                    {
                        contCreatureInfo.Content = null;
                    }
                }
                else
                {
                    lstMessageLog.ItemsSource = null;
                }
            }
        }
        #endregion

        #region cdSendMessage
        private void cdSendMessage_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if ((txtMessage != null) && !string.IsNullOrEmpty(txtMessage.Text))
            {
                if ((lstUsers != null) && (lstUsers.SelectedItem != null))
                {
                    e.CanExecute = true;
                }
            }
            e.Handled = true;
        }

        private void cdSendMessage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            LoginService.SendUserMessage((lstUsers.SelectedItem as UserDefinition).UserName, txtMessage.Text);
            lstMessageLog.ItemsSource = LoginService.GetUserMessages((lstUsers.SelectedItem as UserDefinition).UserName);
        }
        #endregion

        #region cbClearMessages
        private void cbClearMessages_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (lstUsers != null) && (lstUsers.SelectedItem != null);
            e.Handled = true;
        }

        private void cbClearMessages_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _userName = (lstUsers.SelectedItem as UserDefinition).UserName;
            LoginService.ClearArchivedMessages(_userName, _userName);
            lstMessageLog.ItemsSource = LoginService.GetUserMessages((lstUsers.SelectedItem as UserDefinition).UserName);
        }
        #endregion

        #region cbStartService
        private void cbStartService_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (_LoginHost == null) && (_Package != null);
            e.Handled = true;
        }

        private void cbStartService_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // console message hooks
            void _doConsoleMessage(ConsoleMessage msg)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (Action)(() =>
                    {
                        lstConsoleLog.Items.Add(msg);
                    }));
            }
            LoginService.Console = _doConsoleMessage;
            IkosaServices.Console = _doConsoleMessage;

            Roller.RollLog = (title, description, principalID, rollerLog, targets) =>
            {
                IkosaServices.QueueNotifySysStatus(
                    new RollNotify(principalID, new Description(title, description), rollerLog),
                    targets.ToArray());

            };

            Deltable.CreateDeltaCalcNotify = (principalID, name) =>
            {
                var _notify = new DeltaCalcNotify(principalID, name);
                IkosaServices.QueueNotifySysStatus(_notify, new[] { Guid.Empty });
                return _notify;
            };

            Deltable.CreateCheckNotify = (principalID, title, opposedID, opposedTitle) =>
            {
                var _notify = new CheckNotify(principalID, title, opposedID, opposedTitle);
                IkosaServices.QueueNotifySysStatus(_notify, new[] { Guid.Empty });
                return _notify;
            };

            VisualizationService.Console = _doConsoleMessage;
            MasterServices.Console = _doConsoleMessage;
            LoginService.HostCallback = this;
            MasterServices.OnFlowStateChanged = LoginService.DoFlowStateChanged;
            MasterServices.OnPauseChanged = LoginService.DoPauseChanged;
            // MasterServices.OnShutdown = // save and shutdown
            LoginService.OnGetFlowState = MasterServices.GetFlowState;
            LoginService.OnGetPauseState = MasterServices.GetPauseState;

            // login host
            _LoginHost = new ServiceHost(new LoginService());
            _LoginHost.Opened += _LoginHost_Opened;
            _LoginHost.Closed += _LoginHost_Closed;
            _LoginHost.Open();

            // ikosa info host
            _IkosaHost = new ServiceHost(new IkosaServices());
            _IkosaHost.Opened += _IkosaHost_Opened;
            _IkosaHost.Closed += _IkosaHost_Closed;
            _IkosaHost.Open();

            // viewer info host
            _ViewerHost = new ServiceHost(new VisualizationService());
            _ViewerHost.Opened += _ViewerHost_Opened;
            _ViewerHost.Closed += _ViewerHost_Closed;
            _ViewerHost.Open();

            // master host
            _MasterHost = new ServiceHost(new MasterServices());
            _MasterHost.Opened += _MasterHost_Opened;
            _MasterHost.Closed += _MasterHost_Closed;
            _MasterHost.Open();

            // handled
            e.Handled = true;
        }
        #endregion

        #region cbStopService
        private void cbStopService_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (_LoginHost != null)
            {
                e.CanExecute = _LoginHost.State == CommunicationState.Opened
                    || _LoginHost.State == CommunicationState.Faulted;
            }
            e.Handled = true;
        }

        private void cbStopService_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // login host
            _LoginHost.Close();
            _LoginHost.Opened -= _LoginHost_Opened;
            _LoginHost.Closed -= _LoginHost_Closed;
            _LoginHost = null;

            // ikosa info host
            _IkosaHost.Close();
            _IkosaHost.Opened -= _IkosaHost_Opened;
            _IkosaHost.Closed -= _IkosaHost_Closed;
            _IkosaHost = null;

            // view service host
            _ViewerHost.Close();
            _ViewerHost.Opened -= _ViewerHost_Opened;
            _ViewerHost.Closed -= _ViewerHost_Closed;
            _ViewerHost = null;

            // master service host
            _MasterHost.Close();
            _MasterHost.Opened -= _MasterHost_Opened;
            _MasterHost.Closed -= _MasterHost_Closed;
            _MasterHost = null;

            // handled
            e.Handled = true;
        }
        #endregion

        #region cbStartTracker
        private void cbStartTracker_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Map != null)
            {
                var _ikosaManager = Map.IkosaProcessManager;
                if (_ikosaManager != null)
                {
                    // must have no tracker
                    e.CanExecute = _ikosaManager.LocalTurnTracker == null;
                }
            }
            e.Handled = true;
        }

        private void cbStartTracker_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                Map.Synchronizer.EnterWriteLock();

                // NOTE: both are implemented within LocalTurnTracker class
                var _ikosaManager = Map.IkosaProcessManager;
                if (_ikosaManager != null)
                {
                    // fix portraits
                    var _infos = Map.MapContext.CreatureLoginsInfos;
                    foreach (var _cli in _infos.Where(_i => _i.Portrait == null))
                    {
                        var _port = IkosaServices.CreatureProvider?.GetCreature(_cli.ID)?.GetPortrait(Map.Resources);
                        if (_port != null)
                            _cli.Portrait = new Visualize.Contracts.BitmapImageInfo(_port);
                    }

                    // show dialog
                    var _dlg = new TrackerActors(_infos)
                    {
                        Owner = this,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    if (_dlg.ShowDialog() ?? false)
                    {
                        var _critters = _dlg.GetCreatures().Select(_cli => Map.GetCreature(_cli.ID));
                        new LocalTurnTracker(_critters, Map.ContextSet, false);
                        _ikosaManager.DoProcessAll();
                    }
                }
            }
            finally
            {
                if (Map.Synchronizer.IsWriteLockHeld)
                    Map.Synchronizer.ExitWriteLock();
            }
            e.Handled = true;
        }
        #endregion

        #region cbStopTracker
        private void cbStopTracker_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Map != null)
            {
                var _ikosaManager = Map.IkosaProcessManager;
                if (_ikosaManager != null)
                {
                    // current must be a time tracker
                    e.CanExecute = (_ikosaManager.LocalTurnTracker != null) && !_ikosaManager.LocalTurnTracker.IsInitiative;
                }
            }
            e.Handled = true;
        }

        private void cbStopTracker_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                Map.Synchronizer.EnterWriteLock();
                var _ikosaManager = Map.IkosaProcessManager;
                if (_ikosaManager != null)
                {
                    var _localTracker = _ikosaManager.LocalTurnTracker;
                    if (_localTracker != null)
                    {
                        _ikosaManager.PopTracker();
                        _ikosaManager.DoProcessAll();
                    }
                }

            }
            finally
            {
                if (Map.Synchronizer.IsWriteLockHeld)
                    Map.Synchronizer.ExitWriteLock();
            }
            e.Handled = true;
        }
        #endregion

        #region cmdRefresh
        private void cmdRefresh_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Map != null;
            e.Handled = true;
        }

        private void cmdRefresh_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ProcessRefresh();
        }
        #endregion

        #region private void ProcessRefresh()
        private void ProcessRefresh()
        {
            try
            {
                Map.Synchronizer.EnterWriteLock();
                Map.ContextSet.ProcessManager.DoProcessAll();
            }
            finally
            {
                if (Map.Synchronizer.IsWriteLockHeld)
                    Map.Synchronizer.ExitWriteLock();
                RefreshProcessManager();
            }
        }
        #endregion

        #region private void ProcessRefresh(Action action)
        /// <summary>Map.Synchronizer.EnterWriteLock, action, DoProcess, FlushNotify</summary>
        private void ProcessRefresh(Action action)
        {
            try
            {
                Map.Synchronizer.EnterWriteLock();
                action();
                Map.ContextSet.ProcessManager.DoProcessAll();
            }
            finally
            {
                if (Map.Synchronizer.IsWriteLockHeld)
                    Map.Synchronizer.ExitWriteLock();
                RefreshProcessManager();
            }
        }
        #endregion

        #region ILoginCallback Members

        public void NewMessage()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (Action)(() =>
                {
                    ctrlChat.RefreshMessages();
                }));
        }

        public void UserListChanged()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (Action)(() =>
                {
                    ctrlChat.RefreshUserList();
                }));
        }

        public void FlowStateChanged()
        {
            // TODO: host has little to do on this?
        }

        public void PauseChanged(bool isPaused)
        {
            // TODO: host has little to do on this?
        }

        public void UserLogout(string userName)
        {
            try
            {
                IkosaServices.DeRegisterCallback(userName);
                VisualizationService.DeRegisterCallback(userName);
            }
            catch
            {
            }
        }

        #endregion

        #region cbEditPrerequisite
        private void cbEditPrerequisite_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cbEditPrerequisite_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _preReq = e.Parameter;
            if (_preReq is SavePrerequisite)
            {
                var _dlg = new SavePrerequisiteDialog(_preReq as SavePrerequisite);
                if (_dlg.ShowDialog() ?? false)
                {
                    ProcessRefresh(() => _dlg.SavePrerequisite.SaveRoll = new Core.Deltable(_dlg.Value() ?? 0));
                }
            }
            else if (_preReq is SuccessCheckPrerequisite)
            {
                var _dlg = new CheckPrerequisiteDialog(_preReq as SuccessCheckPrerequisite);
                if (_dlg.ShowDialog() ?? false)
                {
                    ProcessRefresh(() =>
                    {
                        _dlg.SuccessCheckPrerequisite.IsUsingPenalty = _dlg.IsUsingPenalty;
                        _dlg.SuccessCheckPrerequisite.CheckRoll = new Core.Deltable(_dlg.Value() ?? 0);
                    });
                }
            }
            else if (_preReq is ChoicePrerequisite)
            {
                var _dlg = new ChoicePrerequisiteDialog(_preReq as ChoicePrerequisite);
                if (_dlg.ShowDialog() ?? false)
                {
                    ProcessRefresh(() => _dlg.ChoicePrerequisite.Selected = _dlg.SelectedOption);
                }
            }
            else if (_preReq is RollPrerequisite)
            {
                var _dlg = new RollPrerequisiteDialog(_preReq as RollPrerequisite);
                if (_dlg.ShowDialog() ?? false)
                {
                    ProcessRefresh(() => _dlg.RollPrerequisite.RollValue = _dlg.RollValue);
                }
            }
        }
        #endregion

        #region cbLogoutuser
        private void cbLogoutuser_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (_Package != null) && (lstUsers != null) && (lstUsers.SelectedItem != null);
            e.Handled = true;
        }

        private void cbLogoutuser_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _user = lstUsers.SelectedItem as UserDefinition;
            LoginService.Logout(_user.UserName);
            lstUsers.Items.Refresh();
            contCreatureInfo.Content = null;
        }
        #endregion

        private void miAbout_Click(object sender, RoutedEventArgs e)
        {
            var _about = new AboutHost(this);
            _about.ShowDialog();
        }

        #region window layout persistence
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                //Save layout
                var serializer = new XmlLayoutSerializer(dmHost);
                using (var stream = new StreamWriter(@"IkosaHost.Layout"))
                    serializer.Serialize(stream);            //Restore the layout
            }
            catch
            {
            }
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            try
            {
                var serializer = new XmlLayoutSerializer(dmHost);
                using (var stream = new StreamReader(@"IkosaHost.Layout"))
                    serializer.Deserialize(stream);
            }
            catch
            {
            }
        }
        #endregion

        private void btnLogClear_Click(object sender, RoutedEventArgs e)
        {
            lstConsoleLog.Items.Clear();
        }
    }

    public class MessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate OutboundTemplate { get; set; }
        public DataTemplate OutboundPublicTemplate { get; set; }
        public DataTemplate InboundTemplate { get; set; }
        public DataTemplate InboundPublicTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is UserMessage _msg)
            {
                if (_msg.FromUser.Equals(@"(Host)", StringComparison.OrdinalIgnoreCase))
                {
                    return _msg.IsPublic ? OutboundPublicTemplate : OutboundTemplate;
                }
                else
                    return _msg.IsPublic ? InboundPublicTemplate : InboundTemplate;
            }
            return null;
        }
    }
}
