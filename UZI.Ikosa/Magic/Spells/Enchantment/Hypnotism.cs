using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;
using Uzi.Core.Dice;
using System.Collections.ObjectModel;
using Uzi.Ikosa.Time;
using Uzi.Visualize;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class Hypnotism : SpellDef, ISpellMode, IDurableCapable, ISaveCapable, IRegionCapable, IBurstCaptureCapable
    {
        public override string DisplayName => @"Hypnotism";
        public override string Description => @"Fascinates creatures and alters their attitudes";
        public override MagicStyle MagicStyle => new Enchantment(Enchantment.SubEnchantment.Compulsion);

        public override IEnumerable<SpellComponent> ArcaneComponents
            => YieldComponents(true, true, false, false, false);

        public override IEnumerable<ISpellMode> SpellModes
            => this.ToEnumerable();

        // ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new PersonalStartAim(@"Self", @"Self", actor);
            yield return new RollAim(@"Roll.PowerDice", @"Total Power Dice Affected", new DiceRoller(2, 4));
            yield return new RollAim(@"Roll.Duration", @"Rounds Fascinated if Affected", new DiceRoller(2, 4));
            yield return new CharacterStringAim(@"Suggestion", @"Attitude Suggestion", FixedRange.One, new FixedRange(200));
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => false;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            // NOTE: for Hypnotism, there's a bit of non-standardness involved
            // NOTE: however, most features of the spell can be mapped through standard features

            // get burst geometry since range is near, every creature in near range is a potential target
            var _target = deliver.TargetingProcess.Targets.FirstOrDefault(_t => _t.Key.Equals(@"Self")) as LocationTarget;
            var _sphere = new Geometry(new SphereBuilder(Convert.ToInt32(
                deliver.PowerUse.CapabilityRoot.GetCapability<IRegionCapable>()
                .Dimensions(deliver.Actor, deliver.PowerUse.PowerActionSource.CasterLevel)
                .FirstOrDefault() / 5)), new Intersection(_target.Location), true);

            // after this, deliver will have multiple following steps for everything that passed burst filter
            SpellDef.DeliverBurstToMultipleSteps(deliver, new Intersection(_target.Location), _sphere, null);

            // get actual duration from aim target
            var _tDuration = deliver.TargetingProcess.Targets.FirstOrDefault(_t => _t.Key.Equals(@"Roll.Duration")) as ValueTarget<int>;
            var _duration = Convert.ToDouble(_tDuration.Value);

            // adjust duration of each captured target's transmitted durable effect
            foreach (var _durable in from _multi in deliver.FollowingSteps.OfType<MultiNextStep>()
                                     from _next in _multi.FollowingSteps.OfType<PowerApplyStep<SpellSource>>()
                                     let _effect = _next.DeliveryInteraction.InteractData as MagicPowerEffectTransit<SpellSource>
                                     where _effect != null
                                     from _dur in _effect.MagicPowerEffects.OfType<DurableMagicEffect>()
                                     select _dur)
            {
                // NOTE: duration is part of durable magic effect
                _durable.ExpirationTime = (deliver.Actor?.GetCurrentTime() ?? 0d) + _duration;

                // additional targets are added to be processed when effect is Activated
                _durable.AllTargets.Add(new ValueTarget<Guid>(@"AttitudeID", deliver.Actor.ID));
                foreach (var _suggest in deliver.TargetingProcess.Targets.Where(_t => _t.Key.Equals(@"Suggestion")))
                {
                    _durable.AllTargets.Add(_suggest);
                }
            }
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDurableMagicEffects(apply);
        }

        // IDurableCapable Members
        public IEnumerable<int> DurableSubModes
            => 0.ToEnumerable();

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            if (source is MagicPowerEffect _spellEffect)
            {
                // fascinate the creature
                var _fascinate = new FascinatedEffect(source);
                target.AddAdjunct(_fascinate);

                // alter attitude and indicate a suggestion (for game-mastering)
                if ((_spellEffect.AllTargets.FirstOrDefault(_t => _t.Key.Equals(@"AttitudeID")) is ValueTarget<Guid> _attitudeID)
                    && (_spellEffect.AllTargets.FirstOrDefault(_t => _t.Key.Equals(@"Suggestion")) is CharacterStringTarget _suggest))
                {
                    Attitude _newAttitude = target.AttitudeTowards(_attitudeID.Value);
                    if (_newAttitude <= Attitude.Unbiased)
                        _newAttitude += 2;
                    else
                        _newAttitude = Attitude.Helpful;

                    // NOTE: no expiration...attitude stays past duration
                    target.AddAdjunct(new AttitudeAdjunct(null, _suggest.CharacterString, _attitudeID.Value, _newAttitude));
                }

                return _fascinate;
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            if (source.ActiveAdjunctObject is FascinatedEffect _fascinate)
            {
                // no longer fascinated
                // NOTE: attitude stays past the duration
                target.RemoveAdjunct(_fascinate);
            }
        }

        public bool IsDismissable(int subMode)
            => true;

        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode)
            => @"Save.Will";

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
            => Enumerable.Empty<StepPrerequisite>();

        public DurationRule DurationRule(int subMode)
        {
            // NOTE: this is the maximum, but is overridden in burst capture
            return new DurationRule(DurationType.Span, new SpanRulePart(8, new Round()));
        }

        // ISaveCapable Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource, Interaction workSet, string saveKey)
        {
            var _difficulty = SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target);
            if ((actor as Creature)?.GetLocalActionBudget()?.TurnTick.TurnTracker.IsInitiative ?? false)
            {
                // spell easier to resist in turn tracker
                _difficulty.AddDelta(@"Turn Tracker", -2);
                _difficulty.Result -= 2;
            }
            return new SaveMode(SaveType.Will, SaveEffect.Negates, _difficulty);
        }

        #region IRegionMode Members

        public IEnumerable<double> Dimensions(CoreActor actor, int casterLevel)
        {
            yield return new NearRange().EffectiveRange(actor, casterLevel);
            yield return 15;
            yield break;
        }

        #endregion

        // IBurstCapture Members

        #region public IEnumerable<CoreStep> Capture(BurstCapture burst, Locator locator)
        public IEnumerable<CoreStep> Capture(BurstCapture burst, Locator locator)
        {
            // get the burst as a spell burst
            if (burst is PowerBurstCapture<SpellSource> _spellBurst)
            {
                // get farthest distance in cluster
                var _farthest = _spellBurst.Activation.PowerUse.CapabilityRoot.GetCapability<IRegionCapable>().Dimensions(_spellBurst.Activation.Actor, _spellBurst.Activation.PowerUse.PowerActionSource.CasterLevel).Last();

                // get current remaining power dice
                var _power = _spellBurst.Context.FirstOrDefault(_t => _t.Key.Equals(@"Roll.PowerDice")) as ValueTarget<decimal>;
                var _remaining = _power.Value;
                if (_remaining > 0)
                {
                    // get captured locators
                    var _cluster = _spellBurst.Context.FirstOrDefault(_t => _t.Key.Equals(@"Locators")) as ValueTarget<Collection<Locator>>;
                    var _actorID = _spellBurst.Activation.Actor?.ID ?? Guid.Empty;
                    foreach (var _step in SpellDef.DeliverDurableDirectFromBurst(_spellBurst, locator,
                        (loc, core) =>
                        {
                            // check for living creature under our remaining power dice level
                            if ((core is Creature _creature)
                            && _creature.CreatureType.IsLiving
                            && _creature.Awarenesses[_actorID] == Senses.AwarenessLevel.Aware
                            && (_creature.AdvancementLog.PowerDiceCount <= _remaining))  // TODO: or can hear...
                            {
                                // if not already determined this locator is OK, must pass the test
                                if (!_cluster.Value.Contains(loc))
                                {
                                    // distance to selected targets
                                    var _far = (from _prev in _cluster.Value
                                                let _d = _prev.GeometricRegion.NearDistance(loc.GeometricRegion)
                                                orderby _d descending
                                                select new { Loc = _prev, Dist = _d }).FirstOrDefault();
                                    if ((_far != null) && (_far.Dist > _farthest))
                                    {
                                        // too far to use
                                        return false;
                                    }

                                    // account for the locator
                                    _cluster.Value.Add(loc);
                                }
                                return true;
                            }
                            return false;
                        },
                        (step) =>
                        {
                            if (step is PowerApplyStep<SpellSource> _applyStep)
                            {
                                var _feedback = _applyStep.DeliveryInteraction.Feedback.OfType<PowerActionTransitFeedback<SpellSource>>().FirstOrDefault();
                                if ((_feedback != null) && _feedback.Success)
                                {
                                    // if the effect was successfully applied, decrease remaining capacity
                                    var _critter = _applyStep.DeliveryInteraction.Target as Creature;
                                    _remaining -= _critter.AdvancementLog.PowerDiceCount;
                                }
                            }
                            return true;
                        }, 0))
                    {
                        yield return _step;
                    }

                    // update the context (power dice remaining)
                    if (_remaining != _power.Value)
                    {
                        _power.Value = _remaining;
                    }
                }
            }
            yield break;
        }
        #endregion

        #region public void PostInitialize(BurstCapture burst)
        public void PostInitialize(BurstCapture burst)
        {
            // get the burst as a spell burst
            if (burst is PowerBurstCapture<SpellSource> _spellBurst)
            {
                if (_spellBurst.Activation.TargetingProcess.Targets.FirstOrDefault(_t => _t.Key.Equals(@"Roll.PowerDice")) is ValueTarget<int> _power)
                {
                    // track power dice so we can change it as we consume it...
                    var _dPower = new ValueTarget<decimal>(_power.Key, Convert.ToDecimal(_power.Value));
                    _spellBurst.Context.Add(_dPower);
                }

                // something to track the locators as well...
                _spellBurst.Context.Add(new ValueTarget<Collection<Locator>>(@"Locators", new Collection<Locator>()));
            }
        }
        #endregion

        #region public IEnumerable<Locator> ProcessOrder(BurstCapture burst, IEnumerable<Locator> selection)
        public IEnumerable<Locator> ProcessOrder(BurstCapture burst, IEnumerable<Locator> selection)
        {
            // get the burst as a spell burst
            if (burst is PowerBurstCapture<SpellSource> _spellBurst)
            {
                // capture locators, sorted by power level of chief and distance from near point
                return BurstCapture.OrderWeakestClosest(selection, burst.Origin.GetPoint3D());
            }
            return selection;
        }
        #endregion
    }
}
