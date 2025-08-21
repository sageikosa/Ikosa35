using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Universal;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Adjuncts;
using System.ComponentModel;
using Uzi.Ikosa.Contracts;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class LocalTurnTracker : CoreTickTracker, ICanReactWithNewProcess, INotifyPropertyChanged
    {
        #region ctor()
        /// <summary>Initiative startup</summary>
        public LocalTurnTracker(IEnumerable<CoreActor> initialActors, CoreSettingContextSet contextSet, bool initiative)
            : base()
        {
            // TODO: timeline tracker

            _Resolution = Time.Round.UnitFactor;
            _CtxSet = contextSet;
            _Budgets = [];
            _Delayed = [];
            _DelayIndex = 0;
            _Init = initiative;
            _CanTimeTick = true;

            // get things moving
            if (initiative)
            {
                // initialize reactor
                _CtxSet.ProcessManager.ContextReactors.Reactors.Add(this, this);

                // need initiative (using turn tracking)
                _Process = new CoreProcess(new InitiativeStartupStep(this, initialActors), @"Turn Tracking");
            }
            else
            {
                // do NOT need initiative (using time tracking)
                // set time for the tick
                var _tick = LocalTurnTick.CreateRoundMarker(this);

                // create action budgets
                foreach (var _actor in initialActors)
                {
                    AddBudget(_actor.CreateActionBudget(_tick) as LocalActionBudget);
                }

                // start tick
                var _start = new LocalStartOfTickStep(this);
                _Process = new CoreProcess(_start, @"Time Tracking");
                new LocalTimeStep(_start);
            }

            // track the tracker and start the process
            _CtxSet.ProcessManager.StartProcess(_Process);
        }
        #endregion

        #region data
        private LocalActionBudget _Focus = null;
        private double _Resolution;
        private CoreSettingContextSet _CtxSet;
        private List<LocalActionBudget> _Budgets;
        private List<LocalActionBudget> _Delayed;
        private int _DelayIndex;
        private CoreProcess _Process;
        private bool _Init;
        private bool _CanTimeTick;
        #endregion

        #region protected void DoChanged()
        protected void DoChanged()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(ReactableBudgets)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(FocusedBudget)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(LeadBudgets)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(DelayedBudget)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(TimeOrderedBudgets)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(DelayedBudgets)));
            }
        }
        #endregion

        [field:NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Indicates whether real time clock can tick time tracking forward</summary>
        public bool IsAutoTimeTick { get => _CanTimeTick; set => _CanTimeTick = value; }

        /// <summary>Indicates this turn tracker is a true turn tracker (otherwise it tracks time)</summary>
        public bool IsInitiative => _Init;

        /// <summary>Non-initiative tracker, and all budgets can flow</summary>
        public bool CanTimelineFlow
            => !IsInitiative && _Budgets.All(_b => _b.CanTimelineFlow);

        /// <summary>Non-initiative tracker, and some budgets are expecting timeline</summary>
        public bool HasTimelineActions
            => !IsInitiative && _Budgets.Any(_b => _b.HasTimelineActions);

        public Step GetTrackerStep<Step>() where Step : CoreStep
            => _Process?.GetCurrentStep() as Step;

        #region internal void Shutdown()
        /// <summary>Called by IkosaProcessManager</summary>
        internal void Shutdown()
        {
            if (_Process != null)
            {
                _Process.IsActive = false;
            }
            if (IsInitiative)
            {
                _CtxSet.ProcessManager.ContextReactors.Reactors.Remove(this);
            }

            void _cleanup(Creature critter)
            {
                critter.Adjuncts.OfType<UnpreparedToDodge>()
                    .FirstOrDefault(_d => (_d.Source as Type) == typeof(LocalTurnTracker))?.Eject();
                critter.Adjuncts.OfType<UnpreparedForOpportunities>()
                    .FirstOrDefault(_d => (_d.Source as Type) == typeof(LocalTurnTracker))?.Eject();
            };

            // do not leave lingering unpreparedness
            var _init = GetTrackerStep<InitiativeStartupStep>();
            if (_init != null)
            {
                // if still in the initiative startup, no budgets yet, so look at prerequisites for things to cleanup
                foreach (var _critter in from _pre in _init.DispensedPrerequisites.OfType<RollPrerequisite>()
                                         where _pre.Fulfiller is Creature
                                         select _pre.Fulfiller as Creature)
                {
                    _cleanup(_critter);
                }
            }

            // cleanup any established budgets
            foreach (var _budget in _Budgets)
            {
                // remove budget items
                foreach (var _item in _budget.BudgetItems.ToList())
                {
                    _budget.BudgetItems.Remove(_item.Key);
                }

                // remove tracker conditions and adjuncts
                if (_budget.Actor is Creature _critter)
                {
                    _cleanup(_critter);
                }
            }
        }
        #endregion

        /// <summary>Find the action budget associated with the actor</summary>
        public LocalActionBudget GetBudget(Guid actorID)
            => _Delayed.Union(_Budgets).FirstOrDefault(_bdg => _bdg.Actor.ID == actorID);

        /// <summary>Get budgets in reactive order.</summary>
        public IList<LocalActionBudget> ReactableBudgets
            => _Delayed.Concat(_Budgets.Except(_Delayed).OrderBy(_b => _b.NextReset)).ToList();

        /// <summary>Resets the DelayIndex to 0, prompting processing of delayed budgets on DoNextStep.</summary>
        private void ResetDelayList()
        {
            _DelayIndex = 0;
            while (DelayedBudget?.Creature?.Actions.Filters.Any(_kvp => _kvp.Value is IActionSkip) ?? false)
            {
                // skip past any leading creatures that are forced to skip
                _DelayIndex++;
            }
        }

        #region public void CompleteStep(CoreStep stepCompleting)
        /// <summary>
        /// Gets the next step for the current state of the tracker.
        /// <para>Called at initiative startup, and when a tick is done</para>
        /// </summary>
        /// <param name="stepCompleting"></param>
        public void CompleteStep<StepType>(StepType stepCompleting)
            where StepType : CoreStep, ITurnTrackingStep
        {
            #region void _endTimeTickBudgets(IList<LocalActionBudget> budgets)
            void _endTimeTickBudgets(IList<LocalActionBudget> budgets)
            {
                // all included but unaffiliated budgets will end turn together
                foreach (var _budget in budgets)
                {
                    // clear WillAct and IsUsingTurn
                    _budget.EndTurn();
                }

                // tick ender budgets ...
                var _tickEnd = (from _b in budgets
                                from _kvp in _b.BudgetItems
                                where _kvp.Value is ITickEndBudgetItem
                                select new
                                {
                                    _kvp.Key,
                                    EndBudget = _kvp.Value as ITickEndBudgetItem,
                                    Budget = _b
                                }).ToList();

                // ... that need to be removed ...
                var _removers = (from _item in _tickEnd
                                 where _item.EndBudget.EndTick()
                                 select _item).ToList();

                // ... will now be removed
                foreach (var _rmv in _removers)
                {
                    _rmv.Budget.BudgetItems.Remove(_rmv.Key);
                }
            }
            #endregion

            if (stepCompleting is InitiativeStartupStep)
            {
                // since there won't be any delayed budgets at this point, start with a tick
                _Focus = LeadBudgets.FirstOrDefault();
                new LocalTickStartStep(stepCompleting, this);
            }
            else if (stepCompleting is LocalTimeStep _lastTime)
            {
                #region processing LocalTimeStep
                _endTimeTickBudgets(ReactableBudgets);

                // end the tick and move time forward
                LeadingTick.EndOfTick();
                _Focus = null;

                // creatures with unfriendly awareness (anywhere in map)
                var _triggered = (from _critter in Map.ContextSet.GetCoreIndex().Select(_idx => _idx.Value).OfType<Creature>()
                                  let _unfriendly = _critter.Awarenesses.UnFriendlyAwarenesses.ToList()
                                  where (_unfriendly.Count != 0)
                                  let _sdgi = _critter.Adjuncts.OfType<StandDownGroupParticipant>().Select(_s => _s.StandDownGroup).ToList()
                                  // that either are not in a standDownGroup
                                  where (_sdgi.Count == 0)
                                  // or do not share a standDownGroup with at least one unfriendly
                                  || _unfriendly.Any(_u => !_sdgi.Any(_g => _g.IsInGroup(_u.ID)))
                                  select _critter).ToList();
                if (_triggered.Any())
                {
                    // disable auto-time tick
                    IsAutoTimeTick = false;
                    new PromptTurnTrackerStep(_lastTime, _triggered);
                }
                else if (HasTimelineActions)
                {
                    // disable auto-time tick if any budget has timeline compatible providers
                    IsAutoTimeTick = false;
                }

                // chain time to start of tick
                var _start = new LocalStartOfTickStep(_lastTime);
                new LocalTimeStep(_start);
                #endregion
            }
            else if (stepCompleting is RoundMarkerTickStep _roundMarker)
            {
                #region processing RoundMarkerTickStep
                _endTimeTickBudgets(_roundMarker.Budgets);

                // move out of round marker
                _Focus = null;

                // no actions will be possible while this is the active step
                new NeedsTurnTickStep(stepCompleting, this);
                #endregion
            }
            else if (stepCompleting is NeedsTurnTickStep _needsTurnTick)
            {
                // push the tick forward (was the RoundMarker tick)
                LeadingTick.EndOfTick();

                // move onto the tick
                _Focus = LeadBudgets.FirstOrDefault();
                new LocalTickStartStep(stepCompleting, this);
            }
            else
            {
                // TODO: should (probably) de-escalate to time-tracker?

                var _inq = stepCompleting.DispensedPrerequisites.OfType<ActionInquiryPrerequisite>().FirstOrDefault();
                if (stepCompleting is DelayedTickStep _delayedTick)
                {
                    if ((_inq != null) && (_inq.Acted ?? false))
                    {
                        // if delayer acted, then the leading tick needs to be pushed forward
                        LeadingTick.EndOfTick();

                        // reset delay list reference
                        ResetDelayList();

                        // remove budget that acted from delay list
                        _Delayed.Remove(_delayedTick.Budget);
                    }
                    else
                    {
                        // progress through the delay list
                        _DelayIndex++;
                        while (DelayedBudget?.Creature?.Actions.Filters.Any(_kvp => _kvp.Value is IActionSkip) ?? false)
                        {
                            // skip past any leading creatures that are forced to skip
                            _DelayIndex++;
                        }
                    }
                }
                else if (stepCompleting is LocalTickStep _localTick)
                {
                    // push the tick forward
                    var _budget = _localTick.Budget;
                    LeadingTick.EndOfTick();

                    if (_inq != null)
                    {
                        if (_inq.Acted ?? false)
                        {
                            // reset delay list reference, if someone acted
                            ResetDelayList();
                            if (_Delayed.Contains(_budget))
                            {
                                _Delayed.Remove(_budget);
                            }
                        }
                        else
                        {
                            // progress through the delay list
                            _DelayIndex++;
                            if (!_Delayed.Contains(_budget))
                            {
                                // needed to add this to delay list
                                _Delayed.Add(_budget);
                            }

                            while (DelayedBudget?.Creature?.Actions.Filters.Any(_kvp => _kvp.Value is IActionSkip) ?? false)
                            {
                                // skip past any leading creatures that are forced to skip
                                _DelayIndex++;
                            }
                        }
                    }
                }

                // exhausted all delaying budgets?
                _Focus = DelayedBudget;
                if (_Focus != null)
                {
                    if (_Focus == LeadBudgets.FirstOrDefault())
                    {
                        // since the next delay budget is the next tick budget ...
                        // ... jump right to the tick
                        new LocalTickStartStep(stepCompleting, this);
                    }
                    else
                    {
                        // create a delayed tick step
                        new DelayedTickStep(stepCompleting, this);
                    }
                }
                else
                {
                    if (LeadingTick == RoundMarker)
                    {
                        // round marker to allow processing of unaffiliated budgets...
                        _Focus = null;
                        new RoundMarkerTickStep(stepCompleting, this);
                    }
                    else
                    {
                        // otherwise, move onto the tick
                        _Focus = LeadBudgets.FirstOrDefault();
                        new LocalTickStartStep(stepCompleting, this);
                    }
                }
            }
            DoChanged();
        }
        #endregion

        #region public bool CanTrackerStepDoAction(LocalActionBudget budget, ActionBase action)
        /// <summary>True if the current step is valid for the budget.</summary>
        public bool CanTrackerStepDoAction(LocalActionBudget budget, ActionBase action)
        {
            var _procMgr = _CtxSet.ProcessManager;
            if (!IsInitiative)
            {
                return (_procMgr.CurrentStep is LocalTimeStep)
                    && TimeOrderedBudgets.Contains(budget)
                    && action.IsHarmless;
            }
            else
            {
                if (action.TimeCost.ActionTimeType == TimeType.Free)
                {
                    return true;
                }

                // running in a RoundMarkerTickStep?
                if (_procMgr.CurrentStep is RoundMarkerTickStep _marker)
                {
                    return _marker.Budgets
                       // must not be bound to a turn tick
                       .Where(_b => !_b.IsInitiative
                            // cannot need a turn tick
                            && !_b.Actor.HasAdjunct<NeedsTurnTick>()
                            // budget is not initiative, so must be harmless action
                            && action.IsHarmless)
                        .Contains(budget);
                }
                else
                {
                    // only allow actions if there an inquiry for this budget ...
                    // ... or if the action cost is free
                    return _procMgr.CurrentStep.DispensedPrerequisites
                        .OfType<ActionInquiryPrerequisite>()
                        .Any(_aip => _aip.Budget == budget);
                }
            }
        }
        #endregion

        /// <summary>Returns the budget associated with the focused action inquiry</summary>
        public LocalActionBudget FocusedBudget => _Focus;

        /// <summary>Yields the budgets associated with the leading tick</summary>
        public IEnumerable<LocalActionBudget> LeadBudgets
            => _Budgets.Where(_b => _b.TurnTick == LeadingTick);

        /// <summary>Returns the next delayed budget by tracked index</summary>
        public LocalActionBudget DelayedBudget
            => (_DelayIndex < _Delayed.Count) ? _Delayed[_DelayIndex] : null;

        public IEnumerable<LocalActionBudget> TickOrderedBudgets
            => (from _tick in Ticks.OfType<LocalTurnTick>()
                from _budget in _Budgets
                where _budget.TurnTick == _tick
                select _budget);

        public IEnumerable<LocalActionBudget> TimeOrderedBudgets
            => _Budgets.OrderBy(_b => _b.NextReset).ToList();

        public IEnumerable<LocalActionBudget> DelayedBudgets
            => _Delayed.Skip(_DelayIndex).ToList();

        /// <summary>Calculates time delta to subdivide interval between current time and next tick time.</summary>
        /// <param name="extraSlices">number of extra slices needed in the interval</param>
        public double TimeDelta(int extraSlices)
            => ((LeadingTick as LocalTurnTick).Time - Map.CurrentTime) / (extraSlices + 1);

        /// <summary>Get time tick that is the round marker.</summary>
        public LocalTurnTick RoundMarker => Ticks.OfType<LocalTurnTick>().FirstOrDefault(_t => _t.IsRoundMarker);

        /// <summary>Add a budget already associated with a tick</summary>
        public void AddBudget(LocalActionBudget budget)
        {
            if ((budget != null)
                && !_Budgets.Contains(budget)
                && !_Budgets.Any(_b => _b.Creature == budget.Creature))
            {
                _Budgets.Add(budget);
                DoChanged();
            }
        }

        /// <summary>Remove a budget from tracker</summary>
        public void RemoveBudget(LocalActionBudget budget)
        {
            if (_Budgets.Contains(budget))
            {
                _Budgets.Remove(budget);
                DoChanged();
            }
        }

        public CoreSettingContextSet ContextSet => _CtxSet;
        public LocalMap Map => _CtxSet.Setting as LocalMap;
        public double TickResolution => _Resolution;

        #region public void StartOfTick()
        /// <summary>Track budgets for NextTick, and TimeTick the map</summary>
        public void StartOfTick()
        {
            var _tick = LeadingTick as LocalTurnTick;

            // post tick
            foreach (var _b in TimeOrderedBudgets)
            {
                _b.TrackTime(_tick.Time, TimeValTransition.Leaving);
            }

            // map tick
            Map.TimeTick(_tick.Time);

            // pre tick
            foreach (var _b in TimeOrderedBudgets)
            {
                _b.TrackTime(Map.CurrentTime, TimeValTransition.Entering);
            }

            DoChanged();
        }
        #endregion

        // TODO: surprise round?
        // TODO: readied actions
        // TODO: react to movement

        #region ICanReact Members
        public void ReactToProcessByStep(CoreProcess process)
        {
            if (process is CoreActivity _activity)
            {
                // spawn opportunistic inquiry is available if turn-tracking and provoking melee
                if ((_activity.Action is ActionBase _action) && IsInitiative)
                {
                    if (_action.ProvokesMelee || _action.ProvokesTarget)
                    {
                        // TODO: inquiry didn't actually spawn prerequisites...(or should have been defunct?)
                        process.AppendPreEmption(new OpportunisticInquiry(_activity));
                    }
                }
            }
        }

        public bool IsFunctional
            => true;
        #endregion

        #region public LocalTurnTrackerInfo ToLocalTurnTrackerInfo()
        public LocalTurnTrackerInfo ToLocalTurnTrackerInfo(Creature creature)
        {
            TickTrackerMode _timeMode()
            {
                if (HasTimelineActions)
                {
                    if (CanTimelineFlow)
                    {
                        if (GetTrackerStep<LocalTimeStep>() is LocalTimeStep _time
                            && !_time.TimeTickablePrerequisite.IsReady)
                        {
                            // timefline can flow, but step's tickable status is not ready
                            // indicating the tick started unable to flow, but has since changed to flowable
                            // just needs a little push to get going
                            return TickTrackerMode.TimelineReady;
                        }

                        // compatible and flowing
                        return TickTrackerMode.TimelineFlowing;
                    }

                    // timeline compatible with open action potentials
                    return TickTrackerMode.TimelinePending;
                }
                return TickTrackerMode.TimeTick;
            }

            if (creature != null)
            {
                return new LocalTurnTrackerInfo
                {
                    TickTrackerMode = IsInitiative ? TickTrackerMode.TurnTick : _timeMode(),
                    IsInitiative = IsInitiative,
                    IsAutoTimeTick = !IsInitiative,
                    CurrentTime = double.NaN,
                    LeadingTick = null,
                    UpcomingTicks = IsInitiative
                        ? (from _tick in Ticks.OfType<LocalTurnTick>()
                           where !_tick.IsRoundMarker
                           select new TickInfo
                           {
                               Time = _tick.Time,
                               InitiativeScore = null,
                               IsRoundMarker = false,
                               // filter budgets to awarenesses...
                               Budgets = _Budgets
                                            .Where(_b => _b.TurnTick == _tick)
                                            .Select(_b => _b.ToLocalActionBudgetInfo(creature))
                                            .Where(_b => _b?.ActorID != null)
                                            .ToList()
                           })
                           // only list ticks where budgets exist
                           .Where(_ti => _ti.Budgets.Count > 0)
                           .ToList()
                        : []
                };
            }
            else
            {
                // base info
                TickTrackerMode _masterMode()
                {
                    if (IsInitiative)
                    {
                        if (GetTrackerStep<RoundMarkerTickStep>() is RoundMarkerTickStep)
                        {
                            return TickTrackerMode.RoundMarker;
                        }
                        else if (GetTrackerStep<NeedsTurnTickStep>() is NeedsTurnTickStep)
                        {
                            return TickTrackerMode.NeedsTurnTick;
                        }
                        return TickTrackerMode.TurnTick;
                    }
                    else
                    {
                        if (GetTrackerStep<PromptTurnTrackerStep>() is PromptTurnTrackerStep)
                        {
                            return TickTrackerMode.PromptTurnTracker;
                        }
                        else if (GetTrackerStep<InitiativeStartupStep>() is InitiativeStartupStep)
                        {
                            return TickTrackerMode.InitiativeStartup;
                        }
                        return _timeMode();
                    }
                }

                return new LocalTurnTrackerInfo
                {
                    TickTrackerMode = _masterMode(),
                    IsInitiative = IsInitiative,
                    IsAutoTimeTick = IsAutoTimeTick,
                    CurrentTime = Map.CurrentTime,
                    LeadingTick = (LeadingTick is LocalTurnTick _leadTick)
                        ? new TickInfo
                        {
                            Time = _leadTick.Time,
                            InitiativeScore = _leadTick.InitiativeScore,
                            IsRoundMarker = _leadTick.IsRoundMarker,
                            Budgets = _Budgets
                                .Where(_b => _b.TurnTick == _leadTick)
                                .Select(_b => _b.ToLocalActionBudgetInfo())
                                .ToList()
                        }
                        : null,
                    ReactableBudgets = ReactableBudgets.Select(_rb => _rb.ToLocalActionBudgetInfo()).ToList(),
                    DelayedBudgets = DelayedBudgets.Select(_db => _db.ToLocalActionBudgetInfo()).ToList(),
                    UpcomingTicks = (from _tick in Ticks.OfType<LocalTurnTick>()
                                     select new TickInfo
                                     {
                                         Time = _tick.Time,
                                         InitiativeScore = _tick.InitiativeScore,
                                         IsRoundMarker = _tick.IsRoundMarker,
                                         Budgets = _Budgets
                                                    .Where(_b => _b.TurnTick == _tick)
                                                    .Select(_b => _b.ToLocalActionBudgetInfo())
                                                    .ToList()
                                     }).ToList()
                };
            }
        }
        #endregion
    }
}
