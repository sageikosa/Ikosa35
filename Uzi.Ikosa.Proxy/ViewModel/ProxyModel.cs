using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Contracts.Host;
using Uzi.Ikosa.Proxy.IkosaSvc;
using Uzi.Ikosa.Proxy.MasterSvc;
using Uzi.Ikosa.Proxy.VisualizationSvc;
using Uzi.Visualize;
using Uzi.Visualize.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    [CallbackBehavior(UseSynchronizationContext = false, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ProxyModel : ViewModelBase, ILoginCallback, IVisualizationCallback,
        IIkosaCallback, IMasterControlCallback
    {
        #region ctor()
        public ProxyModel(string userName, string password, string host, string port,
            Action<ActorModel> generateView, Action<Action> actionDispatcher, Action<IsMasterModel> generateLog)
        {
            _FlowState = FlowState.Shutdown;
            _Dispatcher = actionDispatcher;
            _UserName = userName;
            _Password = password;
            _Host = host;
            _Port = port;
            _MessagesVisible = false;
            _GenerateView = generateView;
            _GenerateLog = generateLog;

            _Portraits = new ConcurrentDictionary<Guid, BitmapImageInfo>();
            _MessageBoard = new MessageBoardModel(this);
            _LocalState = new ConcurrentDictionary<Guid, ActorLocalState>();

            UserListChanged();
            NewMessage();
            _Resolver = new MapIconResolver(this);
        }
        #endregion

        #region data
        private readonly List<Tuple<DateTime, string>> _Exceptions = new List<Tuple<DateTime, string>>();

        // connection
        private readonly string _Host;
        private readonly string _Port;
        private readonly string _UserName;
        private readonly string _Password;
        private UserInfo _User;

        // messaging and other users
        private readonly List<UserMessage> _Messages = new List<UserMessage>();
        private List<UserInfo> _Users = new List<UserInfo>();
        private readonly MessageBoardModel _MessageBoard;
        private bool _MessagesVisible;
        private readonly ConcurrentDictionary<Guid, BitmapImageInfo> _Portraits;

        // general system state
        private FlowState _FlowState;
        private bool _IsPaused;
        private List<string> _WaitList = new List<string>();

        // actors/master
        private ViewModelBase _SelectedClientSelector;
        internal List<ActorModel> _Actors = new List<ActorModel>();
        private IsMasterModel _IsMaster;
        private readonly Action<ActorModel> _GenerateView;
        private readonly Action<IsMasterModel> _GenerateLog;
        private readonly ConcurrentDictionary<Guid, ActorLocalState> _LocalState;
        private Dictionary<Guid, CreatureLoginInfo> _AllCreatures;

        // proxy management
        private readonly ReaderWriterLockSlim _LoginLock = new ReaderWriterLockSlim();
        private readonly ReaderWriterLockSlim _IkosaLock = new ReaderWriterLockSlim();
        private readonly ReaderWriterLockSlim _ViewLock = new ReaderWriterLockSlim();
        private readonly ReaderWriterLockSlim _MasterLock = new ReaderWriterLockSlim();
        private LoginServiceClient _LoginProxy;
        private IkosaServicesClient _IkosaProxy;
        private VisualizationServiceClient _ViewProxy;
        private MasterServicesClient _MasterProxy;
        private readonly Action<Action> _Dispatcher;

        // resource
        private readonly IResolveIcon _Resolver;
        #endregion

        public Action<SysNotify> AddAnimatingLog { get; set; }
        public Action<Exception> ObserveException { get; set; }
        public Action NeedsAttention { get; set; }

        // NOTE: this could be a delegate instead if multi-casting is really needed...
        public Action OnLogOut { get; set; }
        public Action<ActorModel> GenerateView => _GenerateView;
        public Action<IsMasterModel> GenerateLog => _GenerateLog;

        public string Host => _Host;
        public string Port => _Port;
        public string UserName => _UserName;
        public string Password => _Password;
        public bool IsLoggedIn => _User != null;

        // ----- messages

        #region public bool MessagesVisible { get; set; }
        public bool MessagesVisible
        {
            get { return _MessagesVisible; }
            set
            {
                _MessagesVisible = value;
                DoPropertyChanged(nameof(MessagesVisible));
            }
        }
        #endregion

        #region public IEnumerable<UserMessage> Messages { get; }
        public IEnumerable<UserMessage> Messages
        {
            get
            {
                lock (_Messages)
                {
                    return _Messages.ToList();
                }
            }
        }
        #endregion

        #region public void SendMessage(string userName, string text)
        public void SendMessage(string userName, string text)
        {
            LoginProxy.Service.SendMessage(userName, text);

            // adds to our own side
            lock (_Messages)
            {
                _Messages.Add(new UserMessage
                {
                    ToUser = userName,
                    FromUser = UserName,
                    Created = DateTime.Now,
                    IsPublic = string.IsNullOrWhiteSpace(userName),
                    Message = text
                });
            }
            DoPropertyChanged(nameof(Messages));
        }
        #endregion

        public void ClearMessages()
        {
            lock (_Messages)
                _Messages.Clear();
            DoPropertyChanged(nameof(Messages));
        }

        // ----- exceptions

        #region public IEnumerable<Tuple<DateTime, string>> Exceptions { get; }
        public IEnumerable<Tuple<DateTime, string>> Exceptions
        {
            get
            {
                lock (_Exceptions)
                {
                    return _Exceptions.ToList();
                }
            }
        }
        #endregion

        #region internal void MarkException(Exception exception)
        internal void MarkException(Exception exception)
        {
            lock (_Exceptions)
            {
                _Exceptions.Add(new Tuple<DateTime, string>(DateTime.Now, exception.Message));
            }
            DoPropertyChanged(nameof(Exceptions));
            ObserveException?.Invoke(exception);
        }
        #endregion

        public void ClearExceptions()
        {
            lock (_Exceptions)
                _Exceptions.Clear();
            DoPropertyChanged(nameof(Exceptions));
        }

        // ----- client selectors, users and login

        public IEnumerable<UserInfo> Users => _Users;
        public IEnumerable<ActorModel> Actors => _Actors.ToList();

        #region public CreatureLoginInfo AddPortrait(CreatureLoginInfo critter)
        /// <summary>Updates portrait and return the updated CreatureLoginInfo</summary>
        public CreatureLoginInfo AddPortrait(CreatureLoginInfo critter)
        {
            critter.Portrait = _Portraits.GetOrAdd(critter.ID, (_id) =>
             {
                 var _l = new List<Guid>
                 {
                     _id
                 };
                 return LoginProxy.Service.GetPortraits(_l).FirstOrDefault();
             });
            return critter;
        }
        #endregion

        private IEnumerable<ViewModelBase> MessageBoard { get { yield return _MessageBoard; } }

        public IEnumerable<ViewModelBase> ClientSelectors
            => IsMasterModel.Union(_Actors.Where(_a => _a.IsListed)).Concat(MessageBoard).ToList();

        #region public ViewModelBase SelectedClientSelector { get; set; }
        public ViewModelBase SelectedClientSelector
        {
            get { return _SelectedClientSelector; }
            set
            {
                _SelectedClientSelector = value;
                DoPropertyChanged(nameof(SelectedClientSelector));
            }
        }
        #endregion

        public IResolveIcon IconResolver => _Resolver;

        #region internal void AddActor(ActorModel actor)
        internal void AddActor(ActorModel actor)
        {
            var _actors = _Actors.ToList();
            _actors.Add(actor);
            _Actors = _actors;
            ResyncAdvancements();
            SynchronizeCallbacks();
            DoPropertyChanged(nameof(Actors));
            DoPropertyChanged(nameof(ClientSelectors));
        }
        #endregion

        internal void RefreshActors()
        {
            DoPropertyChanged(nameof(Actors));
            DoPropertyChanged(nameof(ClientSelectors));
        }

        #region public void LoginCreature(CreatureLoginInfo creature, Action<CreatureLoginInfo> create)
        public void LoginCreature(CreatureLoginInfo creature, Action<CreatureLoginInfo> createCreature, Action<IsMasterModel> createMasterLog)
        {
            _User = LoginProxy.Service.Login(creature.ID);
            var _critter = AddPortrait(_User.CreatureInfos.FirstOrDefault(_ci => _ci.ID == creature.ID));
            if (_critter != null)
            {
                var _actors = Actors;
                if (!_actors.Any(_a => _a.CreatureLoginInfo.ID == creature.ID))
                {
                    createCreature(_critter);
                }
            }
            if ((_User?.IsMaster ?? false) && (_IsMaster == null))
            {
                // is user is master and not already connected, connect master
                _IsMaster = new IsMasterModel(this);
                createMasterLog(_IsMaster);
                DoPropertyChanged(nameof(IsMasterModel));
                DoPropertyChanged(nameof(ClientSelectors));
            }
            FlowStateChanged();
            PauseChanged(false);
        }
        #endregion

        #region public void LogoutCreature(Guid id)
        public void LogoutCreature(Guid id)
        {
            try
            {
                _User = LoginProxy.Service.Logout(id);
            }
            catch
            {
                // TODO: probably not "logged-in"
            }

            var _actors = _Actors.ToList();
            var _actor = _actors.FirstOrDefault(_a => _a.CreatureLoginInfo.ID == id);
            if (_actor != null)
            {
                // then remove from list...
                _actors.Remove(_actor);
                _Actors = _actors;

                // ...and notify of changes
                DoPropertyChanged(nameof(Actors));
                DoPropertyChanged(nameof(ClientSelectors));
            }
            FlowStateChanged();
            PauseChanged(false);
        }
        #endregion

        #region public void Logout()
        public void Logout()
        {
            ViewProxy.Service.DeRegisterCallback();
            IkosaProxy.Service.DeRegisterCallback();
            LoginProxy.Service.LogoutUser();
            MasterProxy.Service.DeRegisterCallback();
            _User = null;
            _Actors = new List<ActorModel>();
            _IsMaster = null;
            DoPropertyChanged(nameof(Actors));
            DoPropertyChanged(nameof(ClientSelectors));
            ClearMessages();
            ClearExceptions();
            OnLogOut?.Invoke();

            // flowstate
            _FlowState = FlowState.Shutdown;
            DoPropertyChanged(nameof(FlowState));
        }
        #endregion

        public List<CreatureLoginInfo> GetAvailableCreatures()
            => LoginProxy.Service.GetAvailableCreatures().Select(_cli => AddPortrait(_cli)).ToList();

        public Dictionary<Guid, CreatureLoginInfo> GetAllCreatures()
        {
            if (_AllCreatures == null)
            {
                _AllCreatures = LoginProxy.Service.GetAllCreatures()
                    .Select(_cli => AddPortrait(_cli))
                    .ToDictionary(_cli => _cli.ID);
            }
            return _AllCreatures;
        }

        public string GetPrincipalPrefix(Guid id)
        {
            var _lut = GetAllCreatures();
            if (_lut.TryGetValue(id, out var _critter))
                return $@"[{_critter.Name}] ";
            return string.Empty;
        }

        #region public List<TeamGroupInfo> GetTeamRosters()
        public List<TeamGroupInfo> GetTeamRosters()
            => (from _team in MasterProxy.Service.GetTeams()
                from _cti in _team.PrimaryCreatures
                let _critter = AddPortrait(_cti.CreatureLoginInfo)
                group new CreatureTrackerInfo
                {
                    CreatureLoginInfo = _critter,
                    UserInfos = _cti.UserInfos,
                    LocalActionBudgetInfo = _cti.LocalActionBudgetInfo
                } by _team)
                .Select(_x => new TeamGroupInfo
                {
                    ID = _x.Key.ID,
                    Name = _x.Key.Name,
                    PrimaryCreatures = _x.ToArray()
                })
                .ToList();
        #endregion

        // ----- ActorLocalState

        #region public ActorLocalState GetLocalState(Guid id)
        public ActorLocalState GetLocalState(Guid id, ViewPointType viewPointType)
            => _LocalState.GetOrAdd(id, new ActorLocalState
            {
                ID = id,
                MoveStart = MoveStart.Standard,
                UseDouble = false,
                MovementInfoID = null,
                ViewPointType = viewPointType,
                ThirdPersonState = (new ThirdPersonViewPointState { FieldOfView = 115, ShowToken = true }).Value,
                GameBoardState = (new GameBoardViewPointState { Above = false, Gaze = false, Heading = 0, WidthCells = 10 }).Value
            });
        #endregion

        #region public void UpdateLocalState(Guid id, Func<ActorLocalState, ActorLocalState> update)
        public void UpdateLocalState(Guid id, ViewPointType viewPointType, Func<ActorLocalState, ActorLocalState> update)
            => _LocalState.AddOrUpdate(id,
                (key) => update(new ActorLocalState
                {
                    ID = key,
                    MoveStart = MoveStart.Standard,
                    UseDouble = false,
                    MovementInfoID = null,
                    ViewPointType = viewPointType
                }),
                (key, current) => update(current));
        #endregion

        // ----- master control

        #region private IEnumerable<ViewModelBase> IsMaster { get; }
        private IEnumerable<ViewModelBase> IsMasterModel
        {
            get
            {
                if (_User?.IsMaster ?? false)
                    yield return _IsMaster;
                yield break;
            }
        }
        #endregion

        public bool IsMaster => _User?.IsMaster ?? false;

        #region public void DoMasterDispatch(Action dispatchOperations)
        /// <summary>Dispatches an operation to the synchronized root context.  Typically, this is where the IsMasterModel has it's UI</summary>
        public void DoMasterDispatch(Action dispatchOperations)
            => _Dispatcher?.Invoke(dispatchOperations);
        #endregion

        // ----- turn management

        public IEnumerable<string> WaitList => _WaitList.Select(_s => _s).ToList();
        public Visibility WaitingOnVisibility => (_WaitList?.Any() ?? false) ? Visibility.Visible : Visibility.Collapsed;

        #region public LocalTurnTrackerInfo GetLocalTurnTracker()
        public LocalTurnTrackerInfo GetLocalTurnTracker(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                if (_User?.IsMaster ?? false)
                {
                    // maximal tracker info for master
                    var _critters = GetAllCreatures();
                    var _tracker = IkosaProxy.Service.GetTurnTracker(null);
                    foreach (var _rb in _tracker.ReactableBudgets)
                    {
                        _rb.CreatureLoginInfo = _critters.ContainsKey(_rb.ActorID)
                            ? _critters[_rb.ActorID]
                            : null;
                    }
                    foreach (var _db in _tracker.DelayedBudgets)
                    {
                        _db.CreatureLoginInfo = _critters.ContainsKey(_db.ActorID)
                            ? _critters[_db.ActorID]
                            : null;
                    }
                    foreach (var _budget in _tracker.UpcomingBudgets)
                    {
                        _budget.CreatureLoginInfo = _critters.ContainsKey(_budget.ActorID)
                            ? _critters[_budget.ActorID]
                            : null;
                    }
                    if (_tracker.LeadingTick?.Budgets != null)
                    {
                        foreach (var _budget in _tracker.LeadingTick.Budgets)
                        {
                            _budget.CreatureLoginInfo = _critters.ContainsKey(_budget.ActorID)
                                ? _critters[_budget.ActorID]
                                : null;
                        }
                    }
                    return _tracker;
                }
            }
            else
            {
                // minimal tracker info for players
                var _critters = GetAllCreatures();
                var _tracker = IkosaProxy.Service.GetTurnTracker(id);
                foreach (var _budget in _tracker.UpcomingBudgets)
                {
                    _budget.CreatureLoginInfo = _critters.ContainsKey(_budget.ActorID)
                        ? _critters[_budget.ActorID]
                        : null;
                }
                return _tracker;
            }
            return null;
        }
        #endregion

        public FlowState FlowState => _FlowState;
        public bool IsPaused => _IsPaused;

        #region private void ResyncAdvancements()
        private void ResyncAdvancements()
        {
            var _adv = IkosaProxy.Service.GetAdvanceableCreatures();
            foreach (var _actor in Actors)
            {
                _actor.AdvanceableCreature = _adv.FirstOrDefault(_ac => _ac.ID == _actor.FulfillerID);
            }
        }
        #endregion

        public RollerLog RollDice(string key, string description, string expression, Guid notifier)
            => LoginProxy.Service.RollDice(key, description, expression, notifier);

        // ----- proxies

        #region public LoginServiceClient LoginProxy { get; }
        public LoginServiceClient LoginProxy
        {
            get
            {
                // if this grabbed elsewhere on this thread, no need to re-enter
                var _alreadyReading = _LoginLock.IsReadLockHeld;
                var _alreadyWriting = _LoginLock.IsWriteLockHeld;
                try
                {
                    if (!_alreadyReading && !_alreadyWriting)
                    {
                        _LoginLock.EnterReadLock();
                    }
                    if (((_LoginProxy?.State ?? CommunicationState.Closed) != CommunicationState.Opened))
                    {
                        if (!_alreadyReading && !_alreadyWriting)
                        {
                            _LoginLock.ExitReadLock();
                        }
                        try
                        {
                            if (!_alreadyWriting)
                            {
                                _LoginLock.EnterWriteLock();
                            }
                            if (((_LoginProxy?.State ?? CommunicationState.Closed) != CommunicationState.Opened))
                            {
                                if (_LoginProxy?.State == CommunicationState.Faulted)
                                    _LoginProxy?.Abort();

                                var _instance = new InstanceContext(this);
                                _LoginProxy = new LoginServiceClient($@"net.tcp://{_Host}:{_Port}/ikosa/Login",
                                    this, _UserName, _Password);
                                DoPropertyChanged(nameof(LoginProxy));
                            }
                        }
                        finally
                        {
                            if (!_alreadyWriting && _LoginLock.IsWriteLockHeld)
                                _LoginLock.ExitWriteLock();
                        }
                    }
                    return _LoginProxy;
                }
                finally
                {
                    if (!_alreadyReading && _LoginLock.IsReadLockHeld)
                        _LoginLock.ExitReadLock();
                }
            }
        }
        #endregion

        #region public IkosaServicesClient IkosaProxy { get; }
        public IkosaServicesClient IkosaProxy
        {
            get
            {
                // if this grabbed elsewhere on this thread, no need to re-enter
                var _alreadyReading = _IkosaLock.IsReadLockHeld;
                var _alreadyWriting = _IkosaLock.IsWriteLockHeld;
                try
                {
                    if (!_alreadyReading && !_alreadyWriting)
                    {
                        _IkosaLock.EnterReadLock();
                    }
                    if (((_IkosaProxy?.State ?? CommunicationState.Closed) != CommunicationState.Opened))
                    {
                        if (!_alreadyReading && !_alreadyWriting)
                        {
                            _IkosaLock.ExitReadLock();
                        }
                        try
                        {
                            if (!_alreadyWriting)
                            {
                                _IkosaLock.EnterWriteLock();
                            }
                            if (((_IkosaProxy?.State ?? CommunicationState.Closed) != CommunicationState.Opened))
                            {
                                if (_IkosaProxy?.State == CommunicationState.Faulted)
                                    _IkosaProxy?.Abort();

                                _IkosaProxy = new IkosaServicesClient($@"net.tcp://{_Host}:{_Port}/ikosa/services",
                                    this, _UserName, _Password);
                                _IkosaProxy.Service.RegisterCallback(_Actors.Select(_a => _a.CreatureLoginInfo.ID.ToString()).ToArray());
                                DoPropertyChanged(nameof(IkosaProxy));
                            }
                        }
                        finally
                        {
                            if (!_alreadyWriting && _IkosaLock.IsWriteLockHeld)
                                _IkosaLock.ExitWriteLock();
                        }
                    }
                    return _IkosaProxy;
                }
                finally
                {
                    if (!_alreadyReading && _IkosaLock.IsReadLockHeld)
                        _IkosaLock.ExitReadLock();
                }
            }
        }
        #endregion

        #region public VisualizationServiceClient ViewProxy { get; }
        public VisualizationServiceClient ViewProxy
        {
            get
            {
                // if this grabbed elsewhere on this thread, no need to re-enter
                var _alreadyReading = _ViewLock.IsReadLockHeld;
                var _alreadyWriting = _ViewLock.IsWriteLockHeld;
                try
                {
                    if (!_alreadyReading && !_alreadyWriting)
                    {
                        _ViewLock.EnterReadLock();
                    }
                    if (((_ViewProxy?.State ?? CommunicationState.Closed) != CommunicationState.Opened))
                    {
                        if (!_alreadyReading && !_alreadyWriting)
                        {
                            _ViewLock.ExitReadLock();
                        }
                        try
                        {
                            if (!_alreadyWriting)
                            {
                                _ViewLock.EnterWriteLock();
                            }
                            if (((_ViewProxy?.State ?? CommunicationState.Closed) != CommunicationState.Opened))
                            {
                                if (_ViewProxy?.State == CommunicationState.Faulted)
                                    _ViewProxy?.Abort();

                                _ViewProxy = new VisualizationServiceClient($@"net.tcp://{_Host}:{_Port}/ikosa/Visualization",
                                    this, _UserName, _Password);
                                _ViewProxy.Service.RegisterCallback(_Actors.Select(_a => _a.CreatureLoginInfo.ID.ToString()).ToArray());
                                DoPropertyChanged(nameof(ViewProxy));
                            }
                        }
                        finally
                        {
                            if (!_alreadyWriting && _ViewLock.IsWriteLockHeld)
                                _ViewLock.ExitWriteLock();
                        }
                    }
                    return _ViewProxy;
                }
                finally
                {
                    if (!_alreadyReading && _ViewLock.IsReadLockHeld)
                        _ViewLock.ExitReadLock();
                }
            }
        }
        #endregion

        #region public MasterServicesClient MasterProxy { get; }
        public MasterServicesClient MasterProxy
        {
            get
            {
                // if this grabbed elsewhere on this thread, no need to re-enter
                var _alreadyReading = _MasterLock.IsReadLockHeld;
                var _alreadyWriting = _MasterLock.IsWriteLockHeld;
                try
                {
                    if (!_alreadyReading && !_alreadyWriting)
                    {
                        _MasterLock.EnterReadLock();
                    }
                    if (((_MasterProxy?.State ?? CommunicationState.Closed) != CommunicationState.Opened))
                    {
                        if (!_alreadyReading && !_alreadyWriting)
                        {
                            _MasterLock.ExitReadLock();
                        }
                        try
                        {
                            if (!_alreadyWriting)
                            {
                                _MasterLock.EnterWriteLock();
                            }
                            if (((_MasterProxy?.State ?? CommunicationState.Closed) != CommunicationState.Opened))
                            {
                                if (_MasterProxy?.State == CommunicationState.Faulted)
                                    _MasterProxy?.Abort();

                                _MasterProxy = new MasterServicesClient($@"net.tcp://{_Host}:{_Port}/ikosa/master",
                                    this, _UserName, _Password);
                                _MasterProxy.Service.RegisterCallback();
                                DoPropertyChanged(nameof(MasterProxy));
                            }
                        }
                        finally
                        {
                            if (!_alreadyWriting && _MasterLock.IsWriteLockHeld)
                                _MasterLock.ExitWriteLock();
                        }
                    }
                    return _MasterProxy;
                }
                finally
                {
                    if (!_alreadyReading && _MasterLock.IsReadLockHeld)
                        _MasterLock.ExitReadLock();
                }
            }
        }
        #endregion

        // ----- callback control

        #region public void SynchronizeCallbacks()
        public void SynchronizeCallbacks()
        {
            var _actors = _Actors.Select(_a => _a.CreatureLoginInfo.ID.ToString()).ToArray();
            IkosaProxy.Service.RegisterCallback(_actors);
            ViewProxy.Service.RegisterCallback(_actors);
            MasterProxy.Service.RegisterCallback();
            var _newList = IkosaProxy.Service.GetWaitingOnUsers();

            // post initialization callback bootstrapping
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var _old = _WaitList;
                    _WaitList = _newList;
                    if ((AddAnimatingLog != null) && _WaitList.Any())
                    {
                        if (_WaitList.Any(_s => !_old.Contains(_s)))
                        {
                            AddAnimatingLog(new SysNotify(@"Waiting", _WaitList
                                    .Where(_s => !_old.Contains(_s))
                                    .Select(_wl => new Info { Message = $@"Waiting on {_wl}" }).ToArray()));
                        }
                    }
                    DoPropertyChanged(nameof(WaitList));
                    DoPropertyChanged(nameof(WaitingOnVisibility));

                    // game master
                    if (_User?.IsMaster ?? false)
                    {
                        // _IsMaster?.UpdatePrerequisites(); // ???
                        _IsMaster?.IsNewerSerialState();
                        _IsMaster?.UpdateTurnTracker();
                    }
                }
                catch (Exception _except)
                {
                    MarkException(_except);
                }
            });
        }
        #endregion

        #region ILoginServiceCallback Members

        public void NewMessage()
        {
            Task.Factory.StartNew((Action)(() =>
            {
                try
                {
                    var _hasNew = false;
                    lock (_Messages)
                    {
                        var _allMsgs = LoginProxy.Service.GetMessages();
                        foreach (var _msg in _allMsgs)
                            _Messages.Add(_msg);
                        _hasNew = _allMsgs.Any();
                    }
                    if (_hasNew)
                    {
                        DoPropertyChanged(nameof(Messages));
                        MessagesVisible = true;
                    }
                }
                catch (Exception _except)
                {
                    MarkException(_except);
                }
            }));
        }

        public void UserListChanged()
        {
            Task.Factory.StartNew((Action)(() =>
            {
                try
                {
                    var _userList = LoginProxy.Service.GetUserList(false).ToList();

                    // add all and host
                    _userList.Insert(0, new UserInfo { UserName = @"(Host)" });
                    _userList.Insert(0, new UserInfo { UserName = @"(All)" });

                    // remove self
                    var _self = _userList.FirstOrDefault(_u => _u.UserName.Equals(_UserName, StringComparison.OrdinalIgnoreCase));
                    if (_self != null)
                        _userList.Remove(_self);

                    _Users = _userList;
                    DoPropertyChanged(nameof(Users));
                }
                catch (Exception _except)
                {
                    MarkException(_except);
                }
            }));
        }

        public void PauseChanged(bool isPaused)
        {
            Task.Factory.StartNew((Action)(() =>
            {
                try
                {
                    _IsPaused = LoginProxy.Service.GetPauseState();
                    // TODO: set actors to paused/unpaused (for UI changes)
                    DoPropertyChanged(nameof(IsPaused));
                }
                catch (Exception _except)
                {
                    MarkException(_except);
                }
            }));
        }

        public void FlowStateChanged()
        {
            Task.Factory.StartNew((Action)(() =>
            {
                try
                {
                    _FlowState = LoginProxy.Service.GetFlowState();
                    switch (_FlowState)
                    {
                        case FlowState.Advancement:
                            ResyncAdvancements();
                            break;

                        case FlowState.Normal:
                        case FlowState.Shutdown:
                        default:
                            foreach (var _a in Actors)
                            {
                                _a.AdvanceableCreature = null;
                            }
                            break;
                    }
                    DoPropertyChanged(nameof(FlowState));
                }
                catch (Exception _except)
                {
                    MarkException(_except);
                }
            }));
        }

        public void UserLogout(string userName)
        {
            // TODO: anything relevant if this is the user that was logged out?
        }

        #endregion

        #region IVisualizationServiceCallback Members

        public void BrushCollectionChanged(string name) { }
        public void Model3DChanged(string name) { }
        public void ImageChanged(string name, string modelName, string brushCollectionName) { }
        public void TileSetChanged(string cellMaterial, string tiling) { }

        public void MapChanged()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    foreach (var _actor in Actors)
                        _actor.ObservableActor?.UpdateSensors(true);
                }
                catch (Exception _except)
                {
                    MarkException(_except);
                }
            });
        }

        #endregion

        #region IIkosaServicesCallback Members

        #region public void SystemNotifications(Notification[] sysInfos)
        public void SystemNotifications(Notification[] notifications)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var _actors = _Actors;
                    var _critterRefresh = new List<ActorModel>();
                    var _sensorRefresh = new List<ActorModel>();
                    foreach (var _notif in notifications)
                    {
                        var _actor = _actors.FirstOrDefault(_a => _a.CreatureLoginInfo.ID == _notif.NotifyID);
                        if (_actor != null)
                        {
                            _actor.AddNotification(_notif);
                            foreach (var _n in _notif.Notifications)
                            {
                                // internal changes
                                if (_n is RefreshNotify _refresh)
                                {
                                    if ((_refresh.RefreshFlags & RefreshFlags.Creature) == RefreshFlags.Creature)
                                    {
                                        if (!_critterRefresh.Contains(_actor))
                                            _critterRefresh.Add(_actor);
                                    }
                                    // external changes
                                    if (((_refresh.RefreshFlags & RefreshFlags.SensorHost) == RefreshFlags.SensorHost)
                                        || ((_refresh.RefreshFlags & RefreshFlags.Awarenesses) == RefreshFlags.Awarenesses))
                                    {
                                        if (!_sensorRefresh.Contains(_actor))
                                            _sensorRefresh.Add(_actor);
                                    }
                                }
                                else
                                {
                                    _actor.ObservableActor?.Dispatcher.Invoke(new Action(() =>
                                    {
                                        _actor.ObservableActor?.Notifies.Add(new SysNotifyVM(_actor.ObservableActor?.GetNextNotifyID() ?? -1, _n, null));
                                    }));
                                    AddAnimatingLog?.Invoke(_n);
                                }
                            }
                        }
                        else if (_notif.NotifyID == Guid.Empty)
                        {
                            foreach (var _n in _notif.Notifications.Where(_nt => !(_nt is RefreshNotify)))
                            {
                                _IsMaster?.DoMasterLogOutput(_n);
                            }
                        }
                        else
                        {
                            Debug.WriteLine($@"ProxyModel.SystemNotifications missing actor={_notif.NotifyID} @ {DateTime.Now:HH:mm:ss.fff}");
                        }
                    }

                    // refreshes
                    foreach (var _sensor in _sensorRefresh)
                    {
                        _sensor.ObservableActor?.UpdateSensors(false);
                    }
                    foreach (var _critter in _critterRefresh)
                    {
                        _critter.UpdateCreature();
                        _critter.UpdateActions();
                    }
                }
                catch (Exception _except)
                {
                    MarkException(_except);
                }
            });
        }
        #endregion

        #region public void SerialStateChanged()
        public void SerialStateChanged()
        {
            try
            {
                var _actors = Actors;
                foreach (var _actor in _actors)
                {
                    var _a = _actor;
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            if (_a.IsNewerSerialState())
                            {
                                _a.UpdateTurnTracker();
                                if (_a.TurnTracker.TickTrackerMode != TickTrackerMode.TimelineFlowing)
                                {
                                    _a.UpdateCreature();
                                    _a.ObservableActor?.UpdateSensors(false);
                                    _a.UpdateActions();
                                    _a.UpdatePrerequisites();
                                }
                            }
                        }
                        catch (Exception _except)
                        {
                            MarkException(_except);
                        }
                    });
                }

                // game master
                if (_User?.IsMaster ?? false)
                {
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            if (_IsMaster?.IsNewerSerialState() ?? false)
                            {
                                _IsMaster?.UpdatePrerequisites();
                                _IsMaster?.UpdateTurnTracker();
                            }
                        }
                        catch (Exception _except)
                        {
                            MarkException(_except);
                        }
                    });
                }
            }
            catch (Exception _except)
            {
                MarkException(_except);
            }
        }
        #endregion

        #region public void WaitingOnUsers(List<string> waitList)
        public void WaitingOnUsers(List<string> waitList)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var _old = _WaitList;
                    _WaitList = waitList.ToList();
                    if ((AddAnimatingLog != null) && _WaitList.Any())
                    {
                        if (_WaitList.Any(_s => !_old.Contains(_s)))
                        {
                            AddAnimatingLog(new SysNotify(@"Waiting", _WaitList
                                    .Where(_s => !_old.Contains(_s))
                                    .Select(_wl => new Info { Message = $@"Waiting on {_wl}" }).ToArray()));
                        }
                    }
                    DoPropertyChanged(nameof(WaitList));
                    DoPropertyChanged(nameof(WaitingOnVisibility));
                }
                catch (Exception _except)
                {
                    MarkException(_except);
                }
            });
        }
        #endregion

        #endregion

        #region IMasterControlCallback Members

        public void Stub()
        {
        }

        #endregion
    }
}