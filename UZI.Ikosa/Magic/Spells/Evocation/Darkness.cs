using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class Darkness : SpellDef, ISpellMode, IDurableCapable, ICounterDispelCapable, IRegionCapable
    {
        public Darkness()
            : base()
        {
        }

        public override string DisplayName => @"Darkness";
        public override string Description => @"Object illuminates a 20 foot radius sphere with shadowy illumination";
        public override MagicStyle MagicStyle => new Evocation();
        public override bool Equals(object obj) => obj.GetType().Equals(typeof(Darkness));
        public override int GetHashCode() => GetType().FullName.GetHashCode();

        #region public override IEnumerable<Descriptor> Descriptors { get; }
        public override IEnumerable<Descriptor> Descriptors
        {
            get
            {
                yield return new Actions.Darkness();
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
                yield return new MaterialComponent();
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

        // ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new TouchAim(@"Object", @"Object", Lethality.AlwaysNonLethal,
                20, this, new FixedRange(1), new FixedRange(1), new MeleeRange(), new ObjectTargetType());
            yield break;
        }
        public bool AllowsSpellResistance => false;
        public bool IsHarmless => true;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
            => SpellDef.DeliverDurableToTouch(deliver, @"Object", 0);

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            var _rMode = apply.PowerUse.CapabilityRoot.GetCapability<IRegionCapable>();
            var _maxRadius = _rMode.Dimensions(apply.Actor, apply.PowerUse.PowerActionSource.CasterLevel).FirstOrDefault();

            apply.DurableMagicEffects.FirstOrDefault()?.AllTargets
                .Add(new ValueTarget<double>(@"Radius", Math.Max(20d, _maxRadius)));

            SpellDef.ApplyDurableMagicEffects(apply);
        }

        // IDurableCapable Members
        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            if (source is MagicPowerEffect _mpe)
            {
                var _range = _mpe.AllTargets.OfType<ValueTarget<double>>()
                    .Where(_t => _t.Key.Equals(@"Radius")).FirstOrDefault()?.Value ?? 20d;
                var _illumination = new MagicDark(source, _range);
                target.AddAdjunct(_illumination);
                return _illumination;
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            (source.ActiveAdjunctObject as Adjunct)?.Eject();
        }

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
            => Enumerable.Empty<StepPrerequisite>();

        public IEnumerable<int> DurableSubModes
            => 0.ToEnumerable();

        public bool IsDismissable(int subMode) => true;
        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode) => string.Empty;

        public virtual DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(10, new Minute(), 1));

        // IRegionCapable members
        public virtual IEnumerable<double> Dimensions(CoreActor actor, int powerLevel)
            => 20d.ToEnumerable();

        // ICounterDispelCapable Members
        public IEnumerable<Type> CounterableSpells
            => Enumerable.Empty<Type>();

        public IEnumerable<Type> DescriptorTypes
            => typeof(Actions.Light).ToEnumerable();
    }
}
