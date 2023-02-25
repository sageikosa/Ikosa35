using System;
using System.Collections.Generic;
using Uzi.Ikosa.Adjuncts;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Contracts;
using System.Linq;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class TouchOfFatigue : SpellDef, ISpellMode, IDurableCapable, ISaveCapable, IPowerDeliverVisualize
    {
        public override string DisplayName => @"Touch of Fatigue";
        public override string Description => @"Touch to fatigue target";
        public override MagicStyle MagicStyle => new Necromancy();

        public override IEnumerable<SpellComponent> ArcaneComponents
            => YieldComponents(true, true, true, false, false);

        public override IEnumerable<ISpellMode> SpellModes
            => this.ToEnumerable();

        // ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new TouchAim(@"Creature", @"Creature", Lethality.AlwaysLethal,
                ImprovedCriticalTouchFeat.CriticalThreatStart(actor as Creature),
                this, new FixedRange(1), new FixedRange(1), new MeleeRange(), new CreatureTargetType());
            yield break;
        }
        public bool AllowsSpellResistance => true;
        public bool IsHarmless => false;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurableToTouch(deliver, @"Creature", 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDurableMagicEffects(apply);
        }

        // IDurableCapable Members
        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            // can neither run nor charge and takes a -2 penalty to Strength and Dexterity
            var _effect = new Fatigued(source);
            target.AddAdjunct(_effect);
            return _effect;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            target.RemoveAdjunct((Fatigued)source.ActiveAdjunctObject);
        }

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
            => Enumerable.Empty<StepPrerequisite>();

        public IEnumerable<int> DurableSubModes
            => 0.ToEnumerable();

        public bool IsDismissable(int subMode)
            => false;

        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode)
            => @"Save.Forititude";

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Round(), 1));

        #region ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource, Interaction workSet, string saveKey)
        {
            return new SaveMode(SaveType.Fortitude, SaveEffect.Negates, SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
        }
        #endregion

        // IPowerDeliverVisualize Members
        public VisualizeTransferType GetTransferType() => VisualizeTransferType.None;
        public VisualizeTransferSize GetTransferSize() => VisualizeTransferSize.Medium;
        public string GetTransferMaterialKey() => @"#E0FFFFFF";
        public VisualizeSplashType GetSplashType() => VisualizeSplashType.Drain;
        public string GetSplashMaterialKey() => @"#C0FFFFFF|#80FFFFFF|#C0FFFFFF";
    }
}
