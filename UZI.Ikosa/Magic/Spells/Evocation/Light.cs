using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Contracts;
using System.Linq;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class Light : SpellDef, ISpellMode, IDurableCapable, ICounterDispelCapable
    {
        public Light()
            : base()
        {
        }

        public override string DisplayName => @"Light";
        public override string Description => @"Object glows like a torch";
        public override MagicStyle MagicStyle => new Evocation();
        public override bool Equals(object obj) => obj.GetType().Equals(typeof(Light));
        public override int GetHashCode() => GetType().FullName.GetHashCode();

        #region public override IEnumerable<Descriptor> Descriptors { get; }
        public override IEnumerable<Descriptor> Descriptors
        {
            get
            {
                yield return new Actions.Light();
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
            => SpellDef.ApplyDurableMagicEffects(apply);

        // IDurableCapable Members
        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            var _illumination = new MagicLight(source, 20d, 40d, false);
            target.AddAdjunct(_illumination);
            return _illumination;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
            => (source.ActiveAdjunctObject as Adjunct)?.Eject();

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
            => Enumerable.Empty<StepPrerequisite>();

        public IEnumerable<int> DurableSubModes
            => 0.ToEnumerable();

        public bool IsDismissable(int subMode) => true;
        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode) => string.Empty;

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(10, new Minute(), 1));

        // ICounterDispelCapable Members
        public IEnumerable<Type> CounterableSpells
            => Enumerable.Empty<Type>();

        public IEnumerable<Type> DescriptorTypes
            => typeof(Actions.Darkness).ToEnumerable();
    }
}