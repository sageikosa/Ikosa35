using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Uzi.Ikosa.Adjuncts;
using Uzi.Core.Dice;
using Uzi.Ikosa.Contracts.Host;
using Uzi.Core.Contracts.Faults;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Core;

namespace Uzi.Ikosa.Services
{
    // NOTE: If you change the class name "LoginService" here, you must also update the reference to "LoginService" in App.config.
    [ServiceBehavior(
        ConcurrencyMode = ConcurrencyMode.Multiple,
        InstanceContextMode = InstanceContextMode.Single,
        UseSynchronizationContext = false
        )]
    public class LoginService : ILoginService
    {
        #region statics
        private static UserMessageCollection _UnreadMessages = new UserMessageCollection();
        private static UserMessageCollection _ArchivedMessages = new UserMessageCollection();

        private static ReaderWriterLockSlim _TrackedLock = new ReaderWriterLockSlim();
        private static Collection<UserInfo> _TrackedUsers = new Collection<UserInfo>();
        #endregion

        public static Action<ConsoleMessage> Console { get; set; }
        public static ILoginCallback HostCallback { get; set; }
        public static MapContext MapContext { get; set; }
        public static Func<FlowState> OnGetFlowState { get; set; }
        public static Func<bool> OnGetPauseState { get; set; }

        #region public static bool HasUser(Guid id)
        public static bool HasUser(Guid id)
        {
            try
            {
                _TrackedLock.EnterReadLock();
                return _TrackedUsers.Any(_liu => _liu.CreatureInfos.Any(_ci => _ci.ID == id));
            }
            finally
            {
                if (_TrackedLock.IsReadLockHeld)
                    _TrackedLock.ExitReadLock();
            }
        }
        #endregion

