using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Base class for actions within the Ikosa framework</summary>
    [Serializable]
    public abstract class ActionBase : CoreAction, IActionSource
    {
        #region Construction
        protected ActionBase(IActionSource powerLevel, ActionTime cost, bool provokesMelee, bool provokesTarget, string orderKey)
            : this(powerLevel, cost, cost, provokesMelee, provokesTarget, orderKey)
        {
        }

        protected ActionBase(IActionSource powerLevel, ActionTime needed, ActionTime cost, bool provokesMelee, bool provokesTarget, string orderKey)
            : base(powerLevel)
        {
            if ((cost != null) && (needed != null))
            {
                if (cost > needed)
                {
                    // always need at least the cost (sometimes more)
                    needed = cost;
                }
            }

            _Time = cost;
            _NeedsTime = needed ?? cost;
            _Melee = provokesMelee;
            _Target = provokesTarget;
            _Order = orderKey;
        }
        #endregion

        #region state
        protected LocalActionBudget _Budget = null;
        protected ActionTime _Time;
        protected ActionTime _NeedsTime;
        protected bool _Melee;
        protected bool _Target;
        protected string _Order;
        protected int _InitHealth;
        #endregion

        #region public override ActivityResponse CanPerformNow(CoreActionBudget budget)
        /// <summary>Checks budget available effort levels versus time needed</summary>
        public override ActivityResponse CanPerformNow(CoreActionBudget budget)
        {
            _Budget = budget as LocalActionBudget;
            if (_Budget != null)
            {
                var _regular = _Budget.CanPerformRegular;
                var _needed = TimeNeeded;
                switch (_needed.ActionTimeType)
                {
                    case TimeType.Twitch:
                        return new ActivityResponse(_Budget.CanPerformBrief || _Budget.IsTwitchAvailable);
                    case TimeType.Brief:
                        return new ActivityResponse(_Budget.CanPerformBrief);
                    case TimeType.Regular:
                        return new ActivityResponse(_regular);

                    case TimeType.TimelineScheduling:
                        if (!(_Budget?.TurnTick?.TurnTracker.IsInitiative ?? true))
                        {
                            return new ActivityResponse(_Budget.CanPerformTotal);
                        }

                        return new ActivityResponse(false);

                    case TimeType.Total:
                    case TimeType.Span:
                        return new ActivityResponse(_Budget.CanPerformTotal);
                }
            }
            return base.CanPerformNow(budget);
        }
        #endregion

        #region public ActionTime TimeCost { get; set; }
        public ActionTime TimeCost
        {
            get => _Time;
            set
            {
                _Time = value;
                if (TimeCost > TimeNeeded)
                {
                    _NeedsTime = TimeCost;
                }
            }
        }
        #endregion

        protected override ActivityResponse OnCanPerformActivity(CoreActivity activity)
        {
            if (activity?.Actor is Creature _critter)
            {
                _InitHealth = _critter.HealthPoints.CurrentValue;
            }
            return base.OnCanPerformActivity(activity);
        }

        public int InitialHealthPoints => _InitHealth;
        public LocalActionBudget Budget => _Budget;
        public ActionTime TimeNeeded => _NeedsTime;
        public virtual bool IsChoice => false;
        protected virtual string TimeCostString => TimeCost.ToString();

        public override string WorkshopName => _Budget!=null? DisplayName(_Budget.Actor) : DisplayName(null);

        /// <summary>Indicates that performing the action provokes a melee attack</summary>
        public bool ProvokesMelee => _Melee;

        /// <summary>Indicates that performing the action provokes a melee attack from the target of the action</summary>
        public bool ProvokesTarget => _Target;

        /// <summary>Override to indicate whether the potential target is an actual target</summary>
        public abstract bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget);
        //=> false;

        /// <summary>Indicates the action shows on the combat list of the character sheet</summary>
        public virtual bool CombatList => false;

        /// <summary>Indicates the action is considered harmless (for triggers that are concerned with this)</summary>
        public virtual bool IsHarmless => true;

        /// <summary>Use head-up client UI instead of menu</summary>
        public virtual string HeadsUpMode => string.Empty;

        /// <summary>Indicates that the action is strenuous, default is true if timeType >= Regular</summary>
        public virtual bool IsStrenuous => TimeNeeded.ActionTimeType >= TimeType.Regular;

        /// <summary>If IsMental, can be used while paralyzed, pinned, or otherwise restrained</summary>
        public virtual bool IsMental => false;

        public string OrderKey => _Order;

        #region public CoreActivity GetActivity(CoreActor actor, AimTargetInfo[] targets)
        public CoreActivity GetActivity(CoreActor actor, AimTargetInfo[] targets)
        {
            var _loc = actor?.GetLocated();
            if (_loc != null)
            {
                return new CoreActivity(actor, this,
                    ConvertTargets(actor, targets, _loc.Locator.Map).ToList());
            }
            return null;
        }
        #endregion

        #region public override void ActivityFinalization(CoreActivity activity, bool deactivated)
        public override void ActivityFinalization(CoreActivity activity, bool deactivated)
        {
            if (activity?.Targets.OfType<FinalizeTarget>().Any(_ft => _ft.SerialStateDependent) ?? false)
            {
                activity?.Actor?.IncreaseSerialState();
            }
            foreach (var _final in activity?.Targets.OfType<FinalizeTarget>())
            {
                _final.DoActionFinalized(activity, deactivated);
            }
            base.ActivityFinalization(activity, deactivated);
        }
        #endregion

        /// <summary>Creates a sound participant and tracks it for activity finalization eject</summary>
        protected SoundParticipant GetActionSoundParticipant(CoreActivity activity, SoundRef soundRef)
        {
            var _participant = new SoundParticipant(GetType(), GetActionSoundGroup(soundRef));
            activity.Targets.Add(new AdjunctFinalizeTarget(_participant, true));
            return _participant;
        }

        protected SoundGroup GetActionSoundGroup(SoundRef soundRef)
            => new SoundGroup(this, soundRef);

        #region IActionSource Members
        /// <summary>ID of Underlying ActionSource</summary>
        public Guid ID => ActionSource.ID;
        public IVolatileValue ActionClassLevel => ActionSource.ActionClassLevel;
        #endregion

        #region ToActionInfo
        protected AInfo ToInfo<AInfo>(IActionProvider provider, bool isExternal, CoreActor actor)
            where AInfo : ActionInfo, new()
        {
            string _orderKey()
            {
                if (provider is ISlottedItem _sItem)
                {
                    if (_sItem.MainSlot is HoldingSlot _hold)
                    {
                        // holding slots first
                        return $@"1.{_sItem.SlotType}.{_sItem.MainSlot?.SubType}";
                    }
                    if (_sItem.MainSlot?.MagicalSlot ?? false)
                    {
                        // then magical slots
                        return $@"2.{_sItem.SlotType}.{_sItem.MainSlot?.SubType}";
                    }
                    // non-magical slots "last"
                    return $@"3.{_sItem.SlotType}.{_sItem.MainSlot?.SubType}";
                }

                // not a slot...other options...?
                return @"~";
            }
            var _info = new AInfo()
            {
                ID = ID,
                Key = Key,
                OrderKey = OrderKey,
                DisplayName = DisplayName(actor),
                Description = Description,
                ProvokesMelee = ProvokesMelee,
                ProvokesTarget = ProvokesTarget,
                IsExternalProvider = isExternal,
                IsHarmless = IsHarmless,
                IsDistractable = (this as IDistractable) != null,
                TimeCost = TimeCostString,
                TimeTypeVal = (byte)TimeCost.ActionTimeType,
                HeadsUpMode = HeadsUpMode,
                IsChoice = IsChoice,
                AimingModes =
                (from _aimMode in AimingMode(new CoreActivity(actor, this, null))
                 select _aimMode.ToAimingModeInfo(this, actor)).ToArray()
            };
            if (provider != null)
            {
                _info.Provider = new ActionProviderInfo
                {
                    Message = provider.SourceName(),
                    PresenterID = provider.PresenterID,
                    ProviderInfo = provider.GetProviderInfo(Budget),
                    ID = provider.ID,
                    OrderKey = _orderKey()
                };
                _info.IsContextMenuOnly = (provider as ITacticalActionProvider)?.IsContextMenuOnly ?? false;
            }
            else
            {
                _info.Provider = new ActionProviderInfo
                {
                    ID = ID,
                    PresenterID = ID,
                    Message = DisplayName(actor),
                    OrderKey = @"~~"
                };
                _info.IsContextMenuOnly = false;
            }
            return _info;
        }

        public virtual ActionInfo ToActionInfo(IActionProvider provider, bool isExternal, CoreActor actor)
        {
            return ToInfo<ActionInfo>(provider, isExternal, actor);
        }
        #endregion
    }
}