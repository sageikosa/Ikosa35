using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;
using Uzi.Visualize;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class Sleep : SpellDef, ISpellMode, IDurableCapable, ISaveCapable, IRegionCapable, IBurstCaptureCapable
    {
        public override string DisplayName => @"Sleep";
        public override string Description => @"Puts some weak creatures to sleep";
        public override MagicStyle MagicStyle => new Enchantment(Enchantment.SubEnchantment.Compulsion);

        public override IEnumerable<Descriptor> Descriptors
            => new MindAffecting().ToEnumerable();

        public override IEnumerable<ISpellMode> SpellModes
            => this.ToEnumerable();

        #region public override IEnumerable<SpellComponent> ArcaneComponents { get; }
        public override IEnumerable<SpellComponent> ArcaneComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield return new MaterialComponent();
                yield break;
            }
        }
        #endregion

        #region ISpellMode Members

        public virtual IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new LocationAim(@"Origin", @"Burst Origin", LocationAimMode.Any, FixedRange.One, FixedRange.One, new MediumRange());
            yield break;
        }

        public bool AllowsSpellResistance
            => true;

        public bool IsHarmless
            => false;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            // get burst geometry
            var _target = deliver.TargetingProcess.Targets[0] as LocationTarget;
            var _sphere = new Geometry(new SphereBuilder(Convert.ToInt32(
                deliver.PowerUse.CapabilityRoot.GetCapability<IRegionCapable>()
                .Dimensions(deliver.Actor, deliver.PowerUse.PowerActionSource.CasterLevel)
                .FirstOrDefault() / 5)), new Intersection(_target.Location), true);
            SpellDef.DeliverBurstToMultipleSteps(deliver, new Intersection(_target.Location), _sphere, null);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDurableMagicEffects(apply);
        }

        #endregion

        #region IDurableMode Members

        public IEnumerable<int> DurableSubModes
            => 0.ToEnumerable();

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            var _sleep = new SleepEffect(source);
            target.AddAdjunct(_sleep);
            return _sleep;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            if (source.ActiveAdjunctObject is SleepEffect _sleep)
            {
                target.RemoveAdjunct(_sleep);
            }
        }

        public bool IsDismissable(int subMode)
            => false;

        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode)
            => @"Save.Will";

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Minute(), 1));

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
            => Enumerable.Empty<StepPrerequisite>();

        #endregion

        // ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource, Interaction workSet, string saveKey)
            => new SaveMode(SaveType.Will, SaveEffect.Negates, SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));

        // IRegionMode Members
        public virtual IEnumerable<double> Dimensions(CoreActor actor, int casterLevel)
            => (10d).ToEnumerable();

        #region IBurstCapture Members
        public IEnumerable<CoreStep> Capture(BurstCapture burst, Locator locator)
        {
            // get the burst
            if (burst is PowerBurstCapture<SpellSource> _spellBurst)
            {
                var _actor = _spellBurst.Activation.Actor as Creature;

                // get current remaining power dice
                var _power = _spellBurst.Context.FirstOrDefault(_t => _t.Key.Equals(@"PowerDice")) as ValueTarget<decimal>;
                var _remaining = _power.Value;
                if (_remaining > 0)
                {
                    // everything directly on the locator (for now)
                    foreach (var _step in SpellDef.DeliverDurableDirectFromBurst(_spellBurst, locator,
                        (loc, core) =>
                        {
                            // check for living creature under our remaining power dice level
                            if ((core is Creature _creature) && (_creature.CreatureType.IsLiving))
                            {
                                // no effect on unconscious creatures...
                                if (!_creature.Conditions.Contains(Condition.Unconscious))
                                {
                                    if (_creature.AdvancementLog.PowerDiceCount <= _remaining)
                                    {
                                        return true;
                                    }
                                }
                            }
                            return false;
                        },
                        (step) =>
                        {
                            if (step is PowerApplyStep<SpellSource>)
                            {
                                var _applyStep = step as PowerApplyStep<SpellSource>;
                                var _feedback = _applyStep.DeliveryInteraction.Feedback.OfType<PowerActionTransitFeedback<SpellSource>>().FirstOrDefault();
                                if ((_feedback != null) && _feedback.Success)
                                {
                                    // if the effect was successfully applied, decrease remaining capacity
                                    var _critter = _applyStep.DeliveryInteraction.Target as Creature;
                                    _remaining -= _critter.AdvancementLog.PowerDiceCount;
                                }
                            }
                            return true;
                        }
                        , 0))
                        yield return _step;

                    // update the context (power dice remaining)
                    if (_remaining != _power.Value)
                    {
                        _power.Value = _remaining;
                    }
                }
            }
            yield break;
        }

        protected virtual decimal GetPowerDiceEffected()
            => 4m;

        public void PostInitialize(BurstCapture burst)
        {
            // get the burst as a spell burst
            if (burst is PowerBurstCapture<SpellSource> _spellBurst)
            {
                // track power dice so we can change it as we consume it...
                var _dPower = new ValueTarget<decimal>(@"PowerDice", GetPowerDiceEffected());
                _spellBurst.Context.Add(_dPower);
            }
            return;
        }

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
