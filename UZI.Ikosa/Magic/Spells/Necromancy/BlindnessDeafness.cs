using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class BlindnessDeafness : SpellDef, ISpellMode, IDurableCapable, ISaveCapable
    {
        public override string DisplayName => @"Blindness/Deafness";
        public override string Description => @"Make creature blind or deaf";
        public override MagicStyle MagicStyle => new Necromancy();

        public override IEnumerable<SpellComponent> ArcaneComponents
            => YieldComponents(true, false, false, false, false);

        public override IEnumerable<ISpellMode> SpellModes
            => this.ToEnumerable();

        #region ISpellMode Members
        public static IEnumerable<OptionAimOption> EffectOptions
        {
            get
            {
                yield return new OptionAimOption() { Key = @"Blind", Name = @"Blindness", Description = @"Render creature blind" };
                yield return new OptionAimOption() { Key = @"Deaf", Name = @"Deafness", Description = @"Render creature deaf" };
                yield break;
            }
        }

        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new AwarenessAim(@"Creature", @"Creature", FixedRange.One, FixedRange.One, new MediumRange(), new LivingCreatureTargetType());
            yield return new OptionAim(@"Effect", @"Effect", true, FixedRange.One, FixedRange.One, EffectOptions);
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => false;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            if ((deliver.TargetingProcess.Targets[0].Target is Creature _critter)
                && _critter.CreatureType.IsLiving)
            {
                SpellDef.DeliverDurable(deliver, deliver.TargetingProcess.Targets[0], 0);
            }
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.CopyActivityTargetsToSpellEffects(apply);
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
            if ((source is MagicPowerEffect _power)
                && (target is Creature _critter))
            {
                if (_power.AllTargets.OfType<OptionTarget>().FirstOrDefault(_t => _t.Key.Equals(@"Effect"))?
                    .Option.Key.Equals(@"Blind") ?? true)
                {
                    return new BlindEffect(this, double.MaxValue, Day.UnitFactor);
                }
                else
                {
                    return new DeafenedEffect(this, double.MaxValue, Day.UnitFactor);
                }
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            (source?.ActiveAdjunctObject as Adjunct)?.Eject();
        }

        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode) 
            => @"Save.Fortitude";

        public bool IsDismissable(int subMode) => true;
        public DurationRule DurationRule(int subMode) => new DurationRule(DurationType.Permanent);
        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact) { yield break; }

        #endregion

        // ISaveCapable Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource,Interaction workSet, string saveKey)
            => new SaveMode(SaveType.Fortitude, SaveEffect.Negates,
                SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
    }
}
