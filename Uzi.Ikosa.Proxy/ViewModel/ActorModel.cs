using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Uzi.Visualize;
using System.Diagnostics;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Contracts.Host;
using System.Collections.ObjectModel;
using Uzi.Ikosa.Proxy.IkosaSvc;
using System.Threading;
using Uzi.Ikosa.Contracts.Infos;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class ActorModel : ViewModelBase, IPrerequisiteProxy
    {
        #region ctor()
        public ActorModel(ProxyModel proxies, CreatureLoginInfo login, bool listed)
        {
            _Proxies = proxies;
            _CreatureLogin = login;
            _DoAction = new RelayCommand<object>(DoAction_Executed, DoAction_CanExecute);
            _SendPrerequisites = new RelayCommand(SendPrerequisites_Executed);
            _AbortSpan = new RelayCommand(AbortSpan_Executed, AbortSpan_CanExecute);
            _EndTurn = new RelayCommand(EndTurn_Executed, EndTurn_CanExecute);
            _Cancel = new RelayCommand(Cancel_Executed, Cancel_CanExecute);
            _SetIdentity = new RelayCommand<Tuple<ActorModel, ObjectInfo, Guid>>(SetIdentity_Executed, SetIdentity_CanExecute);
            _LogOut = new RelayCommand(() => LogOut_Executed());
            _OpenView = new RelayCommand(() => ShowObservableActor());
            UpdateCreature();
            UpdateActions();
            SetCreatureIconResolver();
            SetActionIconResolver();
            UpdatePrerequisites();
            _Advancement = null;
            _ObservableActor = null;
            _Listed = listed;
            proxies.AddActor(this);
            _TabIndex = 0;
            _SerialState = proxies.IkosaProxy.Service.GetSerialState();
            _TurnTracker = proxies.GetLocalTurnTracker(CreatureLoginInfo.ID.ToString());
        }
        #endregion

        #region data
        // creature
        private CreatureModel _Creature;
        private List<ItemSlotInfo> _ItemSlots = new List<ItemSlotInfo>();
        private List<PossessionInfo> _Possessions = new List<PossessionInfo>();

        private AdvancementVM _Advancement;
        private readonly CreatureLoginInfo _CreatureLogin;
        private LocalActionBudgetInfo _Budget;
        private readonly NotificationVM _Notifications = new NotificationVM();
        private List<ActionInfo> _Actions = new List<ActionInfo>();
        private PrerequisiteListModel _Prerequisites = null;
        private LocalTurnTrackerInfo _TurnTracker = null;
        private ulong _SerialState;

        // integration
        private readonly ProxyModel _Proxies;
        private readonly RelayCommand<object> _DoAction;
        private readonly RelayCommand _SendPrerequisites;
        private readonly RelayCommand _AbortSpan;
        private readonly RelayCommand _EndTurn;
        private readonly RelayCommand _Cancel;
        private readonly RelayCommand<Tuple<ActorModel, ObjectInfo, Guid>> _SetIdentity;
        private readonly RelayCommand _LogOut;
        private readonly RelayCommand _OpenView;

        private ObservableActor _ObservableActor;

        // ui control
        private int _TabIndex;
        private TabIndexEnum _LastSelected = TabIndexEnum.Character;
        private bool _Listed;
        #endregion

        // ----- "pure" view-model -----

        public bool IsAdvancing
            => AdvanceableCreature != null;

        public Visibility AdvancementVisibility
            => IsAdvancing ? Visibility.Visible : Visibility.Collapsed;

        public AdvancementVM AdvancementVM
            => _Advancement;

        public bool IsListed
        {
            get => _Listed;
            set
            {
                if (_Listed != value)
                {
                    _Listed = value;
                    DoPropertyChanged(nameof(IsListed));
                    Proxies.RefreshActors();
                }
            }
        }

        public Visibility PinVisibility => Proxies.IsMaster ? Visibility.Visible : Visibility.Collapsed;

        #region public AdvanceableCreature AdvanceableCreature { get; set; }
        public AdvanceableCreature AdvanceableCreature
        {
            get => _Advancement?.AdvanceableCreature;
            set
            {
                if (value != null)
                {
                    _Advancement = new AdvancementVM(this, value);
                }
                else
                {
                    _Advancement = null;
                }
                if (value != null)
                {
                    // switch to advancement
                    SelectedTabIndexEnum = TabIndexEnum.Advancement;
                }
                else if (SelectedTabIndexEnum == TabIndexEnum.Advancement)
                {
                    // reset tab index if advancement ended
                    SelectedTabIndexEnum = TabIndexEnum.Character;
                }
                DoPropertyChanged(nameof(AdvanceableCreature));
                DoPropertyChanged(nameof(AdvancementVM));
                DoPropertyChanged(nameof(IsAdvancing));
                DoPropertyChanged(nameof(AdvancementVisibility));
            }
        }
        #endregion

        #region public int SelectedTabIndex { get; set; }
        public enum TabIndexEnum { Character, Inventory, Prerequisites, Log, Advancement };

        public TabIndexEnum SelectedTabIndexEnum
        {
            get { return (TabIndexEnum)_TabIndex; }
            set
            {
                if (value != TabIndexEnum.Prerequisites)
                    _LastSelected = value;
                SelectedTabIndex = (int)value;
            }
        }

        public int SelectedTabIndex
        {
            get => _TabIndex;
            set
            {
                _TabIndex = value;
                DoPropertyChanged(nameof(SelectedTabIndex));
                DoPropertyChanged(nameof(SelectedTabIndexEnum));
            }
        }
        #endregion

        // service-proxy communication
        public ProxyModel Proxies => _Proxies;
        public CreatureLoginInfo CreatureLoginInfo => _CreatureLogin;
        public Guid FulfillerID => CreatureLoginInfo?.ID ?? Guid.Empty;

        // character sheet
        public CreatureModel CreatureModel => _Creature;
        public IEnumerable<PossessionInfo> Possessions => _Possessions;
        public IEnumerable<ItemSlotInfo> ItemSlots
            => _ItemSlots.OrderBy(_is => _is.ItemInfo != null ? 0 : 1).ThenBy(_is => _is.SlotType).ThenBy(_is => _is.SubType);

        // "transient" game information
        public LocalActionBudgetInfo Budget => _Budget;
        public IEnumerable<ActionInfo> Actions => _Actions;
        public PrerequisiteListModel Prerequisites => _Prerequisites;
        public NotificationVM Notifications => _Notifications;

        public LocalTurnTrackerInfo TurnTracker => _TurnTracker;
        public Visibility AbortVisibility
            => _Budget?.HeldActivity != null
            ? Visibility.Visible
            : Visibility.Collapsed;

        public FurnishingActionSet FurnishingActions
            => new FurnishingActionSet(DoAction, ActionMenuBuilder.GetFurnishingActions(Actions));

        #region public ObservableActor ObservableActor { get; set; }
        public ObservableActor ObservableActor
        {
            get => _ObservableActor;
            set
            {
                var _prev = _ObservableActor;
                _ObservableActor = value;
                DoPropertyChanged(nameof(ObservableActor));
                try
                {
                    if ((value == null) && (_prev != null) && !IsListed)
                    {
                        LogOut?.Execute(null);
                    }
                }
                catch
                {
                }
            }
        }
        #endregion

        public IEnumerable<AwarenessInfo> GetCoreInfoAwarenesses(IEnumerable<PrerequisiteInfo> preReqs)
        {
            var _resolver = Proxies.IconResolver;
            var _critterID = CreatureLoginInfo.ID.ToString();
            var _sourceAware = Proxies.IkosaProxy.Service.GetAwarenessInfo(_critterID, _critterID);
            foreach (var _info in _sourceAware)
            {
                _info.SetIconResolver(_resolver);
                yield return _info;
            }
            yield break;
        }

        #region public void ShowObservableActor()
        public void ShowObservableActor()
        {
            if (ObservableActor == null)
            {
                _Proxies?.GenerateView?.Invoke(this);
            }
            else
            {
                ObservableActor?.DoFocus();
            }
        }
        #endregion

        #region RelayCommand bindings: Initiative, Aim-Points, Target-Pointers, Target-Awarenesses
        // Ikosa Action and Initiative
        public RelayCommand<object> DoAction => _DoAction;
        public RelayCommand DoSendPrerequisites => _SendPrerequisites;
        public RelayCommand DoAbortSpan => _AbortSpan;
        public RelayCommand DoEndTurn => _EndTurn;
        public RelayCommand DoCancel => _Cancel;
        public RelayCommand<Tuple<ActorModel, ObjectInfo, Guid>> SetIdentity => _SetIdentity;
        public RelayCommand LogOut => _LogOut;
        public RelayCommand OpenView => _OpenView;
        #endregion

        // ----- model refresh from proxy -----

        #region private void SetCreatureIconResolver()
        private void SetCreatureIconResolver()
        {
            var _resolver = Proxies.IconResolver;
            if (_resolver != null)
            {
                // possessions
                foreach (var _iconInfo in Possessions.Select(_p => _p.ObjectInfo).OfType<IIconInfo>())
                {
                    _iconInfo.IconResolver = _resolver;
                }

                // item slots
                foreach (var _item in from _slot in _ItemSlots
                                      let _i = _slot.ItemInfo
                                      where _i != null
                                      select _i)
                {
                    _item.IconResolver = _resolver;
                }
            }
        }
        #endregion

        #region private void SetActionIconResolver()
        private void SetActionIconResolver()
        {
            var _resolver = Proxies.IconResolver;
            if (_resolver != null)
            {
                // action providers
                foreach (var _prov in Actions.Select(_a => _a.Provider.ProviderInfo).OfType<IIconInfo>())
                {
                    _prov.IconResolver = _resolver;
                }

                // object aim options
                foreach (var _obj in (from _act in Actions
                                      from _oList in _act.AimingModes.OfType<ObjectListAimInfo>()
                                      from _oi in _oList.ObjectInfos.OfType<IIconInfo>()
                                      select _oi))
                {
                    _obj.IconResolver = _resolver;
                }
            }
        }
        #endregion

        #region public void UpdatePrerequisites()
        public void UpdatePrerequisites()
        {
            var _preReq = Proxies.IkosaProxy.Service.GetPreRequisites(CreatureLoginInfo.ID.ToString());
            if (_preReq != null)
            {
                if (_preReq.OfType<ActionInquiryPrerequisiteInfo>().Any())
                {
                    // update actions and exclude prerequisite
                    UpdateActions();
                    if (Budget.IsInitiative && Budget.IsFocusedBudget)
                    {
                        ObservableActor?.DoFocus();
                    }
                }
                _Prerequisites = new PrerequisiteListModel(_preReq, this, CreatureLoginInfo);
                if (_Prerequisites.Items.Any()
                    && (_Prerequisites.Items.Select(_i => _i.Prerequisite).OfType<WaitReleasePrerequisiteInfo>().Count() < _Prerequisites.Items.Count))
                {
                    // if proxying a game master or no observable actor available
                    if (Proxies.IsMaster || (ObservableActor == null))
                    {
                        // ...focus the proxy window prerequisite tab
                        Proxies.NeedsAttention?.Invoke();
                    }
                    else
                    {
                        // ...otherwise focus the actor
                        ObservableActor?.DoFocus();
                    }
                    Proxies.SelectedClientSelector = this;
                    SelectedTabIndexEnum = TabIndexEnum.Prerequisites;
                }
                else if (SelectedTabIndexEnum == TabIndexEnum.Prerequisites)
                {
                    SelectedTabIndexEnum = _LastSelected;
                }
            }
            else if (SelectedTabIndexEnum == TabIndexEnum.Prerequisites)
            {
                SelectedTabIndexEnum = _LastSelected;
            }
            DoPropertyChanged(nameof(Prerequisites));
        }
        #endregion

        public void UpdateTurnTracker()
        {
            _TurnTracker = Proxies.GetLocalTurnTracker(CreatureLoginInfo.ID.ToString());
            DoPropertyChanged(nameof(TurnTracker));
        }

        public bool IsNewerSerialState()
        {
            var _state = Proxies.IkosaProxy.Service.GetSerialState();
            if (_state != _SerialState)
            {
                _SerialState = _state;
                return true;
            }
            return false;
        }

        public ulong SerialState => _SerialState;

        #region public void ClearNotifications()
        public void ClearNotifications()
        {
            _Notifications.Clear();
        }
        #endregion

        #region public void AddNotification(Notification notification)
        public void AddNotification(Notification notification)
        {
            _Notifications.AddRange(notification.Notifications.Where(_n => !(_n is RefreshNotify)));
        }
        #endregion

        #region public void UpdateCreature()
        public void UpdateCreature()
        {
            // no dispatcher required
            var _id = CreatureLoginInfo.ID.ToString();
            _ItemSlots = Proxies.IkosaProxy.Service.GetSlottedItems(_id);
            _Possessions = Proxies.IkosaProxy.Service.GetPossessionInfo(_id);
            SetCreatureIconResolver();
            DoPropertyChanged(nameof(ItemSlots));
            DoPropertyChanged(nameof(Possessions));

            if (_Creature == null)
            {
                _Creature = new CreatureModel(this, Proxies.IkosaProxy.Service.GetCreature(_id));
            }
            else
            {
                _Creature.Conformulate(Proxies.IkosaProxy.Service.GetCreature(_id));
            }
            DoPropertyChanged(nameof(CreatureModel));
        }
        #endregion

        #region public void CurrentStep(Guid stepID)
        public void CurrentStep(Guid stepID)
        {
            UpdatePrerequisites();
            if (!_Prerequisites.Items.Any())
            {
                if (SelectedTabIndexEnum == TabIndexEnum.Prerequisites)
                {
                    SelectedTabIndexEnum = _LastSelected;
                }
            }
        }
        #endregion

        #region public void UpdateActions()
        /// <summary>Update Actions and Budget from proxy</summary>
        public void UpdateActions()
        {
            var _id = CreatureLoginInfo.ID.ToString();

            // get available actions
            var _actions = Proxies.IkosaProxy.Service.GetActions(_id);
            if (_actions != null)
            {
                _Actions = _actions;
                SetActionIconResolver();
            }
            else
            {
                _Actions = new List<ActionInfo>();
            }

            // get budget
            _Budget = Proxies.IkosaProxy.Service.GetActionBudget(_id);

            ObservableActor?.UpdateActions();

            // notify UI
            DoPropertyChanged(nameof(Actions));
            DoPropertyChanged(nameof(Budget));
            DoPropertyChanged(nameof(FurnishingActions));
            DoPropertyChanged(nameof(AbortVisibility));
        }
        #endregion

        // ----- actions -----

        #region public void ClearActions()
        /// <summary>Clear Actions on Logout</summary>
        public void ClearActions()
        {
            _Actions = new List<ActionInfo>();

            // notify UI
            DoPropertyChanged(nameof(Actions));
            DoPropertyChanged(nameof(FurnishingActions));

            ObservableActor?.ClearActions();
        }
        #endregion

        #region public void PerformAction(ActivityInfo activity)
        /// <summary>Perform action via proxy, then update available action list</summary>
        /// <param name="activity"></param>
        public void PerformAction(ActivityInfo activity)
        {
            try
            {
                Proxies.IkosaProxy.Service.DoAction(activity);
                UpdateActions();
            }
            catch (Exception _ex)
            {
                Debug.WriteLine(_ex);
                MessageBox.Show(_ex.Message, @"Ikosa Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region private void DoAction_Executed(object parameter)
        private void DoAction_Executed(object parameter)
        {
            if (parameter is Tuple<ActionInfo, AimTargetInfo> _tuple)
            {
                #region One Aim Target that is Ready to Go
                // One Aim Target Ready to Go
                PerformAction(new ActivityInfo
                {
                    ActionID = _tuple.Item1.ID,
                    ActionKey = _tuple.Item1.Key,
                    ActorID = CreatureLoginInfo.ID,
                    Targets = new AimTargetInfo[] { _tuple.Item2 }
                });
                #endregion
            }
            else
            {
                if (parameter is Tuple<ActionInfo, List<AimTargetInfo>> _list)
                {
                    #region multiple aim targets that are ready to go
                    // do action with supplied targets
                    PerformAction(new ActivityInfo
                    {
                        ActionID = _list.Item1.ID,
                        ActionKey = _list.Item1.Key,
                        ActorID = CreatureLoginInfo.ID,
                        Targets = _list.Item2.ToArray()
                    });
                    #endregion
                }
                else if (parameter is ActionInfo _action)
                {
                    if (_action.AimingModes.Any() && !_action.AimingModes.All(_aim => _aim is FixedAimInfo))
                    {
                        ObservableActor?.SetActivityBuilder(_action, (activity) => PerformAction(activity));
                    }
                    else
                    {
                        #region no aiming needed, or all are fixed aims
                        // No Aiming needed, or all are Fixed Aims
                        PerformAction(new ActivityInfo
                        {
                            ActionID = _action.ID,
                            ActionKey = _action.Key,
                            ActorID = CreatureLoginInfo.ID
                        });
                        #endregion
                    }
                }
            }
            ObservableActor?.DoFocus();
        }
        #endregion

        private bool DoAction_CanExecute(object parameter)
            => parameter != null;

        #region private void LogOut_Executed()
        private void LogOut_Executed()
        {
            if ((Proxies.Actors.Count() == 1) && !_Proxies.IsMaster)
            {
                // last tracked actor
                foreach (var _actor in _Proxies.Actors)
                {
                    _actor.ObservableActor?.DoShutdown();
                    _actor.ClearActions();
                }
                Proxies.Logout();
            }
            else
            {
                // simple drop
                ObservableActor?.DoShutdown();
                ClearActions();
                _Proxies.LogoutCreature(CreatureLoginInfo.ID);
                _Proxies.SynchronizeCallbacks();
            }
        }
        #endregion

        // ----- Prerequisites -----

        #region SendPrerequisites
        private bool SendPrerequisites_CanExecute()
            => (Prerequisites?.Items.Any() ?? false)
            && (Prerequisites?.Items.All(_pre => _pre.Prerequisite.IsReady) ?? false);

        private void SendPrerequisites_Executed()
        {
            try
            {
                if (SendPrerequisites_CanExecute())
                {
                    var _ikosa = Proxies.IkosaProxy;
                    if (_ikosa != null)
                    {
                        _ikosa.Service.SetPreRequisites(Prerequisites.Items.Select(_p => _p.Prerequisite).ToArray());
                    }
                }
            }
            catch (Exception _ex)
            {
                Debug.WriteLine(_ex);
                MessageBox.Show(_ex.Message, @"Ikosa Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        // ----- Initiative Control -----

        /// <summary>True if budget is not in initiative or the budget is the focused budget</summary>
        public bool CanTakeTurn
            => (!Budget.IsInitiative || Budget.IsFocusedBudget);

        #region AbortSpan
        private bool AbortSpan_CanExecute()
            => Proxies.IsLoggedIn
            && (Budget?.HeldActivity != null)
            && Actions.Any(_a => _a.Key.Equals(@"AbortSpan", StringComparison.OrdinalIgnoreCase) && (_a.Provider?.ProviderInfo == null));

        private void AbortSpan_Executed()
        {
            try
            {
                var _abort = Actions?.FirstOrDefault(_a => _a.Key.Equals(@"AbortSpan", StringComparison.OrdinalIgnoreCase) && (_a.Provider?.ProviderInfo == null));
                if (_abort != null)
                {
                    PerformAction(new ActivityInfo
                    {
                        ActionID = _abort.ID,
                        ActionKey = _abort.Key,
                        ActorID = CreatureLoginInfo.ID
                    });
                }
            }
            catch (Exception _ex)
            {
                Debug.WriteLine(_ex);
                MessageBox.Show(_ex.Message, @"Ikosa Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region EndTurn
        private bool EndTurn_CanExecute()
            => Proxies.IsLoggedIn
            && (Budget != null)
            && (Budget.IsInitiative && Budget.IsFocusedBudget);

        private void EndTurn_Executed()
        {
            try
            {
                Proxies.IkosaProxy.Service.EndTurn(CreatureLoginInfo.ID.ToString());
            }
            catch (Exception _ex)
            {
                Debug.WriteLine(_ex);
                MessageBox.Show(_ex.Message, @"Ikosa Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Cancel
        private bool Cancel_CanExecute()
            => Proxies.IsLoggedIn;

        private void Cancel_Executed()
        {
            try
            {
                Proxies.IkosaProxy.Service.CancelAction(CreatureLoginInfo.ID.ToString());
            }
            catch (Exception _ex)
            {
                Debug.WriteLine(_ex);
                MessageBox.Show(_ex.Message, @"Ikosa Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        // ----- Set Identity -----

        #region private void SetIdentity_Executed(object parameter)
        private void SetIdentity_Executed(Tuple<ActorModel, ObjectInfo, Guid> parameter)
        {
            try
            {

                Proxies?.IkosaProxy.Service.SetActiveInfo(
                parameter.Item1.FulfillerID.ToString(),
                parameter.Item2.ID.ToString(),
                parameter.Item3.ToString());
            }
            catch (Exception _ex)
            {
                Debug.WriteLine(_ex);
                MessageBox.Show(_ex.Message, @"Ikosa Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        private bool SetIdentity_CanExecute(Tuple<ActorModel, ObjectInfo, Guid> parameter)
            => true;


        // TODO: ObservedActivities (service info also)
    }
}