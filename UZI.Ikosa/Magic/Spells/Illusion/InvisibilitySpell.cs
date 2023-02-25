using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class InvisibilitySpell : SpellDef, ISpellMode, ISaveCapable, IDurableCapable
    {
        public override string DisplayName => @"Invisiblity";
        public override string Description => @"Target vanishes from sight";
        public override MagicStyle MagicStyle => new Illusion(Illusion.SubIllusion.Glamer);

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
                yield return this;
                yield return new InvisibilitySpellPersonalMode();
                yield break;
            }
        }
        #endregion

        // ISpellMode-------------------------------------------------------------

        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new TouchAim(@"Target", @"Target", Lethality.AlwaysNonLethal, 20, this,
                FixedRange.One, FixedRange.One, new MeleeRange(), new CreatureTargetType(), new ObjectTargetType());
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => true;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurableToTouch(deliver, @"Target", 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            // add the fully targetted effect
            SpellDef.ApplyDurableMagicEffects(apply);
        }

        // IDurableMode-------------------------------------------------------------

        public IEnumerable<int> DurableSubModes
        {
            get
            {
                yield return 0;
                yield break;
            }
        }

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            var _invis = new InvisibilitySpellEffect((source as MagicPowerEffect).MagicPowerActionSource);
            target.AddAdjunct(_invis);
            return _invis;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            (source.ActiveAdjunctObject as InvisibilitySpellEffect)?.Eject();
        }

        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode)
            => @"Save.Will";

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Minute(), 1));

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
        {
            // NONE
            yield break;
        }

        public bool IsDismissable(int subMode)
            => true;

        // ISaveMode-------------------------------------------------------------

        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource,Interaction workSet, string saveKey)
            => new SaveMode(SaveType.Will, SaveEffect.Negates,
                SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
    }

    [Serializable]
    public class InvisibilitySpellPersonalMode : ISpellMode, IDurableCapable, ISaveCapable
    {
        public string DisplayName => @"Invisiblity";
        public string Description => @"You vanish from sight";

        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new PersonalAim(@"Self", actor);
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => true;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurable(deliver, deliver.TargetingProcess.Targets[0]);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDurableMagicEffects(apply);
        }

        public virtual IMode GetCapability<IMode>() where IMode : class, ICapability
        {
            return this as IMode;
        }

        // IDurableMode--------------------------------------------------------

        public IEnumerable<int> DurableSubModes
        {
            get
            {
                yield return 0;
                yield break;
            }
        }

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            var _invis = new InvisibilitySpellEffect((source as MagicPowerEffect).MagicPowerActionSource);
            target.AddAdjunct(_invis);
            return _invis;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            (source.ActiveAdjunctObject as InvisibilitySpellEffect)?.Eject();
        }

        public bool IsDismissable(int subMode) => true;

        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode)
            => @"Save.Will";

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Minute(), 1));

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
        {
            // NONE
            yield break;
        }

        // ISaveMode-----------------------------------------------------------

        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource,Interaction workSet, string saveKey)
            => new SaveMode(SaveType.Will, SaveEffect.Negates,
                SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
    }

    /// <summary>
    /// Derived from simple <see cref="Uzi.Ikosa.Adjuncts.Invisibility">Invisibility</see>.
    /// Adds automatic ending on harmful actions.
    /// </summary>
    [Serializable]
    public class InvisibilitySpellEffect : Invisibility, ICanReactBySideEffect
    {
        /// <summary>
        /// Derived from simple <see cref="Uzi.Ikosa.Adjuncts.Invisibility">Invisibility</see>.
        /// Adds automatic ending on harmful actions.
        /// </summary>
        public InvisibilitySpellEffect(object source)
            : base(source)
        {
        }

        public bool IsFunctional
            => IsActive;

        public void ReactToProcessBySideEffect(CoreProcess process)
        {
            var _activity = process as CoreActivity;
            // TODO: filter out "harmful" to an unattended object (as it doesn't count)
            if ((_activity != null) && !((_activity.Action as ActionBase)?.IsHarmless ?? true))
            {
                if (_activity.Actor == Anchor)
                {
                    // activity initiated by the anchor of the adjunct
                    (from _dme in Anchor.Adjuncts.OfType<DurableMagicEffect>()
                     where _dme.Source == Source
                     select _dme).FirstOrDefault()?.Eject();
                }
            }
        }

        public override object Clone()
            => new InvisibilitySpellEffect(Source);
    }
}
