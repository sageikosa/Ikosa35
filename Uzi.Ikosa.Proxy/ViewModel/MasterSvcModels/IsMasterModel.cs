using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Contracts.Host;
using Uzi.Visualize;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class IsMasterModel : ViewModelBase, IPrerequisiteProxy
    {
        #region ctor()
        public IsMasterModel(ProxyModel proxies)
        {
            _Proxies = proxies;
            _SendPrerequisites = new RelayCommand(SendPrerequisites_Executed);
            _SetFlowState = new RelayCommand<FlowState>(SetFlowState_Execute);
            _TabIndex = 0;
            _TickTrackerModeLock = new object();

            // stand down groups
            _StandDownGroups = new ObservableCollection<StandDownGroupModel>();
            _StandDownAvailable = new ObservableCollection<TeamGroupModel>();
            RefreshStandDownGroups();

            _ToggleAutoTimeTick = new RelayCommand(
                () =>
                {
                    try { Proxies.MasterProxy.Service.SetIsTimeTickAuto(!TickTrackerMode.Tracker.IsAutoTimeTick); UpdateTurnTracker(); }
                    catch (Exception _ex) { HandleException(_ex); }
                },
                () => TickTrackerMode is TimeTickMode); // only allow toggle time on time tick mode (not timeline)
            _TogglePause = new RelayCommand(() =>
            {
                try { Proxies.MasterProxy.Service.SetPause(!Proxies.IsPaused); }
                catch (Exception _ex) { HandleException(_ex); }
            });
            _PushTimeTick = new RelayCommand(() =>
            {
                try { Proxies.MasterProxy.Service.PushTimeTick(); }
                catch (Exception _ex) { HandleException(_ex); }
            });

            _StandDownGroupAdd = new RelayCommand<object>(
                (target) =>
                {
                    try
                    {
                        if (target is CreatureTrackerModel _ctm)
                        {
                            Proxies?.MasterProxy?.Service
                                .AddToStandDownGroup(SelectedStandDownGroup.StandDownGroupInfo.Guid,
                                SelectedStandDownGroup.StandDownGroupInfo.GroupName,
                                new[] { _ctm.ID });
                        }
                        else if (target is TeamGroupModel _tgm)
                        {
                            Proxies?.MasterProxy?.Service
                                .AddToStandDownGroup(SelectedStandDownGroup.StandDownGroupInfo.Guid,
                                SelectedStandDownGroup.StandDownGroupInfo.GroupName,
                                _tgm.Creatures.Select(_c => _c.ID).ToArray());
                        }
                        RefreshStandDownGroups();
                        RefreshSelectedStandDownGroup();
                    }
                    catch (Exception _ex) { HandleException(_ex); }
                },
                (target) =>
                    (SelectedStandDownGroup != null)
                    && ((target is CreatureTrackerModel) || (target is TeamGroupModel)));
            _StandDownGroupRemove = new RelayCommand<object>(
                (target) =>
                {
                    try
                    {
                        if (target is CreatureLoginInfo _cli)
                        {
                            Proxies?.MasterProxy?.Service
                                .RemoveFromStandDownGroup(SelectedStandDownGroup.StandDownGroupInfo.Guid, new[] { _cli.ID });
                        }
                        else if (target is StandDownGroupModel _sdg)
                        {
                            Proxies?.MasterProxy?.Service
                                .RemoveFromStandDownGroup(_sdg.StandDownGroupInfo.Guid, _sdg.Creatures.Select(_c => _c.ID).ToArray());
                        }
                        RefreshStandDownGroups();
                        RefreshSelectedStandDownGroup();
                    }
                    catch (Exception _ex) { HandleException(_ex); }
                },
                (target) =>
                    (SelectedStandDownGroup != null)
                    && ((target is CreatureLoginInfo) || (target is StandDownGroupModel)));

            _AllowAdvance = new RelayCommand<CreatureLoginInfo>((cli) => AllowAdvance(cli.ID), (cli) => cli != null);
            _RevokeAdvance = new RelayCommand<CreatureLoginInfo>((cli) => RevokeAdvance(cli.ID), (cli) => cli != null);
            UpdatePrerequisites();
            _TickTrackerMode = TickTrackerModeBase.GetTrackerMode(this, proxies.GetLocalTurnTracker(null));
            _SerialState = proxies.IkosaProxy.Service.GetSerialState();
        }
        #endregion

        #region data
        private readonly ProxyModel _Proxies;
        private MasterLog _MasterLog;
        private readonly RelayCommand _TogglePause;
        private readonly RelayCommand _PushTimeTick;
        private readonly RelayCommand _ToggleAutoTimeTick;
        private readonly RelayCommand<FlowState> _SetFlowState;
        private readonly object _TickTrackerModeLock;
        private TickTrackerModeBase _TickTrackerMode;
        private ulong _SerialState;

        // advancement
        private readonly RelayCommand<CreatureLoginInfo> _AllowAdvance;
        private readonly RelayCommand<CreatureLoginInfo> _RevokeAdvance;
        private List<CreatureLoginInfo> _AdvanceableCreatures;
        private List<CreatureLoginInfo> _AdvancingCreatures;

        // stand down groups
        private readonly RelayCommand<object> _StandDownGroupAdd;
        private readonly RelayCommand<object> _StandDownGroupRemove;
        private readonly ObservableCollection<StandDownGroupModel> _StandDownGroups;
        private readonly ObservableCollection<TeamGroupModel> _StandDownAvailable;
        private StandDownGroupModel _SelectedGroup;

        // prerequisites
        private readonly RelayCommand _SendPrerequisites;
        private PrerequisiteListModel _Prerequisites = null;

        // mvvm tab control
        private int _TabIndex;
        private TabIndexEnum _LastSelected = TabIndexEnum.TurnTracker;
        #endregion

        public Guid FulfillerID => Guid.Empty;
        public ProxyModel Proxies => _Proxies;
        public TickTrackerModeBase TickTrackerMode => _TickTrackerMode;

        public RelayCommand DoTogglePause => _TogglePause;
        public RelayCommand DoPushTimeTick => _PushTimeTick;
        public RelayCommand<FlowState> SetFlowState => _SetFlowState;
        public RelayCommand DoToggleAutoTimeTick => _ToggleAutoTimeTick;

        // ---------- advancement control

        public RelayCommand<CreatureLoginInfo> AllowAdvancement => _AllowAdvance;
        public RelayCommand<CreatureLoginInfo> RevokeAdvancement => _RevokeAdvance;

        #region public IEnumerable<CreatureLoginInfo> AdvanceableCreatures { get; }
        public IEnumerable<CreatureLoginInfo> AdvanceableCreatures
        {
            get
            {
                if (_AdvanceableCreatures == null)
                {
                    _AdvanceableCreatures = Proxies.MasterProxy.Service.GetAdvancementCreatures(false)
                        .Select(_cli => _Proxies.AddPortrait(_cli))
                        .ToList();
                }
                return _AdvanceableCreatures.Select(_cli => _cli).ToList();
            }
        }
        #endregion

        #region public IEnumerable<CreatureLoginInfo> AdvancingCreatures { get; }
        public IEnumerable<CreatureLoginInfo> AdvancingCreatures
        {
            get
            {
                if (_AdvancingCreatures == null)
                {
                    _AdvancingCreatures = Proxies.MasterProxy.Service.GetAdvancementCreatures(true)
                        .Select(_cli => _Proxies.AddPortrait(_cli))
                        .ToList();
                }
                return _AdvancingCreatures.Select(_cli => _cli).ToList();
            }
        }
        #endregion

        #region Advancement RelayCommands
        private void AllowAdvance(Guid id)
        {
            Proxies?.MasterProxy.Service.AllowCreatureAdvancement(id);
            _AdvanceableCreatures = null;
            _AdvancingCreatures = null;
            DoPropertyChanged(nameof(AdvanceableCreatures));
            DoPropertyChanged(nameof(AdvancingCreatures));
        }

        private void RevokeAdvance(Guid id)
        {
            Proxies?.MasterProxy.Service.RevokeCreatureAdvancement(id);
            _AdvanceableCreatures = null;
            _AdvancingCreatures = null;
            DoPropertyChanged(nameof(AdvanceableCreatures));
            DoPropertyChanged(nameof(AdvancingCreatures));
        }
        #endregion

        // ---------- stand-down group control

        public RelayCommand<object> DoStandDownGroupAdd => _StandDownGroupAdd;
        public RelayCommand<object> DoStandDownGroupRemove => _StandDownGroupRemove;

        public ObservableCollection<StandDownGroupModel> StandDownGroups => _StandDownGroups;
        public ObservableCollection<TeamGroupModel> StandDownAvailable => _StandDownAvailable;


        #region public MasterLog MasterLog { get; set; }
        public MasterLog MasterLog
        {
            get => _MasterLog;
            set
            {
                var _prev = _MasterLog;
                _MasterLog = value;
                DoPropertyChanged(nameof(MasterLog));
            }
        }
        #endregion

        public void DoMasterLogOutput(SysNotify notify)
        {
            MasterLog?.Dispatcher.Invoke(new Action(() =>
            {
                MasterLog?.Notifies.Add(new SysNotifyVM(MasterLog?.GetNextNotifyID() ?? -1, notify, null));
            }));
        }

        #region public StandDownGroupModel SelectedStandDownGroup { get; set; }
        public StandDownGroupModel SelectedStandDownGroup
        {
            get => _SelectedGroup;
            set
            {
                if (_SelectedGroup != value)
                {
                    Proxies?.DoMasterDispatch(() =>
                    {
                        // clear available
                        _StandDownAvailable.Clear();

                        // see if it needs to be filled
                        _SelectedGroup = value;
                        if (_SelectedGroup != null)
                        {
                            var _teams = Proxies?.GetTeamRosters();
                            var _budgets = TickTrackerMode?.Tracker.AllBudgets;

                            // conformulate add team-group-models
                            // selecting a new group from the list, so everything's an add
                            if ((_teams != null) && (_budgets != null))
                            {
                                var _available = (from _team in _teams
                                                  let _tgm = new
                                                  {
                                                      TeamGroupInfo = _team,
                                                      IsExpanded = true,
                                                      Creatures = (from _cti in _team.PrimaryCreatures
                                                                   where !value.Creatures.Any(_c => _c.ID == _cti.ID)
                                                                   select new CreatureTrackerModel
                                                                   {
                                                                       CreatureTrackerInfo = _cti,
                                                                       IsInTracker = _budgets.ContainsKey(_cti.ID)
                                                                   }).ToList()
                                                  }
                                                  where _tgm.Creatures.Any()
                                                  select _tgm).ToList();
                                foreach (var _roster in (from _st in _available
                                                         where !StandDownAvailable.Any(_t => _st.TeamGroupInfo.ID == _t.TeamGroupInfo.ID)
                                                         select _st).ToList())
                                {
                                    var _tgm = new TeamGroupModel
                                    {
                                        TeamGroupInfo = _roster.TeamGroupInfo,
                                        IsExpanded = _roster.Creatures.Any(_c => _c.NeedsTurnTick)
                                    };
                                    _tgm.Conformulate(_roster.Creatures);
                                    StandDownAvailable.Add(_tgm);
                                }
                            }
                        }
                        DoPropertyChanged(nameof(SelectedStandDownGroup));
                    });
                }
            }
        }
        #endregion

        #region public void RefreshStandDownGroups()
        public void RefreshStandDownGroups()
        {
            var _standDowns = Proxies.MasterProxy.Service.GetStandDownGroups().OrderBy(_g => _g.GroupName).ToList();
            Proxies?.DoMasterDispatch(() =>
            {
                foreach (var _group in (from _sdg in StandDownGroups
                                        where !_standDowns.Any(_s => _s.Guid == _sdg.StandDownGroupInfo.Guid)
                                        select _sdg).ToList())
                {
                    // remove missing
                    StandDownGroups.Remove(_group);
                }
                foreach (var _group in (from _s in _standDowns
                                        join _sdg in StandDownGroups
                                        on _s.Guid equals _sdg.StandDownGroupInfo.Guid
                                        select new { Group = _sdg, SvcGroup = _s }).ToList())
                {
                    // conformulate
                    _group.Group.Conformulate(_group.SvcGroup.Creatures, Proxies);
                }
                foreach (var _group in (from _s in _standDowns
                                        where !StandDownGroups.Any(_sdg => _sdg.StandDownGroupInfo.Guid == _s.Guid)
                                        select _s).ToList())
                {
                    var _sgm = new StandDownGroupModel
                    {
                        StandDownGroupInfo = _group
                    };
                    _sgm.Conformulate(_group.Creatures, Proxies);

                    // add new
                    StandDownGroups.Add(_sgm);
                }
                if (_SelectedGroup != null)
                {
                    SelectedStandDownGroup = _StandDownGroups.FirstOrDefault(_g => _g.StandDownGroupInfo.Guid == _SelectedGroup.StandDownGroupInfo.Guid);
                }

                // NOTE: if more modes need this, consider an interface definition instead
                if (TickTrackerMode is PromptTurnTrackerMode _prompter)
                {
                    // at some point, might need different dispatchers, so keeping this isolated
                    Proxies.DoMasterDispatch(() =>
                    {
                        _prompter.RefreshStandDownGroups(_standDowns);
                    });
                }
                DoPropertyChanged(nameof(StandDownGroups));
            });
        }
        #endregion

        #region private void RefreshSelectedStandDownGroup()
        private void RefreshSelectedStandDownGroup()
        {
            var _selected = SelectedStandDownGroup;
            if (_selected != null)
            {
                Proxies?.DoMasterDispatch(() =>
                {
                    var _teams = Proxies?.GetTeamRosters();
                    var _budgets = TickTrackerMode?.Tracker.AllBudgets;

                    // conformulate add team-group-models
                    // selecting a new group from the list, so everything's an add
                    if ((_teams != null) && (_budgets != null))
                    {
                        // those not in the group are available
                        var _available = (from _team in _teams
                                          let _tgm = new
                                          {
                                              TeamGroupInfo = _team,
                                              IsExpanded = true,
                                              Creatures = (from _cti in _team.PrimaryCreatures
                                                           where !_selected.Creatures.Any(_c => _c.ID == _cti.ID)
                                                           select new CreatureTrackerModel
                                                           {
                                                               CreatureTrackerInfo = _cti,
                                                               IsInTracker = _budgets.ContainsKey(_cti.ID)
                                                           }).ToList()
                                          }
                                          where _tgm.Creatures.Any()
                                          select _tgm).ToList();

                        // teams not in available
                        foreach (var _roster in (from _sda in StandDownAvailable
                                                 where !_available.Any(_avail => _avail.TeamGroupInfo.ID == _sda.TeamGroupInfo.ID)
                                                 select _sda).ToList())
                        {
                            StandDownAvailable.Remove(_roster);
                        }

                        // conformulate update team-group-models
                        foreach (var _roster in (from _sda in StandDownAvailable
                                                 join _avail in _available
                                                 on _sda.TeamGroupInfo.ID equals _avail.TeamGroupInfo.ID
                                                 select new { Team = _sda, SvcTeam = _avail }).ToList())
                        {
                            _roster.Team.Conformulate(_roster.SvcTeam.Creatures);
                        }

                        // teams to be added
                        var _addOnly = StandDownAvailable.Count == 0;
                        foreach (var _roster in (from _avail in _available
                                                 where !StandDownAvailable.Any(_sda => _avail.TeamGroupInfo.ID == _sda.TeamGroupInfo.ID)
                                                 select _avail).ToList())
                        {
                            var _tgm = new TeamGroupModel
                            {
                                TeamGroupInfo = _roster.TeamGroupInfo,
                                IsExpanded = _roster.Creatures.Any(_c => _c.NeedsTurnTick)
                            };
                            _tgm.Conformulate(_roster.Creatures);
                            if (_addOnly)
                            {
                                StandDownAvailable.Add(_tgm);
                            }
                            else
                            {
                                StandDownAvailable.Insert(StandDownAvailable.Union(_tgm.ToEnumerable()).OrderBy(_t => _t.TeamGroupInfo.Name).ToList().IndexOf(_tgm), _tgm);
                            }
                        }
                    }
                    DoPropertyChanged(nameof(SelectedStandDownGroup));
                });
            }
        }
        #endregion

        // ---------- prerequisite control

        public RelayCommand DoSendPrerequisites => _SendPrerequisites;
        public PrerequisiteListModel Prerequisites => _Prerequisites;

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
                    Proxies.IkosaProxy?.Service
                        .SetPreRequisites(Prerequisites.Items.Select(_p => _p.Prerequisite).ToArray());
                }
            }
            catch (Exception _ex)
            {
                Debug.WriteLine(_ex);
                MessageBox.Show(_ex.Message, @"Ikosa Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region SetFlowState
        private bool SetFlowState_CanExecute(FlowState flowState)
           => true;

        private void SetFlowState_Execute(FlowState flowState)
        {
            try
            {
                if (SetFlowState_CanExecute(flowState))
                {
                    Proxies.MasterProxy?.Service
                        .SetFlowState(flowState);
                }
            }
            catch (Exception _ex)
            {
                HandleException(_ex);
            }
        }
        #endregion

        private void HandleException(Exception except)
        {
            Debug.WriteLine(except);
            MessageBox.Show(except.Message, @"Ikosa Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #region public int SelectedTabIndex { get; set; }
        public enum TabIndexEnum { TurnTracker, Prerequisites, StandDown, Advancement, Log }

        public TabIndexEnum SelectedTabIndexEnum
        {
            get => (TabIndexEnum)_TabIndex;
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

        #region public void UpdatePrerequisites()
        public void UpdatePrerequisites()
        {
            var _preReq = Proxies.IkosaProxy.Service.GetPreRequisites(string.Empty);
            if (_preReq != null)
            {
                _Prerequisites = new PrerequisiteListModel(_preReq, this, _Proxies.GetAllCreatures());
                if (_Prerequisites.Items.Any()
                    && (_Prerequisites.Items.Select(_i => _i.Prerequisite).OfType<WaitReleasePrerequisiteInfo>().Count() < _Prerequisites.Items.Count))
                {
                    Proxies.NeedsAttention?.Invoke();
                    Proxies.SelectedClientSelector = this;
                    if (_Prerequisites.Items.OfType<PromptTurnTrackerPrerequisiteModel>().Any())
                        SelectedTabIndexEnum = TabIndexEnum.TurnTracker;
                    else
                        SelectedTabIndexEnum = TabIndexEnum.Prerequisites;
                }
                else if (SelectedTabIndexEnum == TabIndexEnum.Prerequisites)
                {
                    SelectedTabIndexEnum = _LastSelected;
                }
            }
            DoPropertyChanged(nameof(Prerequisites));
        }
        #endregion

        public void UpdateTurnTracker()
        {
            var _changed = false;
            lock (_TickTrackerModeLock)
            {
                var _newMode = TickTrackerModeBase.GetTrackerMode(this, Proxies.GetLocalTurnTracker(null));
                _changed = _newMode != _TickTrackerMode;
                if (_changed)
                {
                    _TickTrackerMode?.LeaveMode(_newMode);
                    _TickTrackerMode = _newMode;
                }
            }
            if (_changed)
                DoPropertyChanged(nameof(TickTrackerMode));
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

        public IEnumerable<AwarenessInfo> GetCoreInfoAwarenesses(IEnumerable<PrerequisiteInfo> preReqs)
        {
            var _resolver = Proxies.IconResolver;
            var _guids = (from _cspi in preReqs.OfType<CoreSelectPrerequisiteInfo>()
                          from _c in _cspi.IDs
                          select _c.ToString()).ToArray();
            var _masterAware = Proxies.IkosaProxy.Service.GetMasterAwarenesses(_guids);
            foreach (var _info in _masterAware)
            {
                _info.SetIconResolver(_resolver);
                yield return _info;
            }
            yield break;
        }
    }
}
