using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class NeedsTurnTickMode : TickTrackerModeBase
    {
        #region ctor()
        public NeedsTurnTickMode(IsMasterModel master, LocalTurnTrackerInfo tracker)
            : base(master, tracker)
        {
            // initial ordering is tick ordering (minus round marker)
            _Ordering = new ObservableCollection<NeedsTurnTickInitiativeOrder>();
            foreach (var _tick in tracker.UpcomingTicks.Where(_t => !_t.IsRoundMarker))
            {
                _Ordering.Add(new NeedsTurnTickExistingTickOrder { TickInfo = _tick });
            }

            // teams (needs to be after ordering, which is used in refreshing)
            _Teams = new ObservableCollection<TeamGroupModel>();
            RefreshTeams();

            // commands
            _AddTop = new RelayCommand<object>(
                (ctm) =>
                {
                    Ordering.Insert(0, new NeedsTurnTickAddCreatureOrder { CreatureTrackerModel = ctm as CreatureTrackerModel });
                    RefreshTeams();
                },
                (ctm) => (ctm is CreatureTrackerModel));
            _AddAbove = new RelayCommand<object>(
                (ctm) =>
                {
                    var _idx = Ordering.IndexOf(SelectedOrder);
                    Ordering.Insert(_idx, new NeedsTurnTickAddCreatureOrder { CreatureTrackerModel = ctm as CreatureTrackerModel });
                    RefreshTeams();
                },
                (ctm) => (SelectedOrder != null) && (ctm is CreatureTrackerModel));
            _AddBelow = new RelayCommand<object>(
                (ctm) =>
                {
                    var _idx = Ordering.IndexOf(SelectedOrder);
                    Ordering.Insert(_idx + 1, new NeedsTurnTickAddCreatureOrder { CreatureTrackerModel = ctm as CreatureTrackerModel });
                    RefreshTeams();
                },
                (ctm) => (SelectedOrder != null) && (ctm is CreatureTrackerModel));
            _Remove = new RelayCommand(
                () =>
                {
                    if (SelectedOrder is NeedsTurnTickAddCreatureOrder _select)
                    {
                        Ordering.Remove(_select);
                        RefreshTeams();
                        SelectedOrder = null;
                    }
                },
                () => SelectedOrder is NeedsTurnTickAddCreatureOrder);
            _MoveUp = new RelayCommand(
                () =>
                {
                    if (SelectedOrder is NeedsTurnTickAddCreatureOrder _select)
                    {
                        var _idx = Ordering.IndexOf(_select);
                        Ordering.Remove(SelectedOrder);
                        Ordering.Insert(_idx - 1, _select);
                        SelectedOrder = _select;
                    }
                },
                () => (SelectedOrder is NeedsTurnTickAddCreatureOrder _nttaco) && (Ordering.IndexOf(_nttaco) > 0));
            _MoveDown = new RelayCommand(
                () =>
                {
                    if (SelectedOrder is NeedsTurnTickAddCreatureOrder _select)
                    {
                        var _idx = Ordering.IndexOf(_select);
                        Ordering.Remove(SelectedOrder);
                        Ordering.Insert(_idx + 1, _select);
                        SelectedOrder = _select;
                    }
                },
                () => (SelectedOrder is NeedsTurnTickAddCreatureOrder _nttaco) && (Ordering.IndexOf(_nttaco) < (Ordering.Count - 1)));
            _Commit = new RelayCommand(
                () =>
                {
                    var _ids = Ordering.Select(_o => _o.ID).ToArray();
                    MasterModel?.Proxies?.MasterProxy?.Service?.TurnTrackerAdd(_ids);
                    RefreshTeams();
                },
                () => !Teams.Any(_t => _t.Creatures.Any(_c => _c.NeedsTurnTick)));
        }
        #endregion

        #region data
        private readonly ObservableCollection<TeamGroupModel> _Teams;
        private readonly ObservableCollection<NeedsTurnTickInitiativeOrder> _Ordering;
        private NeedsTurnTickInitiativeOrder _Selected;
        private readonly RelayCommand<object> _AddTop;
        private readonly RelayCommand<object> _AddAbove;
        private readonly RelayCommand<object> _AddBelow;
        private readonly RelayCommand _Remove;
        private readonly RelayCommand _MoveUp;
        private readonly RelayCommand _MoveDown;
        private readonly RelayCommand _Commit;
        #endregion

        #region private void RefreshTeams()
        private void RefreshTeams()
        {
            var _budgets = Tracker.AllBudgets;
            (bool inTracker, bool needsTurnTick, bool isInitiative) _getFlags(CreatureTrackerInfo creatureTrackerInfo)
                => (_budgets.TryGetValue(creatureTrackerInfo.ID, out var _b))
                ? (true,
                _b.BudgetItems.OfType<AdjunctBudgetInfo>().Any(_ab => _ab.Adjunct.Message.Equals(@"Uzi.Ikosa.Tactical.NeedsTurnTick", StringComparison.OrdinalIgnoreCase)),
                _b.IsInitiative)
                : (false, false, false);

            bool _isOrdering(Guid creatureID)
                => Ordering.OfType<NeedsTurnTickAddCreatureOrder>().Any(_nttaco => _nttaco.CreatureTrackerModel.ID == creatureID);

            var _svcRosters = (from _team in MasterModel.Proxies.GetTeamRosters()
                               let _tgm = new
                               {
                                   TeamGroupInfo = _team,
                                   IsExpanded = false,
                                   Creatures = (from _cti in _team.PrimaryCreatures
                                                let _flags = _getFlags(_cti)
                                                where !_flags.isInitiative && !_isOrdering(_cti.ID)
                                                orderby _flags.needsTurnTick ? 0 : 1, _flags.inTracker ? 0 : 1, _cti.CreatureLoginInfo.Name
                                                select new CreatureTrackerModel
                                                {
                                                    CreatureTrackerInfo = _cti,
                                                    IsInTracker = _flags.inTracker,
                                                    IsInitiative = false,
                                                    NeedsTurnTick = _flags.needsTurnTick,
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
        }
        #endregion

        public ObservableCollection<TeamGroupModel> Teams => _Teams;
        public ObservableCollection<NeedsTurnTickInitiativeOrder> Ordering => _Ordering;

        public NeedsTurnTickInitiativeOrder SelectedOrder
        {
            get => _Selected;
            set
            {
                _Selected = value;
                DoPropertyChanged(nameof(SelectedOrder));
            }
        }

        public RelayCommand<object> DoAddTop => _AddTop;
        public RelayCommand<object> DoAddAbove => _AddAbove;
        public RelayCommand<object> DoAddBelow => _AddBelow;
        public RelayCommand DoRemove => _Remove;
        public RelayCommand DoMoveUp => _MoveUp;
        public RelayCommand DoMoveDown => _MoveDown;
        public RelayCommand DoCommit => _Commit;

        protected override void OnRefreshTracker()
        {
            base.OnRefreshTracker();
            MasterModel?.Proxies?.DoMasterDispatch(() => RefreshTeams());
        }
    }
}
