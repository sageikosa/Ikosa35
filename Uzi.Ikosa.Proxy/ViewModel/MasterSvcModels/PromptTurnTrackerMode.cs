using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class PromptTurnTrackerMode : TickTrackerModeBase
    {
        #region ctor()
        public PromptTurnTrackerMode(IsMasterModel master, LocalTurnTrackerInfo tracker)
            : base(master, tracker)
        {
            _PromptModel = master.Prerequisites.Items.OfType<PromptTurnTrackerPrerequisiteModel>().FirstOrDefault();
            _Initial = new ObservableCollection<CreatureTrackerModel>();

            // teams (needs to be after ordering, which is used in refreshing)
            _StandDownGroups = new ObservableCollection<StandDownGroupInfo>();
            _Teams = new ObservableCollection<TeamGroupModel>();
            RefreshTeams();

            // commands
            _Add = new RelayCommand<object>(
                (adder) =>
                {
                    if (adder is CreatureTrackerModel _ctm)
                    {
                        Initial.Add(adder as CreatureTrackerModel);
                    }
                    else if (adder is CreatureGroupModel<TeamGroupInfo, CreatureTrackerModel> _tgm)
                    {
                        foreach (var _member in _tgm.Creatures)
                        {
                            Initial.Add(_member);
                        }
                    }
                    RefreshTeams();
                },
                (adder) => (adder is CreatureTrackerModel) || (adder is CreatureGroupModel<TeamGroupInfo, CreatureTrackerModel>));
            _Remove = new RelayCommand<object>(
                (ctm) =>
                {
                    if (Initial.Contains(ctm as CreatureTrackerModel))
                    {
                        Initial.Remove(ctm as CreatureTrackerModel);
                        RefreshTeams();
                    }
                },
                (ctm) => (ctm is CreatureTrackerModel _ctm) && !_ctm.NeedsTurnTick);
            _Commit = new RelayCommand(
                () =>
                {
                    var _ids = Initial.Select(_o => _o.ID).ToArray();
                    MasterModel?.Proxies?.MasterProxy?.Service?.TurnTrackerStart(_ids);
                    RefreshTeams();
                },
                () => !Teams.Any(_t => _t.Creatures.Any(_c => _c.NeedsTurnTick)));
            _Cancel = new RelayCommand(
                () =>
                {
                    if (_PromptModel?.PromptTurnTrackerPrerequisiteInfo is PromptTurnTrackerPrerequisiteInfo _prompt)
                    {
                        _prompt.Done = true;
                        MasterModel?.Proxies.IkosaProxy.Service.SetPreRequisites(new[] { _prompt });
                    }
                    _PromptModel.IsSent = true;
                });
            _DoStandDown = new RelayCommand(
                () =>
                {
                    MasterModel?.Proxies.MasterProxy.Service.AddToStandDownGroup(
                        SelectedStandDownGroup?.Guid ?? Guid.Empty,
                        StandDownGroupName, _Initial.Select(_i => _i.ID).ToArray());
                    _Cancel?.Execute(null);
                    RefreshTeams();
                },
                () => !string.IsNullOrWhiteSpace(_StandDownGroupName) && (_Initial.Count > 1));
        }
        #endregion

        #region data
        private readonly ObservableCollection<TeamGroupModel> _Teams;
        private readonly ObservableCollection<CreatureTrackerModel> _Initial;
        private readonly PromptTurnTrackerPrerequisiteModel _PromptModel;
        private readonly RelayCommand<object> _Add;
        private readonly RelayCommand<object> _Remove;
        private readonly RelayCommand _Commit;
        private readonly RelayCommand _Cancel;
        private readonly RelayCommand _DoStandDown;
        private string _StandDownGroupName;
        private readonly ObservableCollection<StandDownGroupInfo> _StandDownGroups;
        private StandDownGroupInfo _SelectedGroup;
        #endregion

        #region private void RefreshTeams()
        private void RefreshTeams()
        {
            var _triggered = PromptTurnTrackerPrerequisiteModel?.PromptTurnTrackerPrerequisiteInfo?.Triggered ?? new List<Guid>();
            var _budgets = Tracker.AllBudgets;
            (bool inTracker, bool triggered) _getFlags(CreatureTrackerInfo creatureTrackerInfo)
                => (_budgets.TryGetValue(creatureTrackerInfo.ID, out var _b), _triggered.Contains(creatureTrackerInfo.ID));

            bool _gathered(Guid creatureID)
                => Initial.Any(_ctm => _ctm.ID == creatureID);

            // catch any thing triggered that needs to always be added to initial list
            var _rawRosters = MasterModel.Proxies.GetTeamRosters();
            foreach (var (_critter, _inTracker) in (from _team in _rawRosters
                                                    from _c in _team.PrimaryCreatures
                                                    let _flags = _getFlags(_c)
                                                    where _flags.triggered
                                                    && !Initial.Any(_i => _i.ID == _c.ID)
                                                    select (Critter: _c, InTracker: _flags.inTracker)).ToList())
            {
                Initial.Add(new CreatureTrackerModel
                {
                    CreatureTrackerInfo = _critter,
                    IsInTracker = _inTracker,
                    IsInitiative = false,
                    TickTrackerMode = this,
                    NeedsTurnTick = true
                });
            }

            // list everything else
            var _svcRosters = (from _team in _rawRosters
                               let _tgm = new
                               {
                                   TeamGroupInfo = _team,
                                   IsExpanded = false,
                                   Creatures = (from _cti in _team.PrimaryCreatures
                                                let _flags = _getFlags(_cti)
                                                where !_gathered(_cti.ID)
                                                orderby _flags.triggered ? 0 : 1, _flags.inTracker ? 0 : 1, _cti.CreatureLoginInfo.Name
                                                select new CreatureTrackerModel
                                                {
                                                    CreatureTrackerInfo = _cti,
                                                    IsInTracker = _flags.inTracker,
                                                    IsInitiative = false,
                                                    NeedsTurnTick = _flags.triggered,
                                                    TickTrackerMode = this
                                                }).ToList()
                               }
                               where _tgm.Creatures.Any()
                               select _tgm).ToList();

            // teams not in svc
            foreach (var _roster in (from _t in Teams
                                     where !_svcRosters.Any(_st => _st.TeamGroupInfo.ID == _t.TeamGroupInfo.ID)
                                     select _t).ToList())
            {
                Teams.Remove(_roster);
            }

            // conformulate update team-group-models
            foreach (var _roster in (from _t in Teams
                                     join _st in _svcRosters
                                     on _t.TeamGroupInfo.ID equals _st.TeamGroupInfo.ID
                                     select new { Team = _t, SvcTeam = _st }).ToList())
            {
                _roster.Team.Conformulate(_roster.SvcTeam.Creatures);
            }

            // conformulate add team-group-models
            var _addOnly = Teams.Count == 0;
            foreach (var _roster in (from _st in _svcRosters
                                     where !Teams.Any(_t => _st.TeamGroupInfo.ID == _t.TeamGroupInfo.ID)
                                     select _st).ToList())
            {
                var _tgm = new TeamGroupModel
                {
                    TickTrackerMode = this,
                    TeamGroupInfo = _roster.TeamGroupInfo,
                    IsExpanded = _roster.Creatures.Any(_c => _c.NeedsTurnTick)
                };
                _tgm.Conformulate(_roster.Creatures);
                if (_addOnly)
                {
                    Teams.Add(_tgm);
                }
                else
                {
                    Teams.Insert(Teams.Union(_tgm.ToEnumerable()).OrderBy(_t => _t.TeamGroupInfo.Name).ToList().IndexOf(_tgm), _tgm);
                }
            }
            MasterModel?.RefreshStandDownGroups();
        }
        #endregion

        #region public void RefreshStandDownGroups(List<StandDownGroupInfo> standDowns)
        public void RefreshStandDownGroups(List<StandDownGroupInfo> standDowns)
        {
            // stand down groups
            var _standDowns = standDowns.OrderBy(_g => _g.GroupName);
            foreach (var _group in (from _sdg in StandDownGroups
                                    where !_standDowns.Any(_s => _s.Guid == _sdg.Guid)
                                    select _sdg).ToList())
            {
                StandDownGroups.Remove(_group);
            }
            foreach (var _group in (from _sdg in _standDowns
                                    where !StandDownGroups.Any(_s => _s.Guid == _sdg.Guid)
                                    select _sdg).ToList())
            {
                StandDownGroups.Add(_group);
            }
            if (_SelectedGroup != null)
            {
                var _select = _StandDownGroups.FirstOrDefault(_g => _g.Guid == _SelectedGroup.Guid);
                if (_select != null)
                {
                    SelectedStandDownGroup = _select;
                }
            }
        }
        #endregion

        public PromptTurnTrackerPrerequisiteModel PromptTurnTrackerPrerequisiteModel => _PromptModel;
        public ObservableCollection<TeamGroupModel> Teams => _Teams;
        public ObservableCollection<CreatureTrackerModel> Initial => _Initial;

        public ObservableCollection<StandDownGroupInfo> StandDownGroups => _StandDownGroups;

        public RelayCommand<object> DoAdd => _Add;
        public RelayCommand<object> DoRemove => _Remove;
        public RelayCommand DoCommit => _Commit;
        public RelayCommand DoCancel => _Cancel;
        public RelayCommand DoStandDown => _DoStandDown;

        public string StandDownGroupName
        {
            get => _StandDownGroupName;
            set
            {
                _StandDownGroupName = value;
                DoPropertyChanged(nameof(StandDownGroupName));
            }
        }

        public bool IsGroupNameEnabled
            => (_SelectedGroup?.Guid ?? Guid.Empty) == Guid.Empty;

        public StandDownGroupInfo SelectedStandDownGroup
        {
            get => _SelectedGroup;
            set
            {
                _SelectedGroup = value;
                StandDownGroupName = _SelectedGroup?.GroupName ?? string.Empty;
                DoPropertyChanged(nameof(SelectedStandDownGroup));
                DoPropertyChanged(nameof(IsGroupNameEnabled));
            }
        }

        protected override void OnRefreshTracker()
        {
            base.OnRefreshTracker();
            MasterModel?.Proxies?.DoMasterDispatch(() => RefreshTeams());
        }
    }
}
