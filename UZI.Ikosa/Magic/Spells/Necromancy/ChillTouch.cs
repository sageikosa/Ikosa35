using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Core.Dice;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Feats;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class ChillTouch : SpellDef, ISpellMode, IDamageCapable, IDurableCapable, ISaveCapable, IPowerDeliverVisualize
    {
        public override string DisplayName => @"Chill Touch";
        public override string Description => @"Deal 1d6 damage and 1 Strength Damage to living creatures, or panics undead.";
        public override MagicStyle MagicStyle => new Necromancy();

        #region public override IEnumerable<SpellComponent> ArcaneComponents { get; }
        public override IEnumerable<SpellComponent> ArcaneComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
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
            yield return new TouchAim(@"Creature", @"Creature", Lethality.AlwaysLethal,
                ImprovedCriticalTouchFeat.CriticalThreatStart(actor as Creature),
                this, FixedRange.One, FixedRange.One, new MeleeRange(), new CreatureTargetType());
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => false;

        #region public void Deliver(PowerDeliveryStep<SpellSource> deliver)
        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            // see if multiple attacks have been set
            if (deliver.TargetingProcess.Targets.Count == 1)
            {
                // append the touch countdown (1 per caster level)
                var _atkCount = new ValueTarget<int>(@"Touch.Countdown", deliver.PowerUse.PowerActionSource.CasterLevel);
                deliver.TargetingProcess.Targets.Add(_atkCount);
            }

            var _critter = deliver.TargetingProcess.Targets[0].Target as Creature;

            // undead get panicked (possibly)
            if (_critter.CreatureType is UndeadType)
                SpellDef.DeliverDurableToTouch(deliver, @"Creature", 0);

            // living get damaged if hit...
            if (_critter.CreatureType.IsLiving)
            {
                // normal operations
                SpellDef.DeliverDamageToTouch(deliver, 0, 1);
            }
            else
            {
                // Try to attack anyway (not undead and non-living)
                var _result = SpellDef.InteractSingleTargetAttack(deliver, 0, deliver.TargetingProcess.Targets[0]);
                var _atkFB = _result.Feedback.OfType<ISuccessIndicatorFeedback>().FirstOrDefault();
                if ((_atkFB != null) && !_atkFB.Success)
                {
                    if (_atkFB is AttackFeedback)
                    {
                        // since the attack missed, the charge is still held
                        deliver.AppendFollowing(new HoldChargeStep(deliver));
                    }
                }
                else
                {
                    deliver.Notify(@"Invalid target ended spell", @"Failed", false);
                }
            }
        }
        #endregion

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            // get countdown information
            var _atkCount = apply.TargetingProcess.Targets[1] as ValueTarget<int>;
            var _atkLeft = _atkCount.Value - 1;

            var _critter = apply.DeliveryInteraction.Target as Creature;

            // undead get panicked
            if (_critter.CreatureType is UndeadType)
            {
                // add extra panic time to chill touch durable effect
                var _rollPre = apply.AllPrerequisites<RollPrerequisite>(@"Roll.Panic").FirstOrDefault();
                var _effect = apply.DurableMagicEffects.FirstOrDefault();
                if (_effect != null)
                {
                    _effect.ExpirationTime += Round.UnitFactor * _rollPre.RollValue;
                }

                // apply the spell
                SpellDef.ApplyDurableMagicEffects(apply);
            }

            // living get damaged if hit
            if (_critter.CreatureType.IsLiving)
            {
                SpellDef.ApplyDamage(apply, apply, 0, 1);
            }

            // countdown chill touch
            if (_atkLeft > 0)
            {
                // set new countdown value
                apply.TargetingProcess.Targets.Remove(_atkCount);
                _atkCount = new ValueTarget<int>(@"Touch.Countdown", _atkLeft);
                apply.TargetingProcess.Targets.Add(_atkCount);
                apply.AppendFollowing(new HoldChargeStep(apply));
            }
        }
        #endregion

        #region IDamageMode Members
        public IEnumerable<int> DamageSubModes
        {
            get
            {
                yield return 0;
                yield return 1;
                yield break;
            }
        }

        public IEnumerable<DamageRule> GetDamageRules(int subMode, bool isCriticalHit)
        {
            switch (subMode)
            {
                case 0:
                    yield return new EnergyDamageRule(@"Negative.Damage", new DiceRange(@"Negative", DisplayName, new DieRoller(6)), @"Chill Touch", EnergyType.Negative);
                    if (isCriticalHit)
                        yield return new EnergyDamageRule(@"Negative.Damage.Critical", new DiceRange(@"Negative (Critical)", DisplayName, new DieRoller(6)), @"Chill Touch (Critical)", EnergyType.Negative);
                    break;

                case 1:
                    yield return new AbilityDamageRule(@"Ability.Damage", FixedRange.One, MnemonicCode.Str, @"Strength Damage");
                    if (isCriticalHit)
                        yield return new AbilityDamageRule(@"Ability.Damage.Critical", FixedRange.One, MnemonicCode.Str, @"Strength Damage (Critical)");
                    break;
            }
            yield break;
        }

        public string DamageSaveKey(Interaction workSet, int subMode)
        {
            switch (subMode)
            {
                case 1:
                    return @"Save.Fortitude";
            }
            return string.Empty;
        }

        public bool CriticalFailDamagesItems(int subMode) => false;
        #endregion

        #region IDurableMode Members
        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            if (source is MagicPowerEffect)
            {
                var _panic = new PanickedEffect((source as MagicPowerEffect).MagicPowerActionSource);
                target.AddAdjunct(_panic);
                return _panic;
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            target.RemoveAdjunct(source.ActiveAdjunctObject as PanickedEffect);
        }

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
        {
            yield return new RollPrerequisite(interact.Source, interact, interact.Actor, @"Roll.Panic", @"Extra Undead Panic Rounds", new DieRoller(4), false);
            yield break;
        }

        public IEnumerable<int> DurableSubModes
        {
            get
            {
                yield return 0;
                yield break;
            }
        }

        public bool IsDismissable(int subMode)
        {
            return false;
        }

        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode)
        {
            return @"Save.Will";
        }

        public DurationRule DurationRule(int subMode)
        {
            return new DurationRule(DurationType.Span, new SpanRulePart(1, new Round(), 1));
        }
        #endregion

        #region ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource,Interaction workSet, string saveKey)
        {
            switch (saveKey)
            {
                case @"Save.Fortitude":
                    return new SaveMode(SaveType.Fortitude, SaveEffect.Negates, SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));

                default:
                    return new SaveMode(SaveType.Will, SaveEffect.Negates, SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
            }
        }
        #endregion

        #region IPowerDeliverVisualize Members

        public VisualizeTransferType GetTransferType() { return VisualizeTransferType.None; }
        public VisualizeTransferSize GetTransferSize() { return VisualizeTransferSize.Medium; }
        public string GetTransferMaterialKey() { return string.Empty; }
        public VisualizeSplashType GetSplashType() { return VisualizeSplashType.Drain; }
        public string GetSplashMaterialKey() { return @"#C0808080|#80808080|#C0808080"; }

        #endregion
    }
}
