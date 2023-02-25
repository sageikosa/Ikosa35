using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class EtherealJaunt : SpellDef, ISpellMode, IDurableCapable
    {
        public override string DisplayName => @"Ethereal Jaunt";
        public override string Description => @"Move to the ethereal plane";
        public override MagicStyle MagicStyle => new Transformation();

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

        public override IEnumerable<ISpellMode> SpellModes => this.ToEnumerable();

        // ISpellMode --------------

        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
            => new PersonalAim(@"Self", actor).ToEnumerable();

        public bool AllowsSpellResistance => false;
        public bool IsHarmless => true;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurable(deliver, deliver.TargetingProcess.Targets[0]);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDurableMagicEffects(apply);
        }

        // IDurableCapable -----------

        public IEnumerable<int> DurableSubModes
            => 0.ToEnumerable();

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            var _ethereal = new EtherealEffect(source, null);
            target.AddAdjunct(_ethereal);
            return _ethereal;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            if (source.ActiveAdjunctObject is EtherealEffect _ethereal)
                target.RemoveAdjunct(_ethereal);
        }

        public bool IsDismissable(int subMode) => true;

        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode)
            => string.Empty;

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Round(), 1));

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
            => Enumerable.Empty<StepPrerequisite>();
    }
}
