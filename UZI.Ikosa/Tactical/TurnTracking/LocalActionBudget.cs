using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using System.Collections.ObjectModel;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Universal;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Contracts;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Actions.Steps;
using System.Diagnostics;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class LocalActionBudget : CoreActionBudget, ITrackTime, IActionSource
    {
        #region ctor()
        public LocalActionBudget(LocalTurnTick tick, CoreActor actor)
            : base(actor)
        {
            // budget
            _RegularLeft = (tick != null);
            _BriefLeft = (tick != null);
            _CanTwitch = (tick != null);
            _UsingTurn = false;

            // timings
            _Tick = tick;
            if (tick != null)
            {
                NextReset = _Tick.Time;
                _TwitchReady = _Tick.Time;

                // unprepared if tracker is initiative, but budget isn't
                if (tick.TurnTracker.IsInitiative && !IsInitiative)
                {
                    Actor.AddAdjunct(new UnpreparedToDodge(typeof(LocalTurnTracker)));
                    Actor.AddAdjunct(new UnpreparedForOpportunities(typeof(LocalTurnTracker)));
                }
            }
        }
        #endregion

        #region data
        // reset times
        private double _TwitchReady;    // twitch ready time (floats on its own schedule)
        private double _NextReset;      // time at which move and regular actions become available again ...

        // action use tracking
        private bool _CanTwitch;        // true if the budget currently allows a twitch
        private bool _RegularLeft;      // true if the budget currently allows a regular action
        private bool _BriefLeft;        // true if the budget currently allows a brief action
        private LocalTurnTick _Tick;
        //private bool _WillAct;
        private bool _UsingTurn;

        // choice tracking
        private Dictionary<string, ChoiceBinder> _Choices = [];

        // held activity
        private CoreActivity _HeldActivity;     // activity to unhold when span is done
        private double _AbortResetTime;         // if span is aborted, the next time the budget resets

        // next activity
        private CoreActivity _NextActivity;
        #endregion

        // TODO: limited to single regular actions only...

        // NOTE: removed LastAction (may need to revisit?)
        public CoreActivity HeldActivity => _HeldActivity;
        public LocalTurnTick TurnTick { get => _Tick; set => _Tick = value; }

        /// <summary>True if the budget is bound to a tick that indicates using initiative</summary>
        public bool IsInitiative => !(TurnTick?.IsRoundMarker ?? false);

        #region public double NextReset { get; private set; }
        public double NextReset
        {
            get => _NextReset;
            private set
            {
                //Debug.WriteLine($@"{Actor.Name}._NextReset changed from {_NextReset} to {value}");
                _NextReset = value;
            }
        }
        #endregion

        #region public CoreActivity NextActivity { get; set; }
        /// <summary>Next activity for budget's actor (NULL for none).  Time tracking only.</summary>
        public CoreActivity NextActivity
        {
            get => _NextActivity;
            set
            {
                if (value == null)
                {
                    _NextActivity = null;
                }
                else if ((value.Actor == Actor) && !IsInitiative)
                {
                    _NextActivity = value;
                }
            }
        }
        #endregion

        #region public void DoAction(IkosaProcessManager manager, CoreActivity activity)
        /// <summary>
        /// Ensures a tick is available for this action.  
        /// <para>Binds the budget to the tick if budget was delaying and the time cost needs turn binding.</para>
        /// </summary>
        public void DoAction(IkosaProcessManager manager, CoreActivity activity)
        {
            if (activity.Action.CanPerformNow(this).Success && activity.CanPerform().Success)
            {
                // if operating from a DelayedTickStep ...
                if (TurnTick != TurnTick.TurnTracker.LeadingTick)
                {
                    // get action
                    var _act = activity.Action as ActionBase;
                    switch (_act.TimeCost.ActionTimeType)
                    {
                        case TimeType.Free:
                        case TimeType.Opportunistic:
                            break;

                        default:
                            // ... need a new next tick
                            var _newTick = LocalTurnTick.CreateNewLeadTick(TurnTick.TurnTracker);

                            // bound to tick?
                            if (_act.TimeCost.IsBoundToTurnTick)
                            {
                                // bind to new tick
                                TurnTick = _newTick;
                            }

                            // start the new tick
                            TurnTick.TurnTracker.StartOfTick();
                            break;
                    }
                }

                // clear out pending next activity
                if (activity == NextActivity)
                {
                    NextActivity = null;
                }

                // begin processing
                manager?.StartProcess(activity);
            }
        }
        #endregion

        /// <summary>
        /// Used to indicate that the actor is using its turn.
        /// <para>Time-tracking auto-step should be delayed.</para>
        /// <para>EndTurn() will set ActionInquiryPrerequisite.Acted to TRUE.</para>
        /// </summary>
        public bool IsOneTurnAction => _UsingTurn;

        public Creature Creature
            => Actor as Creature;

        /// <summary>Registered Choice Values</summary>
        public Dictionary<string, ChoiceBinder> Choices => _Choices;

        // TODO: reactive

        // TODO: cleave and great cleave...(counterBudgetItem)

        #region public void EndTurn()
        /// <summary>
        /// Called after the client controlling the actor ends the actor's turn.  
        /// <para>Only does useful stuff if actor was using turn.</para>
        /// </summary>
        public void EndTurn()
        {
            if (IsOneTurnAction)
            {
                // enders
                var _enders = (from _itm in BudgetItems
                               where _itm.Value is ITurnEndBudgetItem
                               select new
                               {
                                   _itm.Key,
                                   Value = _itm.Value as ITurnEndBudgetItem
                               }).ToList();

                // removers
                var _removers = (from _e in _enders
                                 where _e.Value.EndTurn()
                                 select _e.Key).ToList();

                // remove!
                foreach (var _key in _removers)
                {
                    BudgetItems.Remove(_key);
                }

                // done
                _UsingTurn = false;
                ClearActivities();
            }
        }
        #endregion

        #region public void EndTick()
        /// <summary>
        /// Called after the tick associated with the budget ends.
        /// <para>May occur on a different tick from EndTurn if actor is delaying.</para>
        /// </summary>
        public void EndTick()
        {
            var _tickEnd = (from _kvp in BudgetItems
                            where _kvp.Value is ITickEndBudgetItem
                            select new
                            {
                                _kvp.Key,
                                TickEnder = _kvp.Value as ITickEndBudgetItem
                            }).ToList();
            var _removers = new List<object>();
            foreach (var _item in _tickEnd)
            {
                if (_item.TickEnder.EndTick())
                {
                    _removers.Add(_item.Key);
                }
            }
            foreach (var _rmv in _removers)
            {
                BudgetItems.Remove(_rmv);
            }
        }
        #endregion

        #region public void ResetBudget()
        public void ResetBudget()
        {
            if (_HeldActivity != null)
            {
                // budget will become available, so unhold the activity ...
                _Tick.TurnTracker.ContextSet.ProcessManager.UnholdProcess(_HeldActivity);

                // ... and free up budget tracking
                Actor.Adjuncts.OfType<SpanActionAdjunct>().FirstOrDefault()?.Eject();
                _HeldActivity = null;
            }

            // reset just occurred, so reset limiters
            _RegularLeft = true;
            _BriefLeft = true;

            var _loc = Actor.GetLocated();
            if (_loc != null)
            {
                NextReset = _loc.Locator.Map.CurrentTime + Round.UnitFactor;
            }

            // reset
            var _resets = (from _itm in BudgetItems
                           where _itm.Value is IResetBudgetItem
                           select new { _itm.Key, Value = _itm.Value as IResetBudgetItem }).ToList();
            var _removers = new Collection<object>();
            foreach (var _item in _resets)
            {
                if (_item.Value.Reset())
                {
                    _removers.Add(_item.Key);
                }
            }
            foreach (var _key in _removers)
            {
                BudgetItems.Remove(_key);
            }

            // stop doing anything else stacked
            ClearActivities();
        }
        #endregion

        /// <summary>Creature can only perform a single action, so it must fulfill the regular requirements</summary>
        public bool IsSingleAction
            => Creature?.Conditions.Contains(Condition.SingleAction) ?? false;

        /// <summary>True if the budget has regular or brief left...</summary>
        public virtual bool CanPerformBrief
            => _RegularLeft || (IsSingleAction ? false : _BriefLeft);

        #region public virtual bool CanPerformRegular
        /// <summary>
        /// true if there is room in the budget for any efforted action.
        /// Also resets if next reset time (by the local map) has passed.
        /// Intended to be called by turn tracker before calling GetActions.
        /// </summary>
        public virtual bool CanPerformRegular
        {
            get
            {
                if (!_RegularLeft)
                {
                    var _currTime = Actor?.GetCurrentTime() ?? 0d;

                    // check for span action
                    if (_HeldActivity != null)
                    {
                        var _spanAdj = Actor.Adjuncts.OfType<SpanActionAdjunct>().FirstOrDefault();
                        if (_spanAdj != null)
                        {
                            // was the activity interrupted?
                            if (_spanAdj.IsInterrupted)
                            {
                                // activity interrupted, next reset is future abort reset time
                                _spanAdj.Eject();
                                (_HeldActivity.Action as IInterruptable)?.Interrupted();
                                _HeldActivity = null;

                                // step abort forward (one round at a time) to next round if we've already gone past abort time
                                while (_currTime > _AbortResetTime)
                                {
                                    _AbortResetTime += Round.UnitFactor;
                                }
                                NextReset = _AbortResetTime;
                            }
                            else if (_currTime < _NextReset)
                            {
                                // can only abort span action
                                return true;
                            }
                        }
                        else
                        {
                            // span adjunct has been removed, indicating self-aborted activity
                            _HeldActivity = null;
                            NextReset = _AbortResetTime;
                        }
                    }

                    // see if reset is possible
                    _RegularLeft = _currTime >= NextReset;
                    if (_RegularLeft)
                    {
                        ResetBudget();
                    }
                }
                return _RegularLeft;
            }
        }
        #endregion

        #region public virtual bool IsTwitchAvailable
        /// <summary>true if the twitch effort hasn't been exhausted this round</summary>
        public virtual bool IsTwitchAvailable
        {
            get
            {
                if (!_CanTwitch)
                {
                    _CanTwitch = (Actor?.GetCurrentTime() ?? 0d) >= _TwitchReady;
                }

                return _CanTwitch;
            }
        }
        #endregion

        /// <summary>true if the brief effort can be used this round</summary>
        protected virtual bool IsBriefAvailable
            => !IsSingleAction && _BriefLeft;

        /// <summary>true if a total action can be started</summary>
        public virtual bool CanPerformTotal
            => IsBriefAvailable && _RegularLeft;

        /// <summary>Running a span, queued up an activity for next round, or not able to perform a brief action (and thus hold up time)</summary>
        public bool CanTimelineFlow
            => (HeldActivity != null) || (NextActivity != null) || !CanPerformBrief;

        /// <summary>True if budget is expecting timeline operations</summary>
        public bool HasTimelineActions
            => Creature.Actions.GetActionProviders().OfType<ITimelineActions>().Any();

        #region public override IEnumerable<ActionResult> GetActions()
        /// <summary>Called to present actions when action is available</summary>
        public override IEnumerable<ActionResult> GetActions()
        {
            if (_HeldActivity != null)
            {
                yield return new ActionResult
                {
                    Provider = null,
                    Action = new AbortSpan(this, @"010"),
                    IsExternal = false
                };
            }
            else
            {
                // tactical action providers
                var _tactProv = new List<ActionResult>();
                var _critter = Creature;
                if (_critter != null)
                {
                    var _cLoc = _critter.GetLocated();
                    if (_cLoc != null)
                    {
                        // regions
                        var _critterRgn = _cLoc.Locator.GeometricRegion;
                        var _reachRgn = _critter.GetMeleeReachRegion();

                        // actions from tactical providers
                        _tactProv.AddRange(from _loc in _cLoc.Locator.MapContext.LocatorsInRegion(_reachRgn, _cLoc.Locator.PlanarPresence)
                                           let _stuff = _loc.ICoreAs<ICore>().Union(_critter.ToEnumerable())
                                           where _loc.EffectLinesToTarget(_critterRgn, ITacticalInquiryHelper.GetITacticals(_stuff.ToArray()).ToArray(),
                                           _loc.PlanarPresence).Any()
                                           from _prov in _loc.AllAccessible(_critter).OfType<ITacticalActionProvider>()
                                           where _critter.Awarenesses.IsActionAware(_prov.ID)
                                           && ((_prov as IAdjunctable)?.Adjuncts.OfType<Attended>().FirstOrDefault()?.AllowTacticalActions(_critter) ?? true)
                                           // TODO: adjuncts that act as ITacticalActionProvider?
                                           from _act in _prov.GetTacticalActions(this)
                                           where _act.CanPerformNow(this).Success
                                           && !_critter.Actions.SuppressAction(this, _prov, _act)
                                           select new ActionResult { Provider = _prov, Action = _act, IsExternal = true });
                    }
                }

                // actions from repetoire
                foreach (var _provided in Actor.Actions.ProvidedActions(this, _tactProv.Select(_r => _r.Provider)).Union(_tactProv))
                {
                    if (_provided.Action is ActionBase _actBase)
                    {
                        var _time = _actBase.TimeCost.ActionTimeType;
                        switch (_time)
                        {
                            case TimeType.FreeOnTurn:
                            case TimeType.Free:
                            case TimeType.SubAction:
                            case TimeType.Twitch:
                            case TimeType.Brief:
                            case TimeType.Regular:
                                yield return _provided;
                                break;

                            case TimeType.Total:
                            case TimeType.Span:
                                // full-round needs brief and regular budget
                                if (CanPerformTotal)
                                {
                                    yield return _provided;
                                }

                                break;

                            case TimeType.TimelineScheduling:
                                // timeline durations only in time tracker
                                if (CanPerformTotal && !(TurnTick?.TurnTracker.IsInitiative ?? true))
                                {
                                    yield return _provided;
                                }

                                break;
                        }
                    }
                }
            }
            yield break;
        }
        #endregion

        #region public bool HasTime(ActionTime time)
        /// <summary>True if the budget can handle an action of the specified time</summary>
        public bool HasTime(ActionTime time)
        {
            switch (time.ActionTimeType)
            {
                case TimeType.Twitch:
                    return IsTwitchAvailable || CanPerformBrief;

                case TimeType.Brief:
                    return CanPerformBrief;

                case TimeType.Regular:
                    return CanPerformRegular;

                case TimeType.Total:
                case TimeType.Span:
                    return CanPerformTotal;

                case TimeType.TimelineScheduling:
                    return CanPerformTotal && !(TurnTick?.TurnTracker.IsInitiative ?? true);

                case TimeType.Reactive:
                    // TODO: reactive...
                    break;
            }
            return false;
        }
        #endregion

        #region protected override void OnRegisterActivity(CoreActivity activity)
        /// <summary>
        /// Updates budget tracking when an activity occurs.  
        /// HeldActivity for spans cannot occur until after this is called
        /// </summary>
        protected override void OnRegisterActivity(CoreActivity activity)
        {
            // TODO: timeline step?

            var _currentTime = Actor?.GetCurrentTime() ?? 0d;
            if (activity.Action is ActionBase _actBase)
            {
                switch (_actBase.TimeCost.ActionTimeType)
                {
                    case TimeType.Twitch:
                        #region Twitch
                        if (IsTwitchAvailable)
                        {
                            // try to use the twitch action first
                            _TwitchReady = _currentTime + Round.UnitFactor;
                        }
                        else if (IsBriefAvailable)
                        {
                            // if it's not available, try the Brief Action
                            // NOTE: this moves the reset time
                            _BriefLeft = false;
                            NextReset = _currentTime + Round.UnitFactor;
                        }
                        else
                        {
                            // otherwise, using the regular action
                            // NOTE: this moves the reset time
                            _RegularLeft = false;
                            NextReset = _currentTime + Round.UnitFactor;
                        }
                        #endregion
                        break;

                    case TimeType.Brief:
                        #region Brief
                        if (IsBriefAvailable)
                        {
                            // NOTE: this moves the reset time
                            _BriefLeft = false;
                            NextReset = _currentTime + Round.UnitFactor;
                        }
                        else
                        {
                            // otherwise, using the regular action
                            // NOTE: this moves the reset time
                            _RegularLeft = false;
                            NextReset = _currentTime + Round.UnitFactor;
                        }
                        _UsingTurn = true;
                        #endregion
                        break;

                    case TimeType.Regular:
                        _RegularLeft = false;
                        NextReset = _currentTime + Round.UnitFactor;
                        _UsingTurn = true;
                        break;

                    case TimeType.Total:
                        _RegularLeft = false;
                        _BriefLeft = false;
                        _UsingTurn = true;
                        NextReset = _currentTime + Round.UnitFactor;
                        break;

                    case TimeType.TimelineScheduling:
                        // timeline only valid without initiative
                        if (TurnTick?.TurnTracker.IsInitiative ?? true)
                        {
                            break;
                        }
                        else
                        {
                            goto case TimeType.Span;
                        }

                    case TimeType.Span:
                        {
                            #region Span
                            // SpanActionAdjunct on actor
                            var _adj = new SpanActionAdjunct(_actBase);
                            Actor.AddAdjunct(_adj);

                            // HoldProcessStep on activity
                            activity.AppendPreEmption(new HoldProcessStep(activity));

                            // hold the activity and set reset time
                            _HeldActivity = activity;
                            NextReset = _currentTime + _actBase.TimeCost.SpanLength;
                            _AbortResetTime = _currentTime + Round.UnitFactor;
                            _RegularLeft = false;
                            _BriefLeft = false;
                            #endregion
                        }
                        break;

                    case TimeType.Reactive:
                        // TODO: reactive...
                        break;
                }

                if (!TurnTick.TurnTracker.IsInitiative)
                {
                    // extend time for TimeTick (if needed)
                    var _timeTick = TurnTick.TurnTracker.GetTrackerStep<LocalTimeStep>()?.TimeTickablePrerequisite;
                    if (_timeTick != null)
                    {
                        switch (_actBase.TimeCost.ActionTimeType)
                        {
                            case TimeType.Twitch:
                                activity.AppendCompletion(new ExtendTickStep(activity, _timeTick, 1000));
                                break;

                            case TimeType.SubAction:
                                activity.AppendCompletion(new ExtendTickStep(activity, _timeTick, 500));
                                break;

                            case TimeType.Brief:
                                activity.AppendCompletion(new ExtendTickStep(activity, _timeTick, 2000));
                                break;

                            case TimeType.Regular:
                                activity.AppendCompletion(new ExtendTickStep(activity, _timeTick, 2000));
                                break;

                            default:
                                break;
                        }
                    }
                }
            }

            // now check if the activity is distractable and whether there is a distraction...
            if (activity.Action is IInterruptable _interruptable)
            {
                // damage might interrupt or distract
                var _damage = activity.GetIncidentalDamages();
                if (_interruptable is IDamageInterrupts _dInt)
                {
                    // if damage interrrupts, then stop the activity
                    if (_damage.Any())
                    {
                        _dInt.Interrupted();
                        activity.Terminate(@"Interrupted with damage");
                    }
                }
                else if (_interruptable is IDistractable _distractable)
                {
                    // if distractable, then interrupted only if concentration check fails
                    var _conBase = _distractable.ConcentrationBase;
                    var _dmg = _damage.Sum(_vt => _vt.Value.Damage) + ContinuousDamage.Total(activity.Actor);
                    if (_dmg > 0)
                    {
                        // one big distraction by damage
                        activity.AppendPreEmption(
                            new DistractionStep(activity, new Interaction(null, typeof(IncidentalDamage), activity.Actor, null),
                                _conBase.EffectiveValue + _dmg));
                    }

                    // other distractions...
                    foreach (var _distraction in activity.Actor.Adjuncts.OfType<Distracted>()
                        .Where(_d => _d.IsActive))
                    {
                        // each other distraction is unique...
                        activity.AppendPreEmption(
                            new DistractionStep(activity, _distraction.Incident, _conBase.EffectiveValue
                                + _distraction.BaseDifficulty.EffectiveValue));
                    }
                }
            }
        }
        #endregion

        #region public void ConsumeBudget(TimeType timeType)
        /// <summary>Consume Twitch, Brief, Regular, or Total Budget</summary>
        public void ConsumeBudget(TimeType timeType)
        {
            var _currentTime = Actor?.GetCurrentTime() ?? 0d;
            switch (timeType)
            {
                case TimeType.Twitch:
                    #region Twitch
                    if (IsTwitchAvailable)
                    {
                        // try to use the twitch action first
                        _TwitchReady = _currentTime + Round.UnitFactor;
                    }
                    else if (IsBriefAvailable)
                    {
                        // if it's not available, try the Brief Action
                        // NOTE: this moves the reset time
                        _BriefLeft = false;
                        NextReset = _currentTime + Round.UnitFactor;
                    }
                    else
                    {
                        // otherwise, using the regular action
                        // NOTE: this moves the reset time
                        _RegularLeft = false;
                        NextReset = _currentTime + Round.UnitFactor;
                    }
                    #endregion
                    break;

                case TimeType.Brief:
                    #region Brief
                    if (IsBriefAvailable)
                    {
                        // NOTE: this moves the reset time
                        _BriefLeft = false;
                        NextReset = _currentTime + Round.UnitFactor;
                    }
                    else
                    {
                        // otherwise, using the regular action
                        // NOTE: this moves the reset time
                        _RegularLeft = false;
                        NextReset = _currentTime + Round.UnitFactor;
                    }
                    _UsingTurn = true;
                    #endregion
                    break;

                case TimeType.Regular:
                    _RegularLeft = false;
                    NextReset = _currentTime + Round.UnitFactor;
                    _UsingTurn = true;
                    break;

                case TimeType.Total:
                    _RegularLeft = false;
                    _BriefLeft = false;
                    _UsingTurn = true;
                    NextReset = _currentTime + Round.UnitFactor;
                    break;
            }
        }
        #endregion

        #region ITrackTime Members

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            // time tracked budget items
            var _timeItems = (from _itm in BudgetItems
                              where _itm.Value is ITrackTime
                              select _itm.Value as ITrackTime).ToList();
            foreach (var _item in _timeItems)
            {
                _item.TrackTime(timeVal, direction);
            }

            // budget reset?
            if ((timeVal >= _NextReset) && (direction == TimeValTransition.Entering))
            {
                // unhold held activity (if any), and reset budget
                ResetBudget();
                if ((NextActivity != null) && !IsInitiative)
                {
                    // start next activity
                    DoAction(TurnTick?.TurnTracker?.ContextSet?.ProcessManager as IkosaProcessManager, NextActivity);
                }
            }
        }

        public double Resolution
            => TurnTick?.TurnTracker.TickResolution ?? 0;

        #endregion

        public Guid ID => Actor.ID;
        public IVolatileValue ActionClassLevel
            => Creature?.ActionClassLevel ?? new Deltable(1);

        #region public LocalActionBudgetInfo ToLocalActionBudgetInfo()
        public LocalActionBudgetInfo ToLocalActionBudgetInfo(Creature creature = null)
        {

            // core
            #region budget item conversions
            BudgetItemInfo _converter(IBudgetItem item)
            {
                switch (item)
                {
                    case MovementBudget _mb:
                        return _mb.ToMovementBudgetInfo();

                    case MovementRangeBudget _mrb:
                        return _mrb.ToMovementRangeBudgetInfo(this);

                    case OpportunityBudget _ob:
                        return _ob.ToCapacityBudgetInfo();

                    case FlightBudget _fb:
                        return _fb.ToFlightBudgetInfo();

                    case AdjunctBudget _ab:
                        return _ab.ToAdjunctBudgetInfo();

                    default:
                        return item?.ToBudgetItemInfo<BudgetItemInfo>();
                }
            };
            #endregion

            if (creature == null)
            {
                var _time = TurnTick.TurnTracker.Map.CurrentTime;
                return new LocalActionBudgetInfo
                {
                    ActorID = ID,
                    ActivityStack = Activities.Select(_ca => _ca.ToActivityInfo()).ToArray(),
                    BudgetItems = BudgetItems.Select(_bi => _converter(_bi.Value)).ToArray(),

                    // local action
                    IsUsingTurn = IsOneTurnAction,
                    IsTwitchAvailable = IsTwitchAvailable,
                    CanPerformBrief = CanPerformBrief,
                    CanPerformRegular = CanPerformRegular,
                    CanPerformTotal = CanPerformTotal,
                    CurrentTime = _time,
                    IsInitiative = IsInitiative,
                    IsFocusedBudget = (TurnTick.TurnTracker.FocusedBudget == this),
                    HeldActivity = HeldActivity?.ToActivityInfo(),
                    NextActivity = NextActivity?.ToActivityInfo(),
                    HoldTimeRemaining = HeldActivity != null ? NextReset - _time : (double?)null,

                    // NOTE: there may not be any actual attacks in budget if a sequencer isn't stacked
                    CanPerformAttack = FullAttackBudget.GetBudget(this)?.AttackStarted ?? false
                };
            }
            else if (creature.Awarenesses[ID] > Senses.AwarenessLevel.UnAware)
            {
                // TODO: if creature hasn't acted in initiative yet, should probably hide this
                return new LocalActionBudgetInfo
                {
                    ActorID = ID,
                    IsUsingTurn = IsOneTurnAction,
                    IsFocusedBudget = (TurnTick.TurnTracker.FocusedBudget == this)
                };
            }
            else
            {
                return new LocalActionBudgetInfo();
            }
        }
        #endregion
    }
}