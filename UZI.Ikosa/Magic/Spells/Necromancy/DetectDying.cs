using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Actions;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Universal;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class DetectDying : DetectBase<DetectDyingAdjunct>
    {
        public override MagicStyle MagicStyle => new Necromancy();
        public override bool IsDismissable(int subMode) => false;
        public override DurationRule DurationRule(int subMode) => new DurationRule(DurationType.Span, new SpanRulePart(10, new Minute(), 1));
        public override string DisplayName => @"Detect Dying";
        public override string Description => @"Determines how near death nearby creatures are, or whether they are undead.";

        #region public override IEnumerable<Descriptor> Descriptors { get; }
        public override IEnumerable<Descriptor> Descriptors
        {
            get
            {
                yield return new Evil();
                yield break;
            }
        }
        #endregion

        #region public override IEnumerable<double> Dimensions(CoreActor actor, int casterLevel)
        public override IEnumerable<double> Dimensions(CoreActor actor, int casterLevel)
        {
            yield return 30;
            yield break;
        }
        #endregion

        protected override DetectDyingAdjunct NewEffect(MagicPowerEffect source, IAdjunctable target)
        {
            foreach (var _range in source.CapabilityRoot.GetCapability<IRegionCapable>().Dimensions(null, source.CasterLevel))
            {
                return new DetectDyingAdjunct(source.MagicPowerActionSource, _range);
            }

            return new DetectDyingAdjunct(source.MagicPowerActionSource, 30);
        }
    }

    [Serializable]
    public class DetectDyingAdjunct : DetectAdjunctBase, IDetectExtraInfo
    {
        public DetectDyingAdjunct(INamedActionSource source, double range) :
            base(source, range)
        {
        }

        #region public override bool DoesLocatorMatch(Locator testLocator)
        public override bool DoesLocatorMatch(Locator testLocator)
        {
            // must contain a living creature, or an undead creature
            return testLocator.AllConnectedOf<Creature>().Any();
        }
        #endregion

        #region Actions to Emit (all the same: no progression)
        protected override IEnumerable<ActionBase> PrimeActions(LocalActionBudget budget)
        {
            var _budget = budget as LocalActionBudget;
            var _counter = CounterBudgetItem.GetCounter(budget, typeof(DetectDyingAction), 1);
            if (_counter.CanUse)
            {
                yield return new DetectDyingAction(this, @"101");
            }

            yield break;
        }

        protected override IEnumerable<ActionBase> SecondActions(LocalActionBudget budget)
        {
            return PrimeActions(budget);
        }

        protected override IEnumerable<ActionBase> ThirdActions(LocalActionBudget budget)
        {
            return PrimeActions(budget);
        }
        #endregion

        #region IDetectExtraInfo Members

        public IEnumerable<ExtraInfo> GetExtraInformation(CoreActivity activity, Locator testLocator)
        {
            var _actor = activity.Actor as Creature;
            var _aura = AuraStrength.None;
            foreach (var _critter in testLocator.AllConnectedOf<Creature>())
            {
                Description _descr;
                if (_critter.CreatureType.IsLiving)
                {
                    if (_critter.HealthPoints.CurrentValue <= _critter.HealthPoints.DeadValue.EffectiveValue)
                    {
                        _descr = new Description(@"Death Status", @"Dead");
                        _aura = AuraStrength.Strong;
                    }
                    else if (_critter.HealthPoints.CurrentValue <= 3)
                    {
                        _descr = new Description(@"Death Status", @"Frail");
                        _aura = AuraStrength.Moderate;
                    }
                    else
                    {
                        _descr = new Description(@"Death Status", @"Refuses to die (yet)");
                        _aura = AuraStrength.Faint;
                    }
                }
                else if (_critter.CreatureType is UndeadType)
                {
                    _descr = new Description(@"Death Status", @"Undead");
                    _aura = AuraStrength.Overwhelming;
                }
                else
                {
                    _descr = new Description(@"Death Status", @"Not living");
                    _aura = AuraStrength.None;
                }

                // get the divination information
                var _info = new Informable(_descr);
                var _marker = new ExtraInfoMarker(GetExtraInfoSource(), _info,
                    testLocator.GeometricRegion, _actor.Awarenesses.GetAwarenessLevel(_critter.ID) < AwarenessLevel.Aware,
                    _aura, null);
                yield return _marker;

            }
            yield break;
        }

        #endregion

        public override Info GetProviderInfo(CoreActionBudget budget)
            => new AdjunctInfo(@"Detect Dying", ID);

        public override object Clone()
            => new DetectDyingAdjunct(Source as INamedActionSource, Range);
    }

    [Serializable, SourceInfo(@"Detect Dying")]
    public class DetectDyingAction : DetectDivinationAction<DetectDyingAdjunct>
    {
        public DetectDyingAction(DetectDyingAdjunct detector, string orderKey)
            : base(@"DetectDying", @"Determine how near death creatures are", new ActionTime(TimeType.FreeOnTurn), detector, orderKey)
        {
        }

        public override ActivityResponse CanPerformNow(CoreActionBudget budget)
        {
            var _budget = budget as LocalActionBudget;
            var _counter = CounterBudgetItem.GetCounter(budget, typeof(DetectDyingAction), 1);
            return new ActivityResponse(_counter.CanUse);
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            // establish the cone volume on the detector...
            Detector.Volume =
                activity.Targets.Where(_t => _t.Key.Equals(@"Cone", StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault() as GeometricTarget;
            return base.OnPerformActivity(activity);
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            // for multi-action detects, this is handled in an earlier action and the targetted volume is bound to the detector ...
            // ... however, here, we catch the volume and set it on the detector before calling the base functionality of OnPerformActivity
            yield return new PersonalConicAim(@"Cone", @"Directional cone", new FixedRange(Detector.Range), activity.Actor);
            yield break;
        }
    }
}
