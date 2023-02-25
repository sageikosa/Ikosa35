using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;
using Uzi.Visualize;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class HypnoticPattern : SpellDef, ISpellMode, IDurableCapable, IRegionCapable, IGeometryCapable<SpellSource>,
        IBurstCaptureCapable, ISaveCapable, IDurableAnchorCapable
    {
        public override string DisplayName => @"Hypnotic Pattern";
        public override string Description => @"Visible pattern fascinates creatures";
        public override MagicStyle MagicStyle => new Enchantment(Enchantment.SubEnchantment.Compulsion);

        public override IEnumerable<SpellComponent> ArcaneComponents
            => YieldComponents(false, true, true, false, false);

        public override IEnumerable<SpellComponent> DivineComponents
            => YieldComponents(false, true, false, false, true);

        public override IEnumerable<ISpellMode> SpellModes
            => this.ToEnumerable();

        // ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new LocationAim(@"NearPoint", @"Attraction Point", LocationAimMode.Any,
                FixedRange.One, FixedRange.One, new MediumRange());
            yield return new RollAim(@"Roll.PowerDice", @"Total Power Dice Affected", new DiceRoller(2, 4));
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => false;

        public void ActivateSpell(PowerActivationStep<SpellSource> activation)
        {
            // deliver to pattern location
            var _iTarget = activation.TargetingProcess.Targets.FirstOrDefault(_t => _t.Key.Equals(@"NearPoint")) as LocationTarget;
            var _intersect = new Intersection(_iTarget.Location);

            // sphere will be used in burst, and as geometry for the hypno-zone
            var _sphere = new Geometry(GetDeliveryGeometry(activation), _intersect, true);

            // this makes the geometry available later as a value target
            activation.TargetingProcess.Targets.Add(new ValueTarget<Geometry>(nameof(Geometry), _sphere));

            // everything will be connected by concentration...
            activation.TargetingProcess.Targets.Add(new ValueTarget<ConcentrationMagicControl>
                (nameof(ConcentrationMagicControl), new ConcentrationMagicControl()));

            SpellDef.DeliverBurstToMultipleSteps(activation, _intersect, _sphere, null);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            // bursted, caster and holder are all durable, so they can go through this call
            CopyActivityTargetsToSpellEffects(apply);

            // setting up the concentration/zone additional targets
            var _effect = apply.DurableMagicEffects.FirstOrDefault();
            var _durable = apply.PowerUse.CapabilityRoot.GetCapability<IDurableCapable>();
            if (((_effect?.SubMode ?? 0) == 2)
                || ((_effect?.SubMode ?? 1) == 0))
            {
                // when defining hypno-zone or target, capture duration info
                _effect.AllTargets.Add(new ValueTarget<Duration>(nameof(Duration), _durable.DurationRule(3)
                    .EffectiveSpan(apply.PowerUse.PowerActionSource, apply.DeliveryInteraction.Target)));
            }

            SpellDef.ApplyDurableMagicEffects(apply);
        }

        // ISaveCapable Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource, Interaction workSet, string saveKey)
        {
            if (!string.IsNullOrEmpty(saveKey))
            {
                var _difficulty = SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target);
                return new SaveMode(SaveType.Will, SaveEffect.Negates, _difficulty);
            }
            return null;
        }

        // IRegionCapable Members
        public IEnumerable<double> Dimensions(CoreActor actor, int casterLevel)
            => 10d.ToEnumerable();

        // IGeometryCapable Members
        public IGeometryBuilder GetBuilder(IPowerUse<SpellSource> powerUse, CoreActor actor)
        {
            // get radius
            var _radius = powerUse.CapabilityRoot.GetCapability<IRegionCapable>()
                .Dimensions(actor, powerUse.PowerActionSource.CasterLevel)
                .FirstOrDefault();
            return new SphereBuilder(Convert.ToInt32(_radius / 5));
        }

        // IDurableCapable Members
        public IEnumerable<int> DurableSubModes
            => new[] { 0, 1, 2, 3 };

        // subMode ==0 is for the targets captured by the burst, the holder and caster do not have saves
        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode)
            => subMode == 0 ? @"Save.Will" : string.Empty;

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
            => Enumerable.Empty<StepPrerequisite>();

        // subMode == 0 gets a default span that reflects what happens when concentration ends
        public DurationRule DurationRule(int subMode)
            => subMode switch
            {
                0 => new DurationRule(DurationType.Permanent),      // permanent span       TARGETS
                1 => new DurationRule(DurationType.Concentration),  // permanent/fragile    CASTER
                2 => new DurationRule(DurationType.Concentration),  // permanent/fragile    HOLDER
                _ => new DurationRule(DurationType.Span, new SpanRulePart(2, new Round()))
            };

        public bool IsDismissable(int subMode)
            => false;

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            if (source is MagicPowerEffect _effect)
            {
                if (_effect.SubMode == 0)
                {
                    // enable the concentration link
                    if (source.AnchoredAdjunctObject is ConcentrationPlusSpanMagicTarget _plusSpan)
                    {
                        _plusSpan.IsActive = true;
                    }

                    // fascinated is just the regular active adjunct object
                    var _fascinated = new FascinatedEffect(_effect);
                    target.AddAdjunct(_fascinated);
                    return _fascinated;
                }
                else if (_effect.SubMode == 1)
                {
                    // allow caster to concentrate
                    var _concentration = _effect.GetTargetValue<ConcentrationMagicControl>();
                    var _master = new ConcentrationMagicMaster(_effect, _concentration);
                    target.AddAdjunct(_master);
                    return _master;
                }
                else if (_effect.SubMode == 2)
                {
                    // get additional targets for hypno-zone
                    var _concentration = _effect.GetTargetValue<ConcentrationMagicControl>();
                    var _power = _effect.GetTarget<ValueTarget<decimal>>(PDPool);
                    var _geom = _effect.GetTargetValue<Geometry>(nameof(Geometry));
                    var _duration = _effect.GetTargetValue<Duration>(nameof(Duration));

                    // create ongoing hypno-zone
                    var _hypnoZone = new HypnoticPatternZone(_effect, _concentration, _geom, _duration, _power);
                    target.AddAdjunct(_hypnoZone);
                    return _hypnoZone;
                }
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            // remove the adjunct group and all members from their anchors
            if (source is MagicPowerEffect _spellEffect)
            {
                // eject link between concentration and effect
                (_spellEffect.AnchoredAdjunctObject as ConcentrationPlusSpanMagicTarget)?.Eject();

                // get rid of effect itself
                (source.ActiveAdjunctObject as Adjunct)?.Eject();
            }
        }

        // IDurableAnchorCapable
        public object OnAnchor(IAdjunctTracker source, IAdjunctable target)
        {
            if (source is MagicPowerEffect _spellEffect)
            {
                // all activity targets have been copied to each spell effect during ApplySpell()
                var _concentration = _spellEffect.GetTargetValue<ConcentrationMagicControl>();
                if (_concentration != null)
                {
                    if (_spellEffect.SubMode == 0)
                    {
                        var _duration = _spellEffect.GetTargetValue<Duration>(nameof(Duration));
                        if (_duration != null)
                        {
                            var _plusSpan = new ConcentrationPlusSpanMagicTarget(_spellEffect, _concentration, _duration)
                            { InitialActive = false };
                            target.AddAdjunct(_plusSpan);
                            return _plusSpan;
                        }
                    }
                }
            }
            return null;
        }

        public void OnEndAnchor(IAdjunctTracker source, IAdjunctable target)
        {
            // NOTE: ConcentrationPlusSpanMagicTarget already removed, so NOP
            (target as IntersectionObject)?.UnPath();
            (target as IntersectionObject)?.UnGroup();
        }

        protected const string PDPool = @"Effect.PowerDice";

        // burst and lingering support

        public bool CanEffect(Locator locator, ICore core, decimal available)
            => (core is Creature _creature)
            && _creature.CreatureType.IsLiving
            && _creature.Senses.AllSenses.Any(_s => _s.IsActive && _s.UsesSight)
            // if currently affected, do not allow additional effect
            && !_creature.Adjuncts.OfType<MagicPowerEffect>().Any(_mpe => _mpe.MagicPowerActionSource.MagicPowerDef == this)
            && (_creature.AdvancementLog.PowerDiceCount <= available);

        public bool DoEffect(CoreStep step, ref decimal pdPool)
        {
            if (step is PowerApplyStep<SpellSource> _applyStep)
            {
                if (_applyStep.DeliveryInteraction.Feedback
                    .OfType<PowerActionTransitFeedback<SpellSource>>()
                    .FirstOrDefault() is PowerActionTransitFeedback<SpellSource> _feedback
                    && _feedback.Success)
                {
                    // if the effect was successfully applied, decrease remaining capacity
                    pdPool -= (_applyStep.DeliveryInteraction.Target as Creature)?.AdvancementLog.PowerDiceCount ?? 0m;
                }
            }
            return true;
        }

        // IBurstCaptureCapable Members
        #region public IEnumerable<CoreStep> Capture(BurstCapture burst, Locator locator)
        public IEnumerable<CoreStep> Capture(BurstCapture burst, Locator locator)
        {
            // get the burst as a spell burst
            if (burst is PowerBurstCapture<SpellSource> _spellBurst)
            {
                // get current remaining power dice
                var _power = _spellBurst.Context.FirstOrDefault(_t => _t.Key.Equals(PDPool)) as ValueTarget<decimal>;
                var _remaining = _power.Value;
                if (_remaining > 0)
                {
                    // get captured locators
                    var _actorID = _spellBurst.Activation.Actor?.ID ?? Guid.Empty;
                    foreach (var _step in SpellDef.DeliverDurableDirectFromBurst(_spellBurst, locator,
                        (loc, core) => CanEffect(loc, core, _remaining),
                        (step) => DoEffect(step, ref _remaining), 0))
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

        #region public void PostInitialize(BurstCapture burst)
        public void PostInitialize(BurstCapture burst)
        {
            // get the burst as a spell burst
            if (burst is PowerBurstCapture<SpellSource> _spellBurst)
            {
                // setup power dice so we can change it as we consume it...
                var _casterLevel = Math.Min(_spellBurst.PowerActionSource.CasterLevel, 10);
                var _power = _spellBurst.Activation.TargetingProcess.Targets.FirstOrDefault(_t => _t.Key.Equals(@"Roll.PowerDice")) as ValueTarget<int>
                    ?? new ValueTarget<int>(@"Roll.PowerDice", DiceRoller.RollDice(Guid.Empty, 2, 4, @"Missing Target", @"Total Power Dice Affected"));
                var _dPower = new ValueTarget<decimal>(PDPool, Convert.ToDecimal(_power.Value + _casterLevel));

                // track power dice in spellBurst and activation process
                _spellBurst.Context.Add(_dPower);
                _spellBurst.Activation.TargetingProcess.Targets.Add(_dPower);

                // setup concentration for caster
                if (_spellBurst.Activation.Actor is Creature _critter)
                {
                    SpellDef.DeliverDurable(_spellBurst.Activation, new AimTarget(@"Caster", _critter), 1);
                }

                // also, setup the virtual holder here
                var _iTarget = _spellBurst.Activation.TargetingProcess.GetFirstTarget<LocationTarget>(@"Location");
                var _intObj = new IntersectionObject(DisplayName, new Intersection(_iTarget.Location));
                var _map = _iTarget.MapContext.Map;
                var _loc = new Locator(_intObj, _iTarget.MapContext, _intObj.GeometricSize,
                    new Cubic(_iTarget.Location, _intObj.GeometricSize));
                SpellDef.DeliverDurable(_spellBurst.Activation, new AimTarget(@"Holder", _intObj), 2);
            }
        }
        #endregion
    }

    [Serializable]
    public class HypnoticPatternBard : HypnoticPattern
    {
        public override IEnumerable<SpellComponent> ArcaneComponents
            => YieldComponents(true, true, true, false, false);
    }

    /// <summary>
    /// This adjunct maintains the zone for residual fascination capacity, and refreshes fascination on affected creatures
    /// as long as concentration is maintained plus whatever the span is...
    /// </summary>
    [Serializable]
    public class HypnoticPatternZone : ConcentrationMagicEffect, ILocatorZone, IPowerUse<SpellSource>
    {
        public HypnoticPatternZone(MagicPowerEffect magicPowerEffect, ConcentrationMagicControl control,
            Geometry geometry, Duration duration, ValueTarget<decimal> pdAffected)
            : base(magicPowerEffect, control)
        {
            _Geometry = geometry;
            _Duration = duration;
            _PDPool = pdAffected;
        }

        #region state
        protected LocatorCapture _Capture;
        private Geometry _Geometry;
        private ValueTarget<decimal> _PDPool;
        private Duration _Duration;
        #endregion

        public Geometry Geometry => _Geometry;
        public ValueTarget<decimal> PDPool => _PDPool;
        public Duration Duration => _Duration;

        public override object Clone()
            => new HypnoticPatternZone(MagicPowerEffect, Control, Geometry, Duration, PDPool);

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            var _loc = Anchor.GetLocated()?.Locator;
            if (_loc != null)
            {
                _Capture = new LocatorCapture(_loc.MapContext, this, Geometry, this, true, _loc.PlanarPresence);
            }
        }

        protected override void OnDeactivate(object source)
        {
            _Capture.MapContext.LocatorZones.Remove(_Capture);
            base.OnDeactivate(source);
        }

        private void HypnoticCapture(Locator locator)
        {
            var _myLocator = Anchor?.GetLocated().Locator;
            if ((_myLocator != null) && (PDPool.Value > 0))
            {
                // get the magic powerDef so we can hook into canEffect and DoEffect...
                var _hypno = MagicPowerEffect.MagicPowerActionSource.MagicPowerActionDef as HypnoticPattern;
                var _remaining = PDPool.Value;
                foreach (var _core in from _c in locator.GetCapturable<ICore>()
                                      where _hypno.CanEffect(locator, _c, _remaining)
                                      select _c)
                {
                    // count down remaining
                    _remaining -= (_core as Creature)?.AdvancementLog.PowerDiceCount ?? 0;

                    // setup target
                    var _interactCore = _core as IInteract;
                    var _target = new AimTarget(@"Target", _interactCore);

                    // setup targeting process targets
                    var _targets = new List<AimTarget>();
                    _targets.Add(new ValueTarget<ConcentrationMagicControl>(nameof(ConcentrationMagicControl), Control));
                    _targets.Add(new ValueTarget<Duration>(nameof(Duration), Duration));

                    // deliver from myLocator to target, durable mode 0 via this as the power use
                    var _process = new CoreTargetingProcess(
                        SpellDef.DeliverDurableNextStep(
                            null, Control.Master.Anchor as Creature, _myLocator, _myLocator.PlanarPresence, this, _targets, _target, true, 0),
                        Anchor as CoreObject, @"Hypnotic Pattern Catpure", _targets);
                    _myLocator.IkosaProcessManager.StartProcess(_process);
                }
                PDPool.Value = _remaining;
            }
        }

        public void Capture(Locator locator)
        {
            HypnoticCapture(locator);
        }

        public void Enter(Locator locator)
        {
            HypnoticCapture(locator);
        }

        public void Start(Locator locator) { }
        public void End(Locator locator) { }

        private void SeverLinks(Locator locator)
        {
            foreach (var _link in (from _conn in locator.AllConnected().OfType<IAdjunctSet>()
                                   from _l in _conn.Adjuncts.OfType<ConcentrationPlusSpanMagicTarget>()
                                   where _l.Control == Control
                                   select _l).ToList())
            {
                _link.Eject();
            }
        }

        public void Exit(Locator locator)
        {
            SeverLinks(locator);
        }

        public void Release(Locator locator)
        {
            SeverLinks(locator);
        }

        public void MoveInArea(Locator locator, bool followOn)
        {
            // NOTE: nothing?
        }

        // power use
        public ICapabilityRoot CapabilityRoot => MagicPowerEffect.CapabilityRoot;
        public SpellSource PowerActionSource => MagicPowerEffect.MagicPowerActionSource as SpellSource;

        public void ApplyPower(PowerApplyStep<SpellSource> step)
        {
            // bursted, caster and holder are all durable, so they can go through this call
            SpellDef.CopyActivityTargetsToSpellEffects(step);

            // setting up the concentration/zone additional targets
            var _effect = step.DurableMagicEffects.FirstOrDefault();
            var _durable = step.PowerUse.CapabilityRoot.GetCapability<IDurableCapable>();
            if (((_effect?.SubMode ?? 0) == 2)
                || ((_effect?.SubMode ?? 1) == 0))
            {
                // when defining hypno-zone or target, capture duration info
                _effect.AllTargets.Add(new ValueTarget<Duration>(nameof(Duration), _durable.DurationRule(3)
                    .EffectiveSpan(step.PowerUse.PowerActionSource, step.DeliveryInteraction.Target)));
            }

            SpellDef.ApplyDurableMagicEffects(step);
        }

        public void ActivatePower(PowerActivationStep<SpellSource> step)
        {
        }

        public PowerAffectTracker PowerTracker => MagicPowerEffect.PowerTracker;
    }
}
