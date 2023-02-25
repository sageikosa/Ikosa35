using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Fidelity
{
    [Serializable]
    public class ProtectionInfluence : Influence, IActionProvider
    {
        #region construction
        public ProtectionInfluence(Devotion devotion, IPrimaryInfluenceClass influenceClass)
            : base(devotion, influenceClass)
        {
            // been ready since the beginning of time
            _Ready = double.MinValue;
        }
        #endregion

        private double _Ready;
        public double Ready { get => _Ready; set => _Ready = value; }

        public override string Name => @"Protection Influence";
        public override object Clone() => new ProtectionInfluence(Devotion, InfluenceClass);
        public override string Description => @"Grant resistance on next save; expires in one hour if not used.";

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
                    && (_budget.TurnTick != null)
                    && _budget.CanPerformRegular)
                {
                    if (_budget.TurnTick.TurnTracker.Map.CurrentTime >= _Ready)
                        yield return new ProtectionInfluenceWard(this, @"101");
                }
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => InfluenceClass.ToPowerClassInfo();

        #endregion
    }

    [Serializable]
    public class ProtectionInfluenceWard : ActionBase
    {
        public ProtectionInfluenceWard(ProtectionInfluence protection, string orderKey)
            : base(protection, new ActionTime(TimeType.Regular), true, false, orderKey)
        {
        }

        public ProtectionInfluence ProtectionInfluence => Source as ProtectionInfluence;
        public override string Key => @"Influence.Protection";
        public override string DisplayName(CoreActor actor) => @"Boost next save (if used within one hour)";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Touch", activity.Actor, observer, activity.Targets[0].Target as CoreObject);

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
                var _caster = (from _c in ProtectionInfluence.Creature.Classes.OfType<IPrimaryInfluenceClass>()
                               where _c.Influences.Contains(ProtectionInfluence)
                               select _c).FirstOrDefault();
                if (_caster != null)
                {
                    // determine level of boost
                    var _iAct = new Interaction(ProtectionInfluence.Creature, null, null, null);
                    var _prot = new ProtectionInfluenceWardAdjunct(_caster.ClassPowerLevel.QualifiedValue(_iAct));

                    // add as an expiry
                    var _expiry = new Expiry(_prot, _Budget.TurnTick.TurnTracker.Map.CurrentTime + Hour.UnitFactor, TimeValTransition.Entering, Round.UnitFactor);
                    (activity.Targets[0].Target as Creature).AddAdjunct(_expiry);

                    // cannot used for another day
                    ProtectionInfluence.Ready += Day.UnitFactor;
                }
            }
            return null;
        }
        #endregion

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new TouchAim(@"Creature", @"Creature", Lethality.AlwaysNonLethal, ImprovedCriticalTouchFeat.CriticalThreatStart(activity.Actor as Creature),
                null, new FixedRange(1), new FixedRange(1), new MeleeRange(), new CreatureTargetType());
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }

    [Serializable]
    public class ProtectionInfluenceWardAdjunct : Adjunct, IInteractHandler
    {
        public ProtectionInfluenceWardAdjunct(int boost)
            : base(typeof(ProtectionInfluence))
        {
            _Boost = boost;
        }

        private int _Boost;

        #region public void HandleInteraction(Interaction workSet)
        public void HandleInteraction(Interaction workSet)
        {
            if ((workSet.InteractData is SavingThrowData _save)
                && (_save.SaveRoll != null))
            {
                // boost save
                var _resist = new Delta(_Boost, typeof(Resistance), @"Protection Influence");
                _save.SaveRoll.Deltas.Add(_resist);

                // eject when done with the save
                var _expiry = Anchor.Adjuncts.OfType<Expiry>().FirstOrDefault(_ex => _ex.ExpirableAdjuncts.Contains(this));
                if (_expiry != null)
                    _expiry.Eject();
                else
                    Eject();
            }
        }
        #endregion

        #region public IEnumerable<Type> GetInteractionTypes()
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(SavingThrowData);
            yield return typeof(SaveableDamageData);
            yield break;
        }
        #endregion

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => interactType.IsAssignableFrom(typeof(SaveableDamageData))
            ? existingHandler is SaveFromDamageHandler
            : false;

        public override object Clone() { return new ProtectionInfluenceWardAdjunct(_Boost); }

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            if (Anchor is Creature)
                (Anchor as Creature).AddIInteractHandler(this);
        }

        protected override void OnDeactivate(object source)
        {
            if (Anchor is Creature)
                (Anchor as Creature).RemoveIInteractHandler(this);
            base.OnDeactivate(source);
        }
    }
}
