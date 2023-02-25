using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class Virtue : SpellDef, ISpellMode, IDurableCapable, ISaveCapable, IDurableAnchorCapable
    {
        public override string DisplayName => @"Virtue";
        public override string Description => @"Creature gains 1 Temporary HP for duration";
        public override MagicStyle MagicStyle => new Transformation();

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

        public override IEnumerable<ISpellMode> SpellModes => this.ToEnumerable();

        #region ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new TouchAim(@"Creature", @"Creature", Lethality.AlwaysNonLethal,
                20, this, FixedRange.One, FixedRange.One, new MeleeRange(), new CreatureTargetType());
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => true;

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
        public IEnumerable<int> DurableSubModes => 0.ToEnumerable();

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
        }

        public bool IsDismissable(int subMode)
            => false;

        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode)
            => @"Save.Fortitude";

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Minute()));

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact) { yield break; }
        #endregion

        #region ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource,Interaction workSet, string saveKey)
        {
            return new SaveMode(SaveType.Fortitude, SaveEffect.Negates, SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
        }
        #endregion

        #region IDurableAnchorMode Members
        public object OnAnchor(IAdjunctTracker source, IAdjunctable target)
        {
            if (target is Creature _critter)
            {
                // create the TempHP, but keep it disabled...
                var _tempPt = new Delta(1, typeof(Virtue));

                // add it as a chunk
                var _chunk = new TempHPChunk(_critter.TempHealthPoints, _tempPt);
                _critter.TempHealthPoints.Add(_chunk);
                return _chunk;
            }
            return null;
        }

        public void OnEndAnchor(IAdjunctTracker source, IAdjunctable target)
        {
            if (target is Creature _critter)
            {
                // remove the chunk
                if (source.AnchoredAdjunctObject is TempHPChunk _chunk)
                {
                    _critter.TempHealthPoints.Remove(_chunk);
                }
            }
        }
        #endregion
    }
}
