using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Fidelity
{
    [Serializable]
    public class StrengthInfluence : Influence, IActionProvider
    {
        public StrengthInfluence(Devotion devotion, IPrimaryInfluenceClass influenceClass)
            : base(devotion, influenceClass)
        {
            // been ready since the beginning of time
            _Ready = double.MinValue;
        }

        private double _Ready;
        public double Ready { get { return _Ready; } set { _Ready = value; } }

        public override string Name { get { return @"Strength Influence"; } }
        public override object Clone() { return new StrengthInfluence(Devotion, InfluenceClass); }
        public override string Description
        {
            get { return @"Enhancement delta to strength as a free action 1/day; duration = 1 round, value = cleric level"; }
        }

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            var _critter = Creature;
            if (_critter != null)
            {
                _critter.Actions.Providers.Add(this, this);
            }
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            var _critter = Creature;
            if (_critter != null)
            {
                _critter.Actions.Providers.Remove(this);
            }
            base.OnDeactivate(source);
        }
        #endregion

        #region IActionProvider Members

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (InfluenceClass.IsPowerClassActive)
            {
                if ((budget is LocalActionBudget _budget)
                    && (_budget.TurnTick != null))
                {
                    if (_budget.TurnTick.TurnTracker.Map.CurrentTime >= _Ready)
                        yield return new StrengthInfluenceBoost(this, @"200");
                }
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => InfluenceClass.ToPowerClassInfo();

        #endregion
    }

    [Serializable]
    public class StrengthInfluenceBoost : ActionBase
    {
        #region construction
        public StrengthInfluenceBoost(StrengthInfluence influence, string orderKey)
            : base(influence, new ActionTime(TimeType.Free), false, false, orderKey)
        {
        }
        #endregion

        public StrengthInfluence StrengthInfluence => Source as StrengthInfluence;
        public override string Key => @"Influence.Strength";
        public override string DisplayName(CoreActor actor) => @"Boost strength for 1 round";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => null;

        public override ActivityResponse CanPerformNow(CoreActionBudget budget)
        {
            _Budget = budget as LocalActionBudget;
            return base.CanPerformNow(budget);
        }

        #region protected override CoreStep OnPerformActivity(CoreActivity activity)
        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            if (_Budget != null)
            {
                var _caster = (from _c in StrengthInfluence.Creature.Classes.OfType<IPrimaryInfluenceClass>()
                               where _c.Influences.Contains(StrengthInfluence)
                               select _c).FirstOrDefault();
                if (_caster != null)
                {
                    // get delta from caster level, and add it to STRENGTH
                    var _iAct = new Interaction(StrengthInfluence.Creature, null, null, null);
                    var _strength = new Delta(_caster.ClassPowerLevel.QualifiedValue(_iAct), typeof(Enhancement), @"Strength Influence");
                    StrengthInfluence.Creature.Abilities.Strength.Deltas.Add(_strength);

                    // prepare delta to terminate ...
                    var _term = new DeltaTerminator(_strength);

                    // ... once a round has passed
                    var _exp = new Expiry(_term, _Budget.TurnTick.TurnTracker.Map.CurrentTime + Round.UnitFactor, TimeValTransition.Entering, Round.UnitFactor);
                    StrengthInfluence.Creature.AddAdjunct(_exp);

                    // wait until next day to use again
                    StrengthInfluence.Ready += Day.UnitFactor;
                }
            }

            // does not need a following step
            return null;
        }
        #endregion

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity) { yield break; }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}