using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class Doom : SpellDef, ISpellMode, IDurableCapable, ISaveCapable
    {
        public override string DisplayName { get { return @"Doom"; } }
        public override string Description { get { return @"One living creature becomes shaken."; } }
        public override MagicStyle MagicStyle { get { return new Necromancy(); } }

        #region public override IEnumerable<SpellComponent> ArcaneComponents { get; }
        public override IEnumerable<SpellComponent> ArcaneComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield return new FocusComponent();
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
                yield break;
            }
        }
        #endregion

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

        #region ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new AwarenessAim(@"Creature", @"Creature", FixedRange.One, FixedRange.One, new MediumRange(), new LivingCreatureTargetType());
            yield break;
        }

        public bool AllowsSpellResistance { get { return true; } }
        public bool IsHarmless { get { return false; } }

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            Creature _critter = deliver.TargetingProcess.Targets[0].Target as Creature;
            if ((_critter != null) && _critter.CreatureType.IsLiving)
            {
                SpellDef.DeliverDurable(deliver, deliver.TargetingProcess.Targets[0], 0);
            }
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDurableMagicEffects(apply);
        }
        #endregion

        #region IDurableMode Members
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
            ShakenEffect _shaken = new ShakenEffect(source);
            Creature _critter = target as Creature;
            if (_critter != null)
            {
                _critter.AddAdjunct(_shaken);
            }
            return _shaken;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            ShakenEffect _shaken = source.ActiveAdjunctObject as ShakenEffect;
            Creature _critter = target as Creature;
            if ((_shaken != null) && (_critter != null))
                _critter.RemoveAdjunct(_shaken);
        }

        public bool IsDismissable(int subMode) { return false; }
        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode) { return @"Save.Will"; }
        public DurationRule DurationRule(int subMode) { return new DurationRule(DurationType.Span, new SpanRulePart(1, new Minute(), 1)); }
        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact) { yield break; }
        #endregion

        #region ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource,Interaction workSet, string saveKey)
        {
            return new SaveMode(SaveType.Will, SaveEffect.Negates, SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
        }
        #endregion
    }
}
