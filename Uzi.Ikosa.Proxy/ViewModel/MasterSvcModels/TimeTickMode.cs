using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class TimeTickMode : TickTrackerModeBase
    {
        #region ctor()
        public TimeTickMode(IsMasterModel master, LocalTurnTrackerInfo tracker)
            : base(master, tracker)
        {
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

            _DropDrop = new RelayCommand<object>(
                (selected) =>
                {
                    if (selected is CreatureTrackerModel _critter)
                    {
                        MasterModel.Proxies.MasterProxy.Service.TurnTrackerDrop(new[] { _critter.ID });
                    }
                    else if (selected is TeamGroupModel _team)
                    {
                        MasterModel.Proxies.MasterProxy.Service.TurnTrackerDrop(_team.Creatures.Select(_c => _c.ID).ToArray());
                    }
                },
                (selected) => (selected is CreatureTrackerModel) || (selected is TeamGroupModel));

            _NonTracked = new ObservableCollection<TeamGroupModel>();
            _ActiveTeams = new ObservableCollection<TeamGroupModel>();
            RefreshTracker(tracker);
        }
        #endregion

        #region data
        private readonly ObservableCollection<TeamGroupModel> _NonTracked;
        private readonly RelayCommand<object> _DropDrop;
        private readonly RelayCommand<CreatureTrackerInfo> _DoControl;
        private readonly ObservableCollection<TeamGroupModel> _ActiveTeams;
        #endregion

        public RelayCommand<object> DoDrop => _DropDrop;
        public RelayCommand<CreatureTrackerInfo> DoControl => _DoControl;
        public ObservableCollection<TeamGroupModel> NonTrackedTeams => _NonTracked;
        public ObservableCollection<TeamGroupModel> ActiveTeams => _ActiveTeams;

        #region private void RefreshNonTrackedTeams(List<TeamGroupInfo> rawTeams)
        private void RefreshNonTrackedTeams(List<TeamGroupInfo> rawTeams)
        {
            var _budgets = Tracker.AllBudgets;
            bool _getStatus(CreatureTrackerInfo creatureTrackerInfo)
                => _budgets.TryGetValue(creatureTrackerInfo.ID, out var _b);

            var _svcRosters = rawTeams
                .Select(_team => new
                {
                    TeamGroupInfo = _team,
                    IsExpanded = true,
                    Creatures = (from _cti in _team.PrimaryCreatures
                                 let _inTracker = _getStatus(_cti)
                                 where !_inTracker
                                 orderby _cti.CreatureLoginInfo.Name
                                 select new CreatureTrackerModel
                                 {
                                     CreatureTrackerInfo = _cti,
                                     IsInTracker = _inTracker,
                                     IsInitiative = false,
                                     NeedsTurnTick = false,
                                     TickTrackerMode = this,
                                     IsExpanded = _team.PrimaryCreatures.Any(_cti => _budgets.Any(_b => _b.Key == _cti.ID))
                                 }).ToList()
                })
                .Where(_team => _team.Creatures.Any())
                .ToList();

            // teams not in svc
            foreach (var _roster in (from _t in NonTrackedTeams
                                     where !_svcRosters.Any(_st => _st.TeamGroupInfo.ID == _t.TeamGroupInfo.ID)
                                     select _t).ToList())
            {
                NonTrackedTeams.Remove(_roster);
            }

            // conformulate update team-group-models
            foreach (var _roster in (from _t in NonTrackedTeams
                                     join _st in _svcRosters
                                     on _t.TeamGroupInfo.ID equals _st.TeamGroupInfo.ID
                                     select new { Team = _t, SvcTeam = _st }).ToList())
            {
                _roster.Team.Conformulate(_roster.SvcTeam.Creatures);
            }

            // conformulate add team-group-models
            foreach (var _roster in (from _st in _svcRosters
                                     where !NonTrackedTeams.Any(_t => _st.TeamGroupInfo.ID == _t.TeamGroupInfo.ID)
                                     select _st).ToList())
            {
                var _tgm = new TeamGroupModel
                {
                    TickTrackerMode = this,
                    TeamGroupInfo = _roster.TeamGroupInfo,
                    IsExpanded = _roster.IsExpanded
                };
                _tgm.Conformulate(_roster.Creatures);
                NonTrackedTeams.Add(_tgm);
            }
        }
        #endregion

        #region private void RefreshActiveTeams(List<TeamGroupInfo> rawTeams)
        private void RefreshActiveTeams(List<TeamGroupInfo> rawTeams)
        {
            var _budgets = Tracker.AllBudgets;
            bool _getStatus(CreatureTrackerInfo creatureTrackerInfo)
                => _budgets.TryGetValue(creatureTrackerInfo.ID, out var _b);

            var _svcRosters = rawTeams
                .Select(_team => new
                {
                    TeamGroupInfo = _team,
                    IsExpanded = true,
                    Creatures = (from _cti in _team.PrimaryCreatures
                                 let _inTracker = _getStatus(_cti)
                                 where _inTracker
                                 orderby _cti.CreatureLoginInfo.Name
                                 select new CreatureTrackerModel
                                 {
                                     CreatureTrackerInfo = _cti,
                                     IsInTracker = _inTracker,
                                     IsInitiative = false,
                                     NeedsTurnTick = false,
                                     TickTrackerMode = this,
                                     IsExpanded = _team.PrimaryCreatures.Any(_cti => _budgets.Any(_b => _b.Key == _cti.ID))
                                 }).ToList()
                })
                .Where(_team => _team.Creatures.Any())
                .ToList();

            // teams not in svc
            foreach (var _roster in (from _t in ActiveTeams
                                     where !_svcRosters.Any(_st => _st.TeamGroupInfo.ID == _t.TeamGroupInfo.ID)
                                     select _t).ToList())
            {
                ActiveTeams.Remove(_roster);
            }

            // conformulate update team-group-models
            foreach (var _roster in (from _t in ActiveTeams
                                     join _st in _svcRosters
                                     on _t.TeamGroupInfo.ID equals _st.TeamGroupInfo.ID
                                     select new { Team = _t, SvcTeam = _st }).ToList())
            {
                _roster.Team.Conformulate(_roster.SvcTeam.Creatures);
            }

            // conformulate add team-group-models
            foreach (var _roster in (from _st in _svcRosters
                                     where !ActiveTeams.Any(_t => _st.TeamGroupInfo.ID == _t.TeamGroupInfo.ID)
                                     select _st).ToList())
            {
                var _tgm = new TeamGroupModel
                {
                    TickTrackerMode = this,
                    TeamGroupInfo = _roster.TeamGroupInfo,
                    IsExpanded = _roster.IsExpanded
                };
                _tgm.Conformulate(_roster.Creatures);
                ActiveTeams.Add(_tgm);
            }
        }
        #endregion

        #region protected override void OnRefreshTracker()
        protected override void OnRefreshTracker()
        {
            base.OnRefreshTracker();

            // get raw team information
            var _rawTeams = MasterModel.Proxies.GetTeamRosters().ToList();
            MasterModel?.Proxies?.DoMasterDispatch(
                () =>
                {
                    RefreshNonTrackedTeams(_rawTeams);
                    RefreshActiveTeams(_rawTeams);
                });
        }
        #endregion

        #region protected override void OnLeaveMode(TickTrackerModeBase newMode)
        protected override void OnLeaveMode(TickTrackerModeBase newMode)
        {
            base.OnLeaveMode(newMode);

            try
            {
                if (!(newMode is TimelinePendingTickMode)
                    && !(newMode is TimeTickMode))
                {
                    // close any unlisted actors started with this mode...
                    var _proxies = MasterModel.Proxies;
                    foreach (var _actor in _proxies.Actors.Where(_a => !_a.IsListed))
                    {
                        _actor.ObservableActor?.DoShutdown();
                    }
                }
            }
            catch
            {
                // TODO: soften any exception
            }
        }
        #endregion
    }
}
