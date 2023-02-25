using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Time;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class Flare : SpellDef, ISpellMode, IDurableCapable, ISaveCapable, IPowerDeliverVisualize
    {
        public override string DisplayName => @"Flare";
        public override string Description => @"-1 ATK, search and spot";
        public override MagicStyle MagicStyle => new Evocation();

        #region public override IEnumerable<Descriptor> Descriptors { get; }
        public override IEnumerable<Descriptor> Descriptors
        {
            get
            {
                yield return new Actions.Light();
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
                yield break;
            }
        }
        #endregion

        #region public override IEnumerable<ISpellMode> SpellModes { get; }
        public override IEnumerable<ISpellMode> SpellModes
        {
            get
            {
                yield return (ISpellMode)this;
                yield break;
            }
        }
        #endregion

        #region ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new AwarenessAim(@"Creature", @"Creature", new FixedRange(1), new FixedRange(1), new NearRange(), new CreatureTargetType());
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => false;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            var _target = deliver.TargetingProcess.Targets[0] as AwarenessTarget;
            var _critter = _target.Target as Creature;
            if (_critter.Senses.AllSenses.Where(s => s.UsesLight).Count() > 0)
            {
                SpellDef.DeliverDurable(deliver, _target, 0);
            }
            else
            {
                deliver.Notify(@"Target has no light based sense.", @"Failed", false);
            }
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDurableMagicEffects(apply);
        }
        #endregion

        #region IDurableSpellMode Members
        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            var _critter = target as Creature;
            var _dazzled = new Condition(Condition.Dazzled, source);
            _critter.Conditions.Add(_dazzled);
            var _bedazzled = new Delta(-1, typeof(Dazzled), @"Dazzled");
            if (_critter.Conditions.Contains(_dazzled))
            {
                _critter.BaseAttack.Deltas.Add(_bedazzled);
                _critter.Skills[typeof(SearchSkill)].Deltas.Add(_bedazzled);
                _critter.Skills[typeof(SpotSkill)].Deltas.Add(_bedazzled);
            }
            return _bedazzled;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            // _bedazzled Named delta
            var _critter = target as Creature;
            var _bedazzled = (Delta)source.ActiveAdjunctObject;
            var _dazzled = _critter.Conditions[Condition.Dazzled, source];
            _bedazzled.DoTerminate();
            _critter.Conditions.Remove(_dazzled);
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
            => false;

        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode)
            => @"Save.Fortitude";

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Minute()));
        #endregion

        // ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource,Interaction workSet, string saveKey)
            => new SaveMode(SaveType.Fortitude, SaveEffect.Negates, 
                SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));

        // IPowerDeliverVisualize Members
        public VisualizeTransferType GetTransferType() => VisualizeTransferType.None;
        public VisualizeTransferSize GetTransferSize() => VisualizeTransferSize.Medium;
        public string GetTransferMaterialKey() => string.Empty;
        public VisualizeSplashType GetSplashType() => VisualizeSplashType.Uniform;
        public string GetSplashMaterialKey() => @"#E0FFFF00|#C0FFFF40|#E0FFFF00";
    }
}
