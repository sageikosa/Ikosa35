using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class SeeInvisibility : SpellDef, ISpellMode, IDurableCapable
    {
        public override MagicStyle MagicStyle => new Divination(Divination.SubDivination.Detection);

        public override IEnumerable<SpellComponent> ArcaneComponents
            => YieldComponents(true, true, true, false, false);

        public override IEnumerable<ISpellMode> SpellModes => this.ToEnumerable();

        public override string DisplayName => @"See Invisibility";
        public override string Description => @"See invisibile and ethereal within range of normal sight";

        // ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
            => new PersonalAim(@"Self", actor).ToEnumerable();

        public bool AllowsSpellResistance => false;
        public bool IsHarmless => true;

        public void ActivateSpell(PowerActivationStep<SpellSource> delivery)
        {
            SpellDef.DeliverDurable(delivery, delivery.TargetingProcess.Targets[0], 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDurableMagicEffects(apply);
        }


        // IDurableCapable Members
        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            var _source = new SeeInvisibleSource();
            if (target is Creature _critter)
            {
                foreach (var _sense in _critter.Senses.AllSenses.Where(_s => _s.UsesSight).ToList())
                {
                    _critter.Senses.Add(new SeeInvisible(_source, _sense));
                }
            }
            return _source;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            if (target is Creature _critter)
            {
                foreach (var _sense in _critter.Senses.AllSenses.Where(_s => _s.Source == deactivateSource).ToList())
                {
                    _critter.Senses.Remove(_sense);
                }
            }
        }

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
            => Enumerable.Empty<StepPrerequisite>();

        public IEnumerable<int> DurableSubModes
            => 0.ToEnumerable();

        public virtual bool IsDismissable(int subMode)
            => true;

        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode)
            => string.Empty;

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(10, new Minute(), 1));
    }

    [Serializable]
    public class SeeInvisibleSource
    {
    }
}
