using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class AcidArrow : SpellDef, ISpellMode, IDamageCapable, IDurableCapable, IPowerDeliverVisualize
    {
        public override string DisplayName => @"Acid Arrow";
        public override string Description => @"Ranged touch arrow deals 2d4 acid damage on contact and additional rounds.";
        public override MagicStyle MagicStyle => new Conjuration(Conjuration.SubConjure.Creation);

        #region public override IEnumerable<Descriptor> Descriptors { get; }
        public override IEnumerable<Descriptor> Descriptors
        {
            get
            {
                yield return new Acid();
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
                yield return new MaterialComponent();
                yield return new FocusComponent();
                yield break;
            }
        }
        #endregion

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

        #region public override IEnumerable<ISpellMode> SpellModes { get; }
        public override IEnumerable<ISpellMode> SpellModes
        {
            get
            {
                yield return (ISpellMode)this;
                yield break;
            }
        }
        #endregion

        #region ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new TouchAim(@"Target", @"Target", Lethality.AlwaysLethal,
                ImprovedCriticalRangedTouchFeat.CriticalThreatStart(actor),
                this, new FixedRange(1), new FixedRange(1), new FarRange(),
                new TargetType[] { new ObjectTargetType(), new CreatureTargetType() });
            yield break;
        }

        public bool AllowsSpellResistance => false;
        public bool IsHarmless => false;
        #endregion

        #region public void Deliver(PowerDeliveryStep<SpellSource> deliver)
        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDamageToTouch(deliver, 0);
        }
        #endregion

        #region public void ApplySpell(PowerApplyStep<SpellSource> apply)
        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            var _target = apply.TargetingProcess.Targets.OfType<AttackTarget>().FirstOrDefault();
            SpellDef.ApplyDamage(apply, apply, 0);

            // get damage mode
            var _powerUse = apply.PowerUse;
            var _mode = _powerUse.CapabilityRoot;
            var _damage = _mode.GetCapability<IDamageCapable>();

            // calculate times using duration mode and current time
            var _nextTime = (apply.Actor?.GetLocated()?.Locator.Map?.CurrentTime ?? 0d) + Round.UnitFactor;
            var _powerSource = _powerUse.PowerActionSource;
            var _endTime = _mode.GetCapability<IDurableCapable>()?
                .DurationRule(0)?.EndTime(apply.Actor, _powerSource, _target.Target)
                ?? _nextTime;

            // create continuous damage
            (_target.Target as IAnchorage)?.AddAdjunct(
                new ContinuousDamageSource(_powerSource, _damage.GetDamageRules(0, false).ToList(),
                _nextTime, _endTime, Round.UnitFactor, _powerSource.CasterLevel));
        }
        #endregion

        #region IDamageSpellMode Members
        public IEnumerable<DamageRule> GetDamageRules(int subMode, bool isCriticalHit)
        {
            yield return new EnergyDamageRule(@"Acid.Damage", new DiceRange(@"Acid", DisplayName, new DiceRoller(2, 4)), @"Acid Damage", EnergyType.Acid);
            if (isCriticalHit)
            {
                yield return new EnergyDamageRule(@"Acid.Damage.Critical", new DiceRange(@"Acid (Critical)", DisplayName, new DiceRoller(2, 4)), @"Acid Damage (Critical)", EnergyType.Acid);
            }

            yield break;
        }

        public IEnumerable<int> DamageSubModes
        {
            get
            {
                yield return 0;
                yield break;
            }
        }

        public string DamageSaveKey(Interaction workSet, int subMode)
        {
            return string.Empty;
        }

        public bool CriticalFailDamagesItems(int subMode) => false;
        #endregion

        #region IDurableMode Members

        public IEnumerable<int> DurableSubModes => throw new NotImplementedException();

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            // never calling this, just have the interface so it can be extended
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
        }

        public bool IsDismissable(int subMode)
            => false;

        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode)
            => string.Empty;

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Round(), 3))
            {
                MaxLevel = 18
            };

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
        {
            yield break;
        }

        #endregion

        #region IPowerDeliverVisualize Members

        public VisualizeTransferType GetTransferType() { return VisualizeTransferType.CylinderBolt; }
        public VisualizeTransferSize GetTransferSize() { return VisualizeTransferSize.Medium; }
        public string GetTransferMaterialKey() { return @"#E030FF20"; }
        public VisualizeSplashType GetSplashType() { return VisualizeSplashType.Pop; }
        public string GetSplashMaterialKey() { return @"#C030FF20|#FF70FF40|#C030FF20"; }

        #endregion
    }
}
