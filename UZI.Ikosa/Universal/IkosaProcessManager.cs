using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using System.ComponentModel;
using System.Threading.Tasks;
using Uzi.Ikosa.Services;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Universal
{
    [Serializable]
    public class IkosaProcessManager : CoreProcessManager, INotifyPropertyChanged
    {
        #region ctor()
        public IkosaProcessManager()
            : base()
        {
        }
        #endregion

        #region public LocalTurnTracker PopTracker()
        public LocalTurnTracker PopTracker()
        {
            var _tracker = LocalTurnTracker;
            _tracker?.Shutdown();
            if (_tracker != null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(@"LocalTurnTracker"));
            }
            return _tracker;
        }
        #endregion

        protected override void OnPreDoStep(CoreStep step)
        {
            IkosaServices.MapContext.SerialState++;
        }

        public ITurnTrackingStep TurnTrackingStep
            => AllProcesses.Select(_p => _p.RootStep).OfType<ITurnTrackingStep>().FirstOrDefault();

        public void RemoveFromTrackers(Creature creature)
        {
            // remove from all distinct trackers
            foreach (var _tracker in AllProcesses
                .Select(_p => _p.RootStep).OfType<ITurnTrackingStep>()
                .Select(_tts => _tts.Tracker)
                .Distinct().ToList())
            {
                // try to find it, then try to remove it
                _tracker.RemoveBudget(_tracker.GetBudget(creature.ID));
            }
        }

        public void CleanupTrackers(MapContext mapContext)
        {
            var _idx = mapContext.ContextSet.GetCoreIndex();

            // remove from all distinct trackers
            foreach (var _tracker in AllProcesses
                .Select(_p => _p.RootStep).OfType<ITurnTrackingStep>()
                .Select(_tts => _tts.Tracker)
                .Distinct().ToList())
            {
                foreach (var _budget in _tracker.TimeOrderedBudgets.ToList())
                {
                    // try to find it
                    if (!_idx.ContainsKey(_budget.Creature.ID))
                    {
                        // then try to remove it
                        _tracker.RemoveBudget(_budget);
                    }
                }
            }
        }

        public LocalTurnTracker LocalTurnTracker
            => TurnTrackingStep?.Tracker;

        protected override IEnumerable<ICanReactToStepComplete> OrderStepCompleteReactors(IEnumerable<ICanReactToStepComplete> canReactToSteps)
        {
            // TODO: examine reactors, see if we can order them by something sensible
            // TODO: if they are group adjuncts, look as masters
            // TODO: if group adjuncts are bound to actors, get in upcoming tick order
            return canReactToSteps;
        }

        [field:NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected override void OnDoProcessAllEnd()
        {
            IkosaServices.FlushNotifications();
        }
    }
}
