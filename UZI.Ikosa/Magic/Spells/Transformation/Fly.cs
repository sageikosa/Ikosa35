using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class Fly : SpellDef, ISpellMode, IDurableCapable, ISaveCapable
    {
        public override string DisplayName => @"Fly";
        public override string Description => @"Fly at 60 feet (or 40 feet if encumbered) with good maneuverability.";
        public override MagicStyle MagicStyle => new Transformation();

        #region public override IEnumerable<SpellComponent> DivineComponents { get; }
        public override IEnumerable<SpellComponent> DivineComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield return new DivineFocusComponent();
                yield break;
            }
        }
        #endregion

        #region public override IEnumerable<SpellComponent> ArcaneComponents { get; }
        public override IEnumerable<SpellComponent> ArcaneComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield return new FocusComponent();
                yield break;
            }
        }
        #endregion

        #region public override IEnumerable<ISpellMode> SpellModes { get; }
        public override IEnumerable<ISpellMode> SpellModes
        {
            get
            {
                yield return this;
                yield break;
            }
        }
        #endregion

        #region ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new TouchAim(@"Creature", @"Creature", Lethality.AlwaysNonLethal,
                20, this, FixedRange.One, FixedRange.One, new MeleeRange(), new CreatureTargetType());
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => true;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurableToTouch(deliver, @"Creature", 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            if (apply.DeliveryInteraction.Target is Creature)
            {
                SpellDef.ApplyDurableMagicEffects(apply);
            }
        }
        #endregion

        #region IDurableMode Members
        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            if ((target is Creature _critter)
                && (source is MagicPowerEffect _magic))
            {
                // define a fly effect, add to creature
                var _fly = new FlyEffect(_magic);
                _critter.AddAdjunct(_fly);
                return _fly;
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            // terminate fly effect
            (source.ActiveAdjunctObject as FlyEffect)?.Eject();
        }

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
        {
            yield break;
        }

        public IEnumerable<int> DurableSubModes
            => 0.ToEnumerable();

        public bool IsDismissable(int subMode)
            => false;

        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode)
            => @"Save.Will";

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Minute(), 1));
        #endregion

        #region ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource, Interaction workSet, string saveKey)
        {
            return new SaveMode(SaveType.Will, SaveEffect.Negates, SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
        }
        #endregion
    }

    [Serializable]
    public class FlyEffect : Adjunct
    {
        public FlyEffect(MagicPowerEffect magic)
            : base(magic)
        {
        }

        #region state
        private FlightSuMovement _Flight;
        #endregion

        public MagicPowerEffect MagicPowerEffect => Source as MagicPowerEffect;
        public Creature Creature => Anchor as Creature;

        public override object Clone()
            => new FlyEffect(MagicPowerEffect);

        protected override void OnActivate(object source)
        {
            var _critter = Creature;
            if (_critter != null)
            {
                _Flight = new FlightSuMovement(60, _critter, Source, FlightManeuverability.Good, true, true);
                _critter.Movements.Add(_Flight);
            }
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            var _inFlight = Creature?.HasActiveAdjunct<InFlight>() ?? false;
            Creature?.Movements.Remove(_Flight);

            // was inflight, now cannot fly
            if (_inFlight
                && !(Creature?.Movements.AllMovements.OfType<FlightSuMovement>().Any(_m => _m.IsUsable) ?? false))
            {
                // get ersatz spell parameters
                var _magicSource = MagicPowerEffect.MagicPowerActionSource;
                var _expire = ((Creature.Setting as LocalMap)?.CurrentTime ?? 0d) + 1d;
                var _effect = new DurableMagicEffect(_magicSource, null, MagicPowerEffect.PowerTracker,
                    _expire, TimeValTransition.Leaving, 0);

                // try to transit it
                var _aLoc = Creature.GetLocated()?.Locator;
                var _transit = new MagicPowerEffectTransit<MagicPowerActionSource>(_magicSource,
                    null, MagicPowerEffect.PowerTracker, _effect.ToEnumerable().ToList(), Creature, _aLoc, _aLoc.PlanarPresence,
                    ((AimTarget)null).ToEnumerable().ToList());
                var _interact = new StepInteraction(null, null, _magicSource, Creature, _transit);
                Creature.HandleInteraction(_interact);

                // success or spell resistance
                if (_interact.Feedback.OfType<PowerActionTransitFeedback<MagicPowerActionSource>>().Any(_paf => _paf.Success)
                    || _interact.Feedback.OfType<SpellResistanceFeedback>().Any())
                {
                    Creature?.StartNewProcess(new FlyEndStep(_effect, Creature), @"Fly ended");
                }
                else
                {
                    if ((Creature?.GetLocated()?.Locator is Locator _loc) && _loc.IsGravityEffective)
                    {
                        FallingStartStep.StartFall(_loc, 500, 500, @"Fly ended", 1);
                    }
                }
            }
            base.OnDeactivate(source);
        }
    }

    [Serializable]
    public class FlyEndStep : PreReqListStepBase, ICapabilityRoot, IDurableCapable
    {
        public FlyEndStep(DurableMagicEffect magicEffect, Creature creature)
            : base((CoreProcess)null)
        {
            _MagicEffect = magicEffect;
            _Critter = creature;
            _PendingPreRequisites.Enqueue(new RollPrerequisite(this, @"Rounds", @"Number of Rounds to Slow Fall",
                new DieRoller(6), true));
        }

        #region state
        protected DurableMagicEffect _MagicEffect;
        protected Creature _Critter;
        #endregion

        protected override bool OnDoStep()
        {
            if (CanDoStep && !IsComplete)
            {
                // fixup effect
                _MagicEffect.CapabilityRoot = this;
                _MagicEffect.ExpirationTime = ((_Critter.Setting as LocalMap)?.CurrentTime ?? 0d)
                    + GetPrerequisite<RollPrerequisite>().RollValue;

                // add the slow fall magic effect
                _Critter.AddAdjunct(_MagicEffect);

                // and start slow falling
                if (_Critter?.GetLocated()?.Locator is Locator _loc)
                {
                    SlowFallingStartStep.StartSlowFall(_loc, 60, @"Fly ended");
                }
            }
            return true;
        }

        public virtual IMode GetCapability<IMode>()
            where IMode : class, ICapability
            => this as IMode;

        public IEnumerable<int> DurableSubModes => 0.ToEnumerable();

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            if (source is MagicPowerEffect _spellEffect)
            {
                var _slowFall = new SlowFallEffect(_spellEffect);
                target.AddAdjunct(_slowFall);
                return _slowFall;
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            (source.ActiveAdjunctObject as SlowFallEffect)?.Eject();
        }

        public bool IsDismissable(int subMode)
            => false;

        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode)
            => string.Empty;

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
        {
            yield break;
        }

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span,
                new SpanRulePart(GetPrerequisite<RollPrerequisite>().RollValue, new Round()));
    }
}