        #region public static UserInfo GetUserInfo(string userName)
        public static UserInfo GetUserInfo(string userName)
        {
            try
            {
                _TrackedLock.EnterReadLock();
                return _TrackedUsers.FirstOrDefault(_u => _u.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
            }
            finally
            {
                if (_TrackedLock.IsReadLockHeld)
                    _TrackedLock.ExitReadLock();
            }
        }
        #endregion

        #region public static void Logout(string userName)
        public static void Logout(string userName)
        {
            try
            {
                // user must be logged in
                _TrackedLock.EnterUpgradeableReadLock();
                var _login = _TrackedUsers.FirstOrDefault(_lu => _lu.UserName.Equals(userName));
                if (_login != null)
                {
                    try
                    {
                        // dissociate
                        _TrackedLock.EnterWriteLock();
                        _TrackedUsers.Remove(_login);
                        DoUserLogout(userName);
                    }
                    finally
                    {
                        if (_TrackedLock.IsWriteLockHeld)
                            _TrackedLock.ExitWriteLock();
                    }
                }
            }
            catch (Exception _ex)
            {
                ConsoleLog(@"Logout Failure", userName, _ex.Message);
            }
            finally
            {
                if (_TrackedLock.IsUpgradeableReadLockHeld)
                    _TrackedLock.ExitUpgradeableReadLock();
            }

            // NOTE: userName shouldn't be in the list...
            DoNotifyUserList(userName);
        }
        #endregion

        #region public static IList<UserMessage> GetUserMessages(string userName)
        public static IList<UserMessage> GetUserMessages(string userName)
        {
            var _msg = _UnreadMessages.GetToUser(userName).Union(_UnreadMessages.GetFromUser(userName));
            _msg = _msg.Union(_ArchivedMessages.GetToUser(userName).Union(_ArchivedMessages.GetFromUser(userName)));
            return _msg.OrderByDescending(_m => _m.Created).ToList();
        }
        #endregion

        #region public static void SendUserMessage(string userName, string message)
        public static void SendUserMessage(string userName, string message)
        {
            if (!string.IsNullOrEmpty(userName))
            {
                DoSendMessage(userName, message, @"(Host)", false);
            }
            else
            {
                foreach (var _user in GetLoggedInUserList())
                {
                    DoSendMessage(_user.UserName, message, @"(Host)", true);
                }
            }
        }
        #endregion

        #region public static void ClearArchivedMessages(string fromUser, string toUser)
        public static void ClearArchivedMessages(string fromUser, string toUser)
        {
            IList<UserMessage> _msgs = new List<UserMessage>();
            if (!string.IsNullOrEmpty(fromUser))
                _msgs = _msgs.Union(_ArchivedMessages.GetFromUser(fromUser)).ToList();
            if (!string.IsNullOrEmpty(toUser))
                _msgs = _msgs.Union(_ArchivedMessages.GetToUser(toUser)).ToList();
            _ArchivedMessages.Remove(_msgs);
        }
        #endregion

        #region public static List<UserMessage> GetMessages(string userName)
        public static List<UserMessage> GetMessages(string userName)
        {
            var _list = _UnreadMessages.GetToUser(userName);
            foreach (var _userMsg in _list)
            {
                _userMsg.Delivered = DateTime.Now;
            }
            _ArchivedMessages.Add(_list);
            _UnreadMessages.Remove(_list);
            return _list;
        }
        #endregion

        #region public static List<UserInfo> GetLoggedInUserList()
        public static List<UserInfo> GetLoggedInUserList(bool readLock = true)
        {
            try
            {
                if (readLock)
                    _TrackedLock.EnterReadLock();
                return _TrackedUsers.ToList();
            }
            finally
            {
                if (readLock)
                    _TrackedLock.ExitReadLock();
            }
        }
        #endregion

        #region public static void DoFlowStateChanged(FlowState flowState)
        public static void DoFlowStateChanged(FlowState flowState)
        {
            // notify
            HostCallback?.FlowStateChanged();

            foreach (var _user in GetLoggedInUserList())
            {
                // each remaining host (not the excluded one) needs to be made aware
                var _u = _user;
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        if (_u.Notifier != null)
                        {
                            _u.Notifier.FlowStateChanged();
                        }
                    }
                    catch (Exception _ex)
                    {
                        _u.Notifier = null;
                        ConsoleLog(@"ClientModeChanged Notify Failure", _u.UserName, _ex.Message);
                    }
                });
            }
        }
        #endregion

        #region public static void DoPauseChanged(bool isPaused)
        public static void DoPauseChanged(bool isPaused)
        {
            // notify
            HostCallback?.PauseChanged(isPaused);

            foreach (var _user in GetLoggedInUserList())
            {
                // each remaining host (not the excluded one) needs to be made aware
                var _u = _user;
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        if (_u.Notifier != null)
                        {
                            _u.Notifier.PauseChanged(isPaused);
                        }
                    }
                    catch (Exception _ex)
                    {
                        _u.Notifier = null;
                        ConsoleLog(@"ClientModeChanged Notify Failure", _u.UserName, _ex.Message);
                    }
                });
            }
        }
        #endregion

        #region private static void DoUserLogout(string userName)
        /// <summary>assumes _TrackedLock already held in write mode</summary>
        private static void DoUserLogout(string userName)
        {
            // notify
            HostCallback?.UserLogout(userName);

            foreach (var _user in GetLoggedInUserList(false))
            {
                // each remaining host (not the excluded one) needs to be made aware
                var _u = _user;
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        if (_u.Notifier != null)
                        {
                            _u.Notifier.UserLogout(userName);
                        }
                    }
                    catch (Exception _ex)
                    {
                        _u.Notifier = null;
                        ConsoleLog(@"UserLogout Notify Failure", _u.UserName, _ex.Message);
                    }
                });
            }
        }
        #endregion

        #region private static void DoNotifyUserList()
        private static void DoNotifyUserList(params string[] exclude)
        {
            // notify
            HostCallback?.UserListChanged();

            foreach (var _user in GetLoggedInUserList()
                .Where(_u => !exclude.Any(_x => _x.Equals(_u.UserName, StringComparison.OrdinalIgnoreCase))))
            {
                // each remaining host (not the excluded one) needs to be made aware
                var _u = _user;
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        if (_u.Notifier != null)
                        {
                            _u.Notifier.UserListChanged();
                        }
                    }
                    catch (Exception _ex)
                    {
                        _u.Notifier = null;
                        ConsoleLog(@"UserList Notify Failure", _u.UserName, _ex.Message);
                    }
                });
            }
        }
        #endregion

        #region private static void DoSendMessage(string toUser, string message, string sender, bool isPublic)
        private static void DoSendMessage(string toUser, string message, string sender, bool isPublic)
        {
            // add message to collection
            _UnreadMessages.Add(new UserMessage
            {
                ToUser = toUser,
                Created = DateTime.Now,
                Delivered = null,
                FromUser = sender,
                Message = message,
                IsPublic = isPublic
            });

            // find user
            var _user = GetLoggedInUserList()
                .FirstOrDefault(_u => _u.UserName.Equals(toUser, StringComparison.OrdinalIgnoreCase));
            if (_user?.Notifier != null)
            {
                // logged-in, needs to be notified
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        _user.Notifier.NewMessage();
                    }
                    catch (Exception _except)
                    {
                        _user.Notifier = null;
                        ConsoleLog(@"Send Message Failure", _user.UserName, _except.Message);
                    }
                });
            }
            else if (toUser.Equals(@"(Host)", StringComparison.OrdinalIgnoreCase))
            {
                // host is short-circuited
                HostCallback?.NewMessage();
            }
        }
        #endregion

        #region private static void ConsoleLog(string title, string userName, string details)
        private static void ConsoleLog(string title, string userName, string details)
            => Console?.Invoke(new ConsoleMessage
            {
                Title = title,
                Message = $@"User: '{userName}' errored out.",
                Details = details,
                Source = typeof(LoginService)
            });
        #endregion

        #region private void EnsureSession(string userName, Guid sessionID)
        private void EnsureSession(string userName)
        {
            EnsureSession(GetUserInfo(userName));
        }
        #endregion

        #region private void EnsureSession(UserInfo userInfo)
        private void EnsureSession(UserInfo userInfo)
        {
            // user?
            if (userInfo != null)
            {
                // missing a notifier?
                var _callback = OperationContext.Current.GetCallbackChannel<ILoginCallback>();
                if (_callback == null)
                    throw new FaultException<InvalidStateFault>(new InvalidStateFault(), @"No callback defined");

                if (userInfo.Notifier != _callback)
                {
                    try
                    {
                        _TrackedLock.EnterWriteLock();
                        userInfo.Notifier = _callback;
                    }
                    finally
                    {
                        _TrackedLock.ExitWriteLock();
                    }
                }
            }
        }
        #endregion

        #region ILoginService Members

        #region public void SendMessage(string user, string message)
        public void SendMessage(string user, string message)
        {
            var _fromName = Thread.CurrentPrincipal?.Identity.Name ?? @"???";
            var _user = GetUserInfo(user);
            EnsureSession(_fromName);
            if (user?.Equals(@"(Host)", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                DoSendMessage(user, message, _fromName, false);
            }
            else if (string.IsNullOrEmpty(user))
            {
                // send to everyone except the sender
                foreach (var _sendUser in GetLoggedInUserList()
                    .Where(_u => !_u.UserName.Equals(_fromName, StringComparison.OrdinalIgnoreCase)))
                {
                    DoSendMessage(_sendUser.UserName, message, _fromName, true);
                }
                DoSendMessage(@"(Host)", message, _fromName, true);
            }
        }
        #endregion

        #region public List<UserMessage> GetMessages()
        public List<UserMessage> GetMessages()
        {
            var _principal = Thread.CurrentPrincipal;
            var _userName = _principal != null ? _principal.Identity.Name : @"???";
            EnsureSession(_userName);
            return GetMessages(_userName);
        }
        #endregion

        #region public List<CreatureLoginInfo> GetAvailableCreatures()
        public List<CreatureLoginInfo> GetAvailableCreatures()
        {
            var _critterList = MapContext.CreatureLoginsInfos
                .OrderBy(_ci => _ci.Name)
                .ToList();

            var _user = Thread.CurrentPrincipal.Identity.Name;
            if (!Thread.CurrentPrincipal.IsInRole(@"Master"))
            {
                // only master user(s) can access non-player creatures
                var _provider = MapContext?.Map;
                if (_provider != null)
                {
                    try
                    {
                        // synchronize
                        _provider.Synchronizer.EnterReadLock();

                        // user controller
                        _critterList = _critterList
                            .Where(_ci => _provider.GetCreature(_ci.ID)?.CanUserControl(_user) ?? false)
                            .ToList();
                    }
                    finally
                    {
                        // desynchronize
                        if (_provider.Synchronizer.IsReadLockHeld)
                            _provider.Synchronizer.ExitReadLock();
                    }
                }
                else
                {
                    // no list available
                    _critterList.Clear();
                }
            }

            // exclude creatures already logged in (to someone else)...
            var _users = GetUserList(false)
                .Where(_u => !_u.UserName.Equals(_user, StringComparison.OrdinalIgnoreCase))
                .ToList();
            _critterList = _critterList
                .Where(_ci => !_users.Any(_u => _u.CreatureInfos.Any(_ici => _ici.ID == _ci.ID)))
                .ToList();

            return _critterList;
        }
        #endregion

        #region public List<CreatureLoginInfo> GetAllCreatures()
        public List<CreatureLoginInfo> GetAllCreatures()
        {
            var _critterList = MapContext.CreatureLoginsInfos;

            if (!Thread.CurrentPrincipal.IsInRole(@"Master"))
            {
                // only master user(s) can access non-player creatures
                var _provider = MapContext?.Map;
                if (_provider != null)
                {
                    try
                    {
                        // synchronize
                        _provider.Synchronizer.EnterReadLock();

                        // user controller
                        var _user = Thread.CurrentPrincipal.Identity.Name;
                        _critterList = _critterList
                            .Where(_ci => _provider.GetCreature(_ci.ID)?.CanUserControl(_user) ?? false)
                            .ToList();
                    }
                    finally
                    {
                        // desynchronize
                        if (_provider.Synchronizer.IsReadLockHeld)
                            _provider.Synchronizer.ExitReadLock();
                    }
                }
                else
                {
                    // no list available
                    _critterList.Clear();
                }
            }

            return _critterList;
        }
        #endregion

        #region public List<BitmapImageInfo> GetPortraits(List<Guid> ids)
        public List<BitmapImageInfo> GetPortraits(List<Guid> ids)
        {
            var _provider = IkosaServices.CreatureProvider;
            if (_provider != null)
            {
                return (from _id in ids
                        let _portrait = _provider.GetCreature(_id)?.GetPortrait(MapContext?.Map?.Resources)
                        where _portrait != null
                        select new BitmapImageInfo(_portrait)).ToList();
            }
            return new List<BitmapImageInfo>();
        }
        #endregion

        #region public List<UserInfo> GetUserList(bool allUsers)
        public List<UserInfo> GetUserList(bool allUsers)
        {
            var _principal = Thread.CurrentPrincipal;
            if (_principal?.Identity != null)
            {
                if (!allUsers)
                    return GetLoggedInUserList();
                if (_principal.IsInRole(@"Master"))
                {
                    return UserValidator.UserDefinitions.GetList()
                        .Select(_u =>
                        new UserInfo
                        {
                            UserName = _u.UserName,
                            IsMaster = _u.IsMasterUser,
                            IsDisabled = _u.IsDisabled
                        }).ToList();
                }
            }
            throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authenticated");
        }
        #endregion

        #region public UserInfo Login(Guid id)
        public UserInfo Login(Guid id)
        {
            // user must have authenticated correctly
            var _principal = Thread.CurrentPrincipal;
            if (_principal?.Identity != null)
            {
                try
                {
                    // if actor is already logged in, won't work
                    _TrackedLock.EnterUpgradeableReadLock();
                    var _user = _TrackedUsers.FirstOrDefault(_liu => _liu.UserName.Equals(_principal.Identity.Name, StringComparison.OrdinalIgnoreCase));
                    var _already = _TrackedUsers.FirstOrDefault(_liu => _liu.CreatureInfos.Any(_ci => _ci.ID == id));
                    if (_already != null)
                    {
                        if (_already != _user)
                        {
                            ConsoleLog(@"Login Failure", _principal.Identity.Name, @"Actor already logged in to a different user");
                            throw new FaultException<SecurityFault>(new SecurityFault(), @"Actor already logged in to a different user");
                        }

                        // stitch up session (in case it changed)
                        EnsureSession(_user);
                        return _user;
                    }

                    // get creature info
                    var _critterInfo = MapContext.GetCreatureLoginInfo(id);
                    if (_critterInfo != null)
                    {
                        // only master user(s) can access non-player creatures
                        var _creature = new Lazy<Creature>(() => IkosaServices.CreatureProvider?.GetCreature(id));
                        var _isMaster = _principal.IsInRole(@"Master");
                        if (_isMaster
                            || (_creature.Value?.CanUserControl(_principal.Identity.Name) ?? false))
                        {
                            // make sure creature can act in time tracker
                            void _ensureTimeTracker()
                            {
                                var _map = MapContext?.Map;
                                if (_map != null)
                                {
                                    try
                                    {
                                        _map.Synchronizer.EnterUpgradeableReadLock();
                                        var _tracker = _map.IkosaProcessManager.LocalTurnTracker;
                                        var _budget = _tracker.GetBudget(id);
                                        if (_budget == null)
                                        {
                                            try
                                            {
                                                _map.Synchronizer.EnterWriteLock();
                                                _tracker.AddBudget(
                                                    _creature.Value?.CreateActionBudget(_tracker.RoundMarker) as LocalActionBudget);
                                            }
                                            finally
                                            {
                                                if (_map.Synchronizer.IsWriteLockHeld)
                                                    _map.Synchronizer.ExitWriteLock();
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        if (_map.Synchronizer.IsUpgradeableReadLockHeld)
                                            _map.Synchronizer.ExitUpgradeableReadLock();
                                    }
                                }
                            };

                            // allowed to access
                            if (_user == null)
                            {
                                #region new user login
                                try
                                {
                                    // associate the user-name with the specified actor
                                    _TrackedLock.EnterWriteLock();
                                    _user = new UserInfo
                                    {
                                        UserName = _principal.Identity.Name,
                                        CreatureInfos = new[] { _critterInfo },
                                        IsMaster = _isMaster,
                                        Notifier = OperationContext.Current.GetCallbackChannel<ILoginCallback>()
                                    };
                                    _TrackedUsers.Add(_user);
                                    _ensureTimeTracker();
                                    return _user;
                                }
                                catch (Exception _ex)
                                {
                                    ConsoleLog(@"Login Failure", _principal.Identity.Name, _ex.Message);
                                    throw new FaultException<SecurityFault>(new SecurityFault(), @"Critical failure, logged at console");
                                }
                                finally
                                {
                                    if (_TrackedLock.IsWriteLockHeld)
                                        _TrackedLock.ExitWriteLock();
                                }
                                #endregion
                            }
                            else
                            {
                                // add to list
                                var _newList = _user.CreatureInfos.ToList();
                                EnsureSession(_user);
                                _newList.Add(_critterInfo);
                                _user.CreatureInfos = _newList.ToArray();
                            }
                            _ensureTimeTracker();
                            return _user;
                        }
                        ConsoleLog(@"Login Failure", _principal.Identity.Name, @"Login cannot control creature");
                        throw new FaultException<SecurityFault>(new SecurityFault(), @"Login cannot control creature");
                    }
                    ConsoleLog(@"Login Failure", _principal.Identity.Name, @"Creature not defined");
                    throw new FaultException<SecurityFault>(new SecurityFault(), @"Creature not defined");
                }
                finally
                {
                    if (_TrackedLock.IsUpgradeableReadLockHeld)
                        _TrackedLock.ExitUpgradeableReadLock();

                    // notify
                    DoNotifyUserList(_principal.Identity.Name);
                }
            }
            else
            {
                ConsoleLog(@"Login Failure", _principal?.Identity?.Name, @"User not authenticated");
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authenticated");
            }
        }
        #endregion

        #region public UserInfo Logout(Guid id)
        public UserInfo Logout(Guid id)
        {
            var _principal = Thread.CurrentPrincipal;
            if (_principal?.Identity != null)
            {
                try
                {
                    // if actor not logged in, won't work
                    _TrackedLock.EnterUpgradeableReadLock();
                    var _user = _TrackedUsers.FirstOrDefault(_liu => _liu.UserName.Equals(_principal.Identity.Name, StringComparison.OrdinalIgnoreCase));
                    var _already = _TrackedUsers.FirstOrDefault(_liu => _liu.CreatureInfos.Any(_ci => _ci.ID == id));
                    if (_already == null)
                    {
                        ConsoleLog(@"Logout Failure", _principal.Identity.Name, @"Actor not logged in");
                        throw new FaultException<SecurityFault>(new SecurityFault(), @"Actor not logged in");
                    }
                    else if (_already != _user)
                    {
                        ConsoleLog(@"Logout Failure", _principal.Identity.Name, @"Actor not logged in to current user");
                        throw new FaultException<SecurityFault>(new SecurityFault(), @"Actor not logged in to current user");
                    }

                    try
                    {
                        _TrackedLock.EnterWriteLock();
                        _user.CreatureInfos = _user.CreatureInfos.Where(_ci => _ci.ID != id).ToArray();

                        // TODO: if not creature has NeedsTurnTick or LocalActionBudget.IsInitiative, remove budget

                        if (!_user.CreatureInfos.Any() && !_user.IsMaster)
                        {
                            _TrackedUsers.Remove(_user);
                            DoUserLogout(_principal.Identity.Name);
                            return null;
                        }
                    }
                    finally
                    {
                        if (_TrackedLock.IsWriteLockHeld)
                            _TrackedLock.ExitWriteLock();
                    }

                    // otherwise, ensure the callback is still accurate
                    EnsureSession(_user);
                    return _user;
                }
                finally
                {
                    if (_TrackedLock.IsUpgradeableReadLockHeld)
                        _TrackedLock.ExitUpgradeableReadLock();

                    // notify
                    DoNotifyUserList(_principal.Identity.Name);
                }
            }
            else
            {
                ConsoleLog(@"Logout Failure", _principal.Identity.Name, @"User not authenticated");
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authenticated");
            }
        }
        #endregion

        #region public void LogoutUser()
        public void LogoutUser()
        {
            // user must have authenticated correctly
            var _principal = Thread.CurrentPrincipal;
            if (_principal?.Identity != null)
            {
                LoginService.Logout(_principal.Identity.Name);
            }
        }
        #endregion

        #region public RollerLog RollDice(string key, string description, string expression, Guid notify)
        public RollerLog RollDice(string title, string description, string expression, Guid notify)
        {
            // confirm session
            var _principal = Thread.CurrentPrincipal;
            if ((_principal?.Identity != null) && _principal.IsInRole(notify.ToString()))
            {
                var _notify = notify.ToEnumerable().Union(Guid.Empty.ToEnumerable()).ToList();
                var _roller = new ComplexDiceRoller(expression);
                var _rollVal = _roller.GetRollerLog();
                IkosaServices.QueueNotifySysStatus(new RollNotify(notify, new Description(title, description), _rollVal), _notify);
                IkosaServices.FlushNotifications();
                return _rollVal;
            }
            else
            {
                var _roller = new ComplexDiceRoller(expression);
                var _rollVal = _roller.GetRollerLog();
                return _rollVal;
            }
        }
        #endregion

        #region public FlowState GetFlowState()
        public FlowState GetFlowState()
        {
            var _userName = Thread.CurrentPrincipal?.Identity?.Name ?? @"???";
            EnsureSession(_userName);

            return OnGetFlowState?.Invoke() ?? FlowState.Normal;
        }
        #endregion

        #region public bool GetPauseState()
        public bool GetPauseState()
        {
            var _userName = Thread.CurrentPrincipal?.Identity?.Name ?? @"???";
            EnsureSession(_userName);

            return OnGetPauseState?.Invoke() ?? false;
        }
        #endregion

        #endregion
    }
}
