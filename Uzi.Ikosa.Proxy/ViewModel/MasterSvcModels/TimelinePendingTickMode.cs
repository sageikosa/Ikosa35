using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class TimelinePendingTickMode : TickTrackerModeBase
    {
        #region ctor(...)
        public TimelinePendingTickMode(IsMasterModel master, LocalTurnTrackerInfo tracker)
            : base(master, tracker)
        {
            _DoActAll = new RelayCommand(
                () =>
                {
                    foreach (var _critter in (from _c in _Creatures
                                              where (_c.ActivityInfoBuilder?.IsReady ?? false)
                                              select _c).ToList())
                    {
                        // execute and clear
                        MasterModel.Proxies.IkosaProxy.Service.DoAction(_critter.ActivityInfoBuilder.BuildActivity());
                        _critter.ClearActivityBuilding();
                    }

                    // refresh once
                    RefreshTracker(Tracker);
                });

            _DoClearAll = new RelayCommand(
                () =>
                {
                    foreach (var _critter in (from _c in _Creatures
                                              where _c.ActivityInfoBuilder != null
                                              select _c).ToList())
                    {
                        // clear
                        _critter.ClearActivityBuilding();
                    }

                    // refresh once
                    RefreshTracker(Tracker);
                });

            _TimeTickMode = new TimeTickMode(master, tracker);
            _Teams = new ObservableCollection<TimelineGroupModel>();
            _Creatures = new ObservableCollection<CreatureTimelinePendingModel>();
            RefreshTracker(tracker);
        }
        #endregion

        #region data
        private readonly RelayCommand _DoClearAll;
        private readonly RelayCommand _DoActAll;
        private TimeTickMode _TimeTickMode;
        private readonly ObservableCollection<TimelineGroupModel> _Teams;
        private readonly ObservableCollection<CreatureTimelinePendingModel> _Creatures;
        #endregion

        public RelayCommand DoClearAll => _DoClearAll;
        public RelayCommand DoActAll => _DoActAll;
        public TimeTickMode TimeTickMode => _TimeTickMode;
        public ObservableCollection<TimelineGroupModel> Teams => _Teams;
        public ObservableCollection<CreatureTimelinePendingModel> Creatures => _Creatures;

        #region private void RefreshTeams()
        private void RefreshTeams()
        {
            var _budgets = Tracker.AllBudgets;
            bool _getStatus(CreatureTrackerInfo creatureTrackerInfo)
                => _budgets.TryGetValue(creatureTrackerInfo.ID, out var _b);

            var _teams = MasterModel.Proxies.GetTeamRosters().ToList();
            var _svcRosters = _teams
                .Select(_team => new
                {
                    TeamGroupInfo = _team,
                    IsExpanded = true,
                    Creatures = (from _cti in _team.PrimaryCreatures
                                 let _inTracker = _getStatus(_cti)
                                 where _inTracker
                                 orderby _cti.CreatureLoginInfo.Name
                                 select new CreatureTimelinePendingModel
                                 {
                                     CreatureTrackerInfo = _cti,
                                     IsInTracker = _inTracker,
                                     IsInitiative = false,
                                     NeedsTurnTick = false,
                                     TickTrackerMode = this,
                                     IsExpanded = false
                                 }).ToList()
                })
                .Where(_team => _team.Creatures.Any())
                .ToList();

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
            foreach (var _roster in (from _st in _svcRosters
                                     where !Teams.Any(_t => _st.TeamGroupInfo.ID == _t.TeamGroupInfo.ID)
                                     select _st).ToList())
            {
                var _tgm = new TimelineGroupModel
                {
                    TickTrackerMode = this,
                    TeamGroupInfo = _roster.TeamGroupInfo,
                    IsExpanded = _roster.IsExpanded
                };
                _tgm.Conformulate(_roster.Creatures);
                Teams.Add(_tgm);
            }

            // --------------------------
            // creatures no longer in teams
            foreach (var _critter in (from _c in Creatures
                                      where !Teams.Any(_t => _t.FindCreature(_c.ID) != null)
                                      select _c).ToList())
            {
                Creatures.Remove(_critter);
            }

            // creatures new to teams
            foreach (var _critter in (from _t in Teams
                                      from _c in _t.Creatures
                                      where !Creatures.Any(_cr => _cr.ID == _c.ID)
                                      select _c).ToList())
            {
                Creatures.Add(_critter);
            }
        }
        #endregion

        protected override void OnRefreshTracker()
        {
            base.OnRefreshTracker();
            _TimeTickMode.RefreshTracker(Tracker);
            MasterModel.Proxies.DoMasterDispatch(() => RefreshTeams());
        }

        protected override void OnLeaveMode(TickTrackerModeBase newMode)
        {
            _TimeTickMode.LeaveMode(newMode);
            base.OnLeaveMode(newMode);
        }
    }
}
