using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class RemoveCurse : SpellDef, ISpellMode, ISaveCapable, ICounterDispelCapable
    {
        public override string DisplayName => @"Remove Curse";
        public override string Description => @"Remove curses on creature or object.";
        public override MagicStyle MagicStyle => new Abjuration();

        public override IEnumerable<SpellComponent> ArcaneComponents
            => YieldComponents(true, true, false, false, false);

        public override IEnumerable<ISpellMode> SpellModes
            => this.ToEnumerable();

        // ISpellMode
        public bool AllowsSpellResistance => true;
        public bool IsHarmless => true;

        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new TouchAim(@"Creature", @"Creature", Lethality.AlwaysNonLethal,
                ImprovedCriticalTouchFeat.CriticalThreatStart(actor as Creature),
                this, new FixedRange(1), new FixedRange(1), new MeleeRange(), new CreatureTargetType(), new ObjectTargetType());
            yield break;
        }

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverSpell(deliver, 0, deliver.TargetingProcess.Targets[0], 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            var _target = apply.TargetingProcess.Targets.OfType<AttackTarget>().FirstOrDefault();
            if (_target?.Target is Creature _critter)
            {
                foreach (var _curse in _critter.Adjuncts.OfType<DurableMagicEffect>()
                    .Where(_dme => _dme.MagicPowerActionSource.MagicPowerDef.Descriptors.OfType<Curse>().Any())
                    .ToList())
                {
                    // allow the power apply step to be the source of the removal
                    // some curses may resist a simple RemoveCurse, maybe requiring a check, or simply ignoring it
                    var _remove = new RemoveAdjunctData(apply.Actor, _curse);
                    var _interact = new Interaction(apply.Actor, apply, _critter, _remove);
                    _critter.HandleInteraction(_interact);
                }
            }
            else
            {
                // TODO: suppress cursed item
            }
        }

        // ISaveCapable Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource, Interaction workSet, string saveKey)
        => new SaveMode(SaveType.Will, SaveEffect.Negates,
            SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));

        // ICounterDispelCapable
        public IEnumerable<Type> CounterableSpells
            => typeof(BestowCurse).ToEnumerable();

        public IEnumerable<Type> DescriptorTypes
            => Enumerable.Empty<Type>();
    }
}
