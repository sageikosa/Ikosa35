using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Core.Dice;
using Uzi.Ikosa.Time;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class CauseFear : SpellDef, ISpellMode, IDurableCapable, ISaveCapable, ICounterDispelCapable
    {
        public override string DisplayName => @"Cause Fear";
        public override string Description => @"Living creature (5 PD or less) frightened or shaken";
        public override MagicStyle MagicStyle => new Necromancy();

        #region public override IEnumerable<Descriptor> Descriptors { get; }
        public override IEnumerable<Descriptor> Descriptors
        {
            get
            {
                yield return new Fear();
                yield return new MindAffecting();
                yield break;
            }
        }
        #endregion

        public override IEnumerable<SpellComponent> ArcaneComponents
            => YieldComponents(true, true, false, false, false);

        public override IEnumerable<ISpellMode> SpellModes
            => this.ToEnumerable();

        #region ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new AwarenessAim(@"Creature", @"Living Creature", FixedRange.One, FixedRange.One, new NearRange(), new CreatureTargetType());
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => false;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            var _target = deliver.TargetingProcess.GetFirstTarget<AwarenessTarget>(@"Creature");
            if (_target.Target is Creature _critter)
            {
                if (_critter.AdvancementLog.NumberPowerDice <= 5)
                {
                    SpellDef.DeliverDurable(deliver, _target, 0);
                }
                else
                {
                    deliver.Notify(@"Target PD exceeds spell capacity.", @"Failed", false);
                }
            }
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            // Convert Prerequisites to Targets for SpellDef...
            var _savePre = apply.AllPrerequisites<SavePrerequisite>(@"Save.Will").FirstOrDefault();
            var _effect = apply.DurableMagicEffects.FirstOrDefault();
            if ((_savePre != null) && (!_savePre.Success))
            {
                // using frightened, so need the roll prerequisite as duration (already assuming 1 round duration)
                RollPrerequisite _rollPre = apply.AllPrerequisites<RollPrerequisite>(@"Roll.Frightened").FirstOrDefault();
                if (_rollPre != null)
                {
                    _effect.ExpirationTime += (_rollPre.RollValue - 1) * Round.UnitFactor;
                }

                // must setup the flag to use frightened instead of shaken
                _effect.AllTargets.Add(new ValueTarget<bool>(@"Frighten", true));
            }
            else
            {
                // not using frightened
                _effect.AllTargets.Add(new ValueTarget<bool>(@"Frighten", false));
            }

            SpellDef.ApplyDurableMagicEffects(apply);
        }
        #endregion

        #region IDurableMode Members
        public IEnumerable<int> DurableSubModes
            => 0.ToEnumerable();

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            if (source is MagicPowerEffect _spellEffect)
            {
                var _frightened = (from _t in _spellEffect.AllTargets
                                   where typeof(ValueTarget<bool>).IsAssignableFrom(_t.GetType())
                                   && _t.Key.Equals(@"Frighten")
                                   select _t as ValueTarget<bool>).FirstOrDefault().Value;

                Adjunct _fear = null;
                if (_frightened)
                {
                    _fear = new FrightenedEffect(source);
                }
                else
                {
                    _fear = new ShakenEffect(source);
                }
                if (target is Creature _critter)
                {
                    _critter.AddAdjunct(_fear);
                }
                return _fear;
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            if ((source.ActiveAdjunctObject is Adjunct _fear)
                && (target is Creature _critter))
            {
                _critter.RemoveAdjunct(_fear);
            }
        }

        public bool IsDismissable(int subMode) 
            => false;

        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode) 
            => @"Save.Will"; 

        public DurationRule DurationRule(int subMode)
        {
            // NOTE: duration for shaken...
            return new DurationRule(DurationType.Span, new SpanRulePart(1, new Round()));
        }

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
        {
            yield return new RollPrerequisite(interact.Source, interact, interact.Actor,
                @"Roll.Frightened", @"Frightened Rounds", new DieRoller(4), false);
            yield break;
        }
        #endregion

        #region ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource, Interaction workSet, string saveKey)
        {
            return new SaveMode(SaveType.Will, SaveEffect.Partial, SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
        }
        #endregion

        #region ICounterDispelMode Members
        public IEnumerable<Type> CounterableSpells
        {
            get
            {
                yield return typeof(RemoveFear);
                yield break;
            }
        }

        public IEnumerable<Type> DescriptorTypes { get { yield break; } }
        #endregion
    }
}
