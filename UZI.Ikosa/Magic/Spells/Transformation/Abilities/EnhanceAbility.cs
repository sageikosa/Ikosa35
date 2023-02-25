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
    public abstract class EnhanceAbility : SpellDef, ISpellMode, IDurableCapable, ISaveCapable
    {
        public override string Description => $"+4 Enhancement to {AbilityMnemonic}";
        public override MagicStyle MagicStyle => new Transformation();

        protected abstract string AbilityMnemonic { get; }

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
            yield return new TouchAim(@"Creature", @"Creature", Lethality.AlwaysNonLethal,
                20, this, FixedRange.One, FixedRange.One,
                new MeleeRange(), new CreatureTargetType());
            yield break;
        }

        public bool AllowsSpellResistance { get { return true; } }
        public bool IsHarmless { get { return true; } }

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurableToTouch(deliver, @"Creature", 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDurableMagicEffects(apply);
        }
        #endregion

        #region IDurableMode Members
        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            var _spellEffect = source as MagicPowerEffect;
            if (_spellEffect != null)
            {
                // get delta
                var _delta = new Delta(4, typeof(Deltas.Enhancement));

                // add to ability
                (target as Creature)?.Abilities[AbilityMnemonic]?.Deltas.Add(_delta);
                return _delta;
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            // remove delta
            (((DurableMagicEffect)source).ActiveAdjunctObject as Delta)?.DoTerminate();
        }

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
        {
            yield break;
        }

        public IEnumerable<int> DurableSubModes
        {
            get
            {
                yield return 0;
                yield break;
            }
        }

        public bool IsDismissable(int subMode)
        {
            return true;
        }

        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode)
        {
            return @"Save.Will";
        }

        public DurationRule DurationRule(int subMode)
        {
            return new DurationRule(DurationType.Span, new SpanRulePart(1, new Minute(), 1));
        }
        #endregion

        #region ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource,Interaction workSet, string saveKey)
        {
            return new SaveMode(SaveType.Will, SaveEffect.Negates, SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
        }
        #endregion
    }
}
