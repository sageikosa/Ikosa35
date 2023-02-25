using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class TurnTickMode : TickTrackerModeBase
    {
        #region ctor()
        public TurnTickMode(IsMasterModel master, LocalTurnTrackerInfo tracker)
            : base(master, tracker)
        {
            _Creatures = new List<CreatureTrackerModel>();

            _EndTurn = new RelayCommand<CreatureTrackerModel>(
                (critter) =>
                {
                    try
                    {
                        MasterModel?.Proxies.IkosaProxy.Service.EndTurn(critter.ID.ToString());
                    }
                    catch (Exception _ex)
                    {
                        Debug.WriteLine(_ex);
                        MessageBox.Show(_ex.Message, @"Ikosa Exception",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                },
                (critter) => critter?.CreatureTrackerInfo.LocalActionBudgetInfo.IsFocusedBudget ?? false);

            _DoControl = new RelayCommand<CreatureTrackerModel>(
                (critter) =>
                {
                    var _proxies = MasterModel.Proxies;
                    var _actor = _proxies.Actors.FirstOrDefault(_a => _a.CreatureLoginInfo.ID == critter.ID);
                    if (_actor != null)
                    {
                        // found actor in list, show it
                        _actor.ShowObservableActor();
                    }
                    else
                    {
                        // did not, so login and generate view
                        _proxies.LoginCreature(critter.CreatureTrackerInfo.CreatureLoginInfo,
                            (found) => _proxies.GenerateView?.Invoke(new ActorModel(_proxies, found, false)),
                            (m) => _proxies.GenerateLog?.Invoke(m));
                    }
                });

            _DoStandDown = new RelayCommand(
                () =>
                {
                    MasterModel.Proxies.MasterProxy.Service.TurnTrackerStop(_StandDownGroupName);
                },
                () => !string.IsNullOrWhiteSpace(_StandDownGroupName));

            RefreshTracker(tracker);
        }
        #endregion

        #region data
        private readonly RelayCommand<CreatureTrackerModel> _DoControl;
        private readonly RelayCommand<CreatureTrackerModel> _EndTurn;
        private readonly RelayCommand _DoStandDown;
        private string _StandDownGroupName;
        private List<CreatureTrackerModel> _Creatures;
        #endregion

        public RelayCommand<CreatureTrackerModel> DoControl => _DoControl;
        public RelayCommand<CreatureTrackerModel> EndTurnCommand => _EndTurn;
        public RelayCommand DoStandDown => _DoStandDown;
        public List<CreatureTrackerModel> Creatures => _Creatures;

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

            var _teams = MasterModel.Proxies.GetTeamRosters();
            var _critters = (from _upcoming in Tracker.UpcomingBudgets
                             let _cti = (from _t in _teams
                                         let _c = _t.FindCreature(_upcoming.ActorID)
                                         where _c != null
                                         select _c).FirstOrDefault()
                             where _cti != null
                             select new CreatureTrackerModel
                             {
                                 CreatureTrackerInfo = _cti,
                                 TickTrackerMode = this,
                                 IsExpanded = false,
                                 IsInitiative = _upcoming.IsInitiative,
                                 IsInTracker = true,
                                 NeedsTurnTick = _upcoming.BudgetItems.OfType<AdjunctBudgetInfo>()
                                    .Any(_ab => _ab.Adjunct.Message.Equals(@"Uzi.Ikosa.Tactical.NeedsTurnTick", StringComparison.OrdinalIgnoreCase))
                             }).ToList();
            _Creatures = _critters;
            DoPropertyChanged(nameof(Creatures));
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
