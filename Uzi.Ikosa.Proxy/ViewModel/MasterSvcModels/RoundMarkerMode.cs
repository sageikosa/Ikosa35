using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class RoundMarkerMode : TickTrackerModeBase
    {
        #region ctor()
        public RoundMarkerMode(IsMasterModel master, LocalTurnTrackerInfo tracker)
            : base(master, tracker)
        {
            _NeedsIDs = new HashSet<Guid>();
            _Teams = new ObservableCollection<TeamGroupModel>();
            _Needs = new ObservableCollection<TeamGroupModel>();
            RefreshTeams();

            _DoControl = new RelayCommand<CreatureTrackerInfo>((critter) =>
            {
                var _proxies = MasterModel.Proxies;
                var _actor = _proxies.Actors.FirstOrDefault(_a => _a.CreatureLoginInfo.ID == critter.CreatureLoginInfo.ID);
                if (_actor != null)
                {
                    // found actor in list, show it
                    _actor.ShowObservableActor();
                }
                else
                {
                    // did not, so login and generate view
                    _proxies.LoginCreature(critter.CreatureLoginInfo,
                        (found) => _proxies.GenerateView?.Invoke(new ActorModel(_proxies, found, false)),
                        (m) => _proxies.GenerateLog?.Invoke(m));
                }
            });
            _DoNeedsTurnTick = new RelayCommand<CreatureTrackerInfo>((critter) =>
            {
                if (!_NeedsIDs.Contains(critter.ID))
                    _NeedsIDs.Add(critter.ID);
                RefreshTeams();
            });
            _DoNeedsTeamTicks = new RelayCommand<TeamGroupModel>((team) =>
            {
                foreach (var _member in team.Creatures.Where(_c => !_c.NeedsTurnTick && !_c.IsInitiative))
                {
                    if (!_NeedsIDs.Contains(_member.ID))
                        _NeedsIDs.Add(_member.ID);
                }
                RefreshTeams();
            });
            _DoRemoveNeedsTurnTick = new RelayCommand<CreatureTrackerInfo>((critter) =>
            {
                if (_NeedsIDs.Contains(critter.ID))
                    _NeedsIDs.Remove(critter.ID);
                RefreshTeams();
            });
            _DoCommitNeedsTurnTick = new RelayCommand(() =>
            {
                foreach (var _id in _NeedsIDs)
                {
                    MasterModel.Proxies.MasterProxy.Service.AddNeedsTurnTick(_id);
                }
                _NeedsIDs.Clear();
                MasterModel.IsNewerSerialState();
                MasterModel.UpdateTurnTracker();
            });

            _DoStandDown = new RelayCommand(
                () =>
                {
                    MasterModel.Proxies.MasterProxy.Service.TurnTrackerStop(_StandDownGroupName);
                },
                () => !string.IsNullOrWhiteSpace(_StandDownGroupName));
        }
        #endregion

        #region data
        private readonly ObservableCollection<TeamGroupModel> _Teams;
        private readonly HashSet<Guid> _NeedsIDs;
        private readonly ObservableCollection<TeamGroupModel> _Needs;
        private readonly RelayCommand<CreatureTrackerInfo> _DoControl;
        private readonly RelayCommand<CreatureTrackerInfo> _DoNeedsTurnTick;
        private readonly RelayCommand<TeamGroupModel> _DoNeedsTeamTicks;
        private readonly RelayCommand<CreatureTrackerInfo> _DoRemoveNeedsTurnTick;
        private readonly RelayCommand _DoCommitNeedsTurnTick;
        private readonly RelayCommand _DoStandDown;
        private string _StandDownGroupName;
        #endregion

        public RelayCommand<CreatureTrackerInfo> DoControl => _DoControl;
        public RelayCommand<CreatureTrackerInfo> DoNeedsTurnTick => _DoNeedsTurnTick;
        public RelayCommand<TeamGroupModel> DoNeedsTeamTicks => _DoNeedsTeamTicks;
        public RelayCommand<CreatureTrackerInfo> DoRemoveNeedsTurnTick => _DoRemoveNeedsTurnTick;
        public RelayCommand DoCommitNeedsTurnTick => _DoCommitNeedsTurnTick;
        public RelayCommand DoStandDown => _DoStandDown;

        #region private void RefreshTeams()
        private void RefreshTeams()
        {
            var _budgets = Tracker.AllBudgets;
            (bool inTracker, bool needsTurnTick, bool isInitiative, bool pending) _getFlags(CreatureTrackerInfo creatureTrackerInfo)
                => (_budgets.TryGetValue(creatureTrackerInfo.ID, out var _b))
                ? (true,
                _b.BudgetItems.OfType<AdjunctBudgetInfo>().Any(_ab => _ab.Adjunct.Message.Equals(@"Uzi.Ikosa.Tactical.NeedsTurnTick", StringComparison.OrdinalIgnoreCase)),
                _b.IsInitiative,
                _NeedsIDs.Contains(_b.ActorID))
                : (false, false, false, _NeedsIDs.Contains(creatureTrackerInfo.ID));

            var _svcRosters = MasterModel.Proxies.GetTeamRosters()
                .Select(_team => new
                {
                    TeamGroupInfo = _team,
                    IsExpanded = true,
                    Creatures = (from _cti in _team.PrimaryCreatures
                                 let _flags = _getFlags(_cti)
                                 where !_flags.isInitiative && !_flags.needsTurnTick
                                 orderby _flags.inTracker ? 0 : 1, _cti.CreatureLoginInfo.Name
                                 select new CreatureTrackerModel
                                 {
                                     CreatureTrackerInfo = _cti,
                                     IsInTracker = _flags.inTracker,
                                     IsInitiative = _flags.isInitiative,
                                     NeedsTurnTick = _flags.pending,
                                     TickTrackerMode = this
                                 }).ToList()
                })
                .Where(_team => _team.Creatures.Any())
                .ToList();

            // filter into active and pending team lists: sort into pending and non-pending
            var _activeRosters = (from _r in _svcRosters
                                  select new
                                  {
                                      _r.TeamGroupInfo,
                                      IsExpanded = true,
                                      Creatures = (from _ctm in _r.Creatures
                                                   where !_ctm.NeedsTurnTick
                                                   orderby _ctm.IsInTracker ? 0 : 1, _ctm.CreatureTrackerInfo.CreatureLoginInfo.Name
                                                   select _ctm).ToList()
                                  })
                                  .Where(_team => _team.Creatures.Any())
                                  .ToList();
            var _pendingRosters = (from _r in _svcRosters
                                   select new
                                   {
                                       _r.TeamGroupInfo,
                                       IsExpanded = true,
                                       Creatures = (from _ctm in _r.Creatures
                                                    where _ctm.NeedsTurnTick
                                                    orderby _ctm.IsInTracker ? 0 : 1, _ctm.CreatureTrackerInfo.CreatureLoginInfo.Name
                                                    select _ctm).ToList()
                                   })
                                   .Where(_team => _team.Creatures.Any())
                                   .ToList();


            // teams not in svc
            foreach (var _roster in (from _t in Teams
                                     where !_activeRosters.Any(_st => _st.TeamGroupInfo.ID == _t.TeamGroupInfo.ID)
                                     select _t).ToList())
            {
                Teams.Remove(_roster);
            }
            foreach (var _roster in (from _t in PendingNeedsTeams
                                     where !_pendingRosters.Any(_st => _st.TeamGroupInfo.ID == _t.TeamGroupInfo.ID)
                                     select _t).ToList())
            {
                PendingNeedsTeams.Remove(_roster);
            }

            // conformulate update team-group-models
            foreach (var _roster in (from _t in Teams
                                     join _st in _activeRosters
                                     on _t.TeamGroupInfo.ID equals _st.TeamGroupInfo.ID
                                     select new { Team = _t, SvcTeam = _st }).ToList())
            {
                _roster.Team.Conformulate(_roster.SvcTeam.Creatures);
            }
            foreach (var _roster in (from _t in PendingNeedsTeams
                                     join _st in _pendingRosters
                                     on _t.TeamGroupInfo.ID equals _st.TeamGroupInfo.ID
                                     select new { Team = _t, SvcTeam = _st }).ToList())
            {
                _roster.Team.Conformulate(_roster.SvcTeam.Creatures);
            }

            // conformulate add team-group-models
            foreach (var _roster in (from _st in _activeRosters
                                     where !Teams.Any(_t => _st.TeamGroupInfo.ID == _t.TeamGroupInfo.ID)
                                     select _st).ToList())
            {
                var _tgm = new TeamGroupModel
                {
                    TickTrackerMode = this,
                    TeamGroupInfo = _roster.TeamGroupInfo,
                    IsExpanded = _roster.IsExpanded
                };
                _tgm.Conformulate(_roster.Creatures);
                Teams.Add(_tgm);
            }
            foreach (var _roster in (from _st in _pendingRosters
                                     where !PendingNeedsTeams.Any(_t => _st.TeamGroupInfo.ID == _t.TeamGroupInfo.ID)
                                     select _st).ToList())
            {
                var _tgm = new TeamGroupModel
                {
                    TickTrackerMode = this,
                    TeamGroupInfo = _roster.TeamGroupInfo,
                    IsExpanded = _roster.IsExpanded
                };
                _tgm.Conformulate(_roster.Creatures);
                PendingNeedsTeams.Add(_tgm);
            }
        }
        #endregion

        public ObservableCollection<TeamGroupModel> Teams
            => _Teams;

        public ObservableCollection<TeamGroupModel> PendingNeedsTeams
            => _Needs;

        public string StandDownGroupName
        {
            get => _StandDownGroupName;
            set
            {
                _StandDownGroupName = value;
                DoPropertyChanged(nameof(StandDownGroupName));
            }
        }

        protected override void OnRefreshTracker()
        {
            base.OnRefreshTracker();
            MasterModel?.Proxies?.DoMasterDispatch(
                () =>
                {
                    RefreshTeams();

                    // close any actors that might fall out of this mode...
                    bool _inRoundMarker(Guid actorID)
                        => Teams.Any(_t => _t.Creatures.Any(_c => _c.CreatureTrackerInfo?.CreatureLoginInfo.ID == actorID));
                    var _proxies = MasterModel.Proxies;
                    foreach (var _actor in _proxies.Actors.Where(_a => !_a.IsListed && !_inRoundMarker(_a.CreatureLoginInfo.ID)))
                    {
                        _actor.ObservableActor?.DoShutdown();
                    }
                });
        }

        protected override void OnLeaveMode(TickTrackerModeBase newMode)
        {
            base.OnLeaveMode(newMode);

            try
            {
                // close any unlisted actors started with this mode...
                var _proxies = MasterModel.Proxies;
                foreach (var _actor in _proxies.Actors.Where(_a => !_a.IsListed))
                {
                    _actor.ObservableActor?.DoShutdown();
                }
            }
            catch
            {
                // TODO: soften any exception
            }
        }
    }
}
