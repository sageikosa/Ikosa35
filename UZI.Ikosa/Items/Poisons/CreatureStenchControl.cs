using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class CreatureStenchControl : Adjunct, IActionProvider, ITrackTime, IInteractHandler, IActionSource
    {
        public CreatureStenchControl(Stench stench, int lingerRounds, bool actions)
            : base(stench)
        {
            _Actions = actions;
            _Linger = lingerRounds;
            _EndTime = null;
        }

        #region state
        private bool _Actions;
        private int _Linger;
        private double? _EndTime;
        #endregion

        public virtual IVolatileValue ActionClassLevel => new Deltable(1);

        public Stench Stench => Source as Stench;
        public int LingerRounds => _Linger;
        public bool Actions => _Actions;

        public void EnableStench()
        {
            _EndTime = null;
            if (!Stench.IsActive)
            {
                Stench.Activation = new Activation(this, true);
            }
        }

        public void DisableStench()
        {
            _EndTime ??= (Anchor?.GetCurrentTime() ?? 0d) + LingerRounds;
        }

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            Anchor.AddAdjunct(Stench);
            (Anchor as CoreObject)?.AddIInteractHandler(this);
        }

        protected override void OnDeactivate(object source)
        {
            (Anchor as CoreObject)?.RemoveIInteractHandler(this);
            Stench?.Eject();
            base.OnDeactivate(source);
        }

        public override object Clone()
            => new CreatureStenchControl(Stench, LingerRounds, Actions);

        protected virtual bool CanDisable
            => true;

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (Actions)
            {
                if (Stench.IsActive && CanDisable && (_EndTime != null)
                    && budget is LocalActionBudget _budget
                    && (!_budget.IsInitiative || (_budget.TurnTick?.TurnTracker.FocusedBudget == _budget)))
                {
                    // can freely deactivate stench on turn
                    yield return new CreatureStenchActivation(this, new ActionTime(TimeType.FreeOnTurn), false);
                }
                else if (!Stench.IsActive || (_EndTime != null))
                {
                    // can freely activate stench whenever
                    yield return new CreatureStenchActivation(this, new ActionTime(TimeType.Free), true);
                }
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => new AdjunctInfo($@"Stench Control: {Stench.PoisonProvider.Name}", ID);

        // ITrackTime: to handle lag between deactivation and actual end of stench
        public double Resolution => Round.UnitFactor;

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if ((_EndTime != null) && (timeVal >= _EndTime))
            {
                Stench.Activation = new Activation(this, false);
                _EndTime = null;
            }
        }

        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
            if (IsActive
                && (workSet?.InteractData as AddAdjunctData)?.Adjunct is DeadEffect _addDead)
            {
                // if dead is added, then disable stench
                DisableStench();
            }
            else if (IsActive
                && !Actions
                && (workSet?.InteractData as RemoveAdjunctData)?.Adjunct is DeadEffect _removeDead)
            {
                // if dead is removed, and actions cannot control, then enable stench
                EnableStench();
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(AddAdjunctData);
            yield return typeof(RemoveAdjunctData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => true;

        #endregion
    }
}
