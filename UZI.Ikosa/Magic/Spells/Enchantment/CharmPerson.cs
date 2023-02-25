using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class CharmPerson : SpellDef, ISpellMode, IDurableCapable, ISaveCapable
    {
        public override string DisplayName => @"Charm Person";
        public override string Description => @"Adjust a person's attitude";
        public override MagicStyle MagicStyle => new Enchantment(Enchantment.SubEnchantment.Charm);
        public override IEnumerable<Descriptor> Descriptors => new MindAffecting().ToEnumerable();
        public override IEnumerable<ISpellMode> SpellModes => this.ToEnumerable();

        public override IEnumerable<SpellComponent> ArcaneComponents
            => YieldComponents(true, true, false, false, false);

        // ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new AwarenessAim(@"Humanoid", @"Humanoid Creature", FixedRange.One, FixedRange.One, new NearRange(), new CreatureTypeTargetType<HumanoidType>());
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => false;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurable(deliver, deliver.TargetingProcess.GetFirstTarget<AwarenessTarget>(@"Humanoid"), 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            // make sure the SpellEffect has the target name...
            var _feedback = apply.DeliveryInteraction.Feedback.OfType<PowerActionTransitFeedback<SpellSource>>().FirstOrDefault();
            if (_feedback.PowerTransit is MagicPowerEffectTransit<SpellSource> _transit
                && (apply.Actor != null))
            {
                _transit.MagicPowerEffects.First().AllTargets.Add(new CharacterStringTarget(@"Name", apply.Actor.Name));
                _transit.MagicPowerEffects.First().AllTargets.Add(new ValueTarget<Guid>(@"AttitudeID", apply.Actor.ID));
            }
            SpellDef.ApplyDurableMagicEffects(apply);
        }

        // IDurableCapable Members
        public IEnumerable<int> DurableSubModes => 0.ToEnumerable();

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            if (source is MagicPowerEffect _spellEffect)
            {
                if (_spellEffect.AllTargets.Where(_t => _t.Key.Equals(@"Name")).FirstOrDefault() is CharacterStringTarget _charStr)
                {
                    var _attitudeID = _spellEffect.GetTargetValue<Guid>(@"AttitudeID", Guid.Empty);
                    if (_attitudeID != Guid.Empty)
                    {
                        var _attitude = new AttitudeAdjunct(_spellEffect, _charStr.CharacterString, _attitudeID, Attitude.Friendly);
                        target.AddAdjunct(_attitude);
                        return _attitude;
                    }
                }
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            if (source.ActiveAdjunctObject is AttitudeAdjunct _attitude)
            {
                target.RemoveAdjunct(_attitude);
            }
        }

        public bool IsDismissable(int subMode) => false;

        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode)
            => @"Save.Will";

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Hour(), 1));

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
            => Enumerable.Empty<StepPrerequisite>();

        // ISaveCapable Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource, Interaction workSet, string saveKey)
        {
            var _mode = new SaveMode(SaveType.Will, SaveEffect.Negates,
                SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
            // TODO: +5 bonus for being threatened or attacked by caster or caster's allies...
            return _mode;
        }
    }
}
