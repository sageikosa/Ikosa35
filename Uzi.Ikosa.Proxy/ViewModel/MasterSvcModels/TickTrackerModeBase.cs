using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public abstract class TickTrackerModeBase : ViewModelBase
    {
        protected TickTrackerModeBase(IsMasterModel master, LocalTurnTrackerInfo tracker)
        {
            _Master = master;
            _Tracker = tracker;
        }

        #region data
        private readonly IsMasterModel _Master;
        protected LocalTurnTrackerInfo _Tracker;
        #endregion

        public IsMasterModel MasterModel => _Master;
        public LocalTurnTrackerInfo Tracker => _Tracker;

        protected virtual void OnLeaveMode(TickTrackerModeBase newMode)
        {
        }

        public void LeaveMode(TickTrackerModeBase newMode)
        {
            OnLeaveMode(newMode);
        }

        protected virtual void OnRefreshTracker()
        {
        }

        public void RefreshTracker(LocalTurnTrackerInfo tracker)
        {
            _Tracker = tracker;
            OnRefreshTracker();
            DoPropertyChanged(nameof(Tracker));
        }

        public static TickTrackerModeBase GetTrackerMode(IsMasterModel masterModel,
            LocalTurnTrackerInfo tracker)
        {
            // check if current mode if desired mode and refresh
            // otherwise return null
            Mode _checkMode<Mode>() where Mode : TickTrackerModeBase
            {
                if (masterModel?.TickTrackerMode is Mode _mode)
                {
                    _mode.RefreshTracker(tracker);
                    return _mode;
                }
                return null;
            }

            switch (tracker?.TickTrackerMode)
            {
                case TickTrackerMode.TurnTick:
                    return _checkMode<TurnTickMode>() ?? new TurnTickMode(masterModel, tracker);

                case TickTrackerMode.RoundMarker:
                    return _checkMode<RoundMarkerMode>() ?? new RoundMarkerMode(masterModel, tracker);

                case TickTrackerMode.NeedsTurnTick:
                    return _checkMode<NeedsTurnTickMode>() ?? new NeedsTurnTickMode(masterModel, tracker);

                case TickTrackerMode.PromptTurnTracker:
                    return _checkMode<PromptTurnTrackerMode>() ?? new PromptTurnTrackerMode(masterModel, tracker);

                case TickTrackerMode.InitiativeStartup:
                    return _checkMode<InitStartupMode>() ?? new InitStartupMode(masterModel, tracker);

                case TickTrackerMode.TimelinePending:
                case TickTrackerMode.TimelineReady:
                    return _checkMode<TimelinePendingTickMode>() ?? new TimelinePendingTickMode(masterModel, tracker);

                case TickTrackerMode.TimelineFlowing:
                // TODO:

                default:
                case TickTrackerMode.TimeTick:
                    return _checkMode<TimeTickMode>() ?? new TimeTickMode(masterModel, tracker);
            }
        }
    }
}
