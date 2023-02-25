using System;
using System.Collections.Generic;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Actions;
using Uzi.Core;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class PermanentTorch : SpellDef, ISpellMode, IDurableCapable, ICounterDispelCapable
    {
        public PermanentTorch()
            : base()
        {
        }

        public override string DisplayName { get { return @"Permanent Torch"; } }
        public override string Description { get { return @"permanently makes an object glow like a torch without burning"; } }
        public override MagicStyle MagicStyle { get { return new Evocation(); } }

        #region public override IEnumerable<Descriptor> Descriptors { get; }
        public override IEnumerable<Descriptor> Descriptors
        {
            get
            {
                yield return new Uzi.Ikosa.Actions.Light();
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
                yield return new CostlyMaterialComponent(typeof(CostlyComponent<PermanentTorch>), 50m);
                yield break;
            }
        }
        #endregion

        #region public override IEnumerable<ISpellMode> SpellModes
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
            yield return new TouchAim(@"Object", @"Object", Lethality.AlwaysNonLethal,
                20, this, new FixedRange(1), new FixedRange(1), new MeleeRange(), new ObjectTargetType());
            yield break;
        }

        public bool AllowsSpellResistance { get { return false; } }
        public bool IsHarmless { get { return true; } }

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurableToTouch(deliver, @"Object", 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDurableMagicEffects(apply);
        }
        #endregion

        #region IDurableMode Members

        #region public IEnumerable<int> DurableSubModes { get; }
        public IEnumerable<int> DurableSubModes
        {
            get
            {
                yield return 0;
                yield break;
            }
        }
        #endregion

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            var _illumination = new MagicLight(source, 20d, 40d, false);
            target.AddAdjunct(_illumination);
            return _illumination;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            target.RemoveAdjunct((MagicLight)source.ActiveAdjunctObject);
        }

        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode) { return string.Empty; }
        public bool IsDismissable(int subMode) { return false; }
        public DurationRule DurationRule(int subMode) { return new DurationRule(DurationType.Permanent); }
        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact) { yield break; }

        #endregion

        #region ICounterDispelMode Members
        public IEnumerable<Type> CounterableSpells
        {
            get { yield break; }
        }

        public IEnumerable<Type> DescriptorTypes
        {
            get
            {
                yield return typeof(Darkness);
                yield break;
            }
        }
        #endregion
    }
}