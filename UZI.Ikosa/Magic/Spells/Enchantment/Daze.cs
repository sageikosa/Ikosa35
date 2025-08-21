using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Time;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class Daze : SpellDef, ISpellMode, IDurableCapable, IActionSkip, ISaveCapable
    {
        public override string DisplayName => @"Daze";
        public override string Description => @"No actions for one round for humanoid with 4 PD or less";
        public override MagicStyle MagicStyle => new Enchantment(Enchantment.SubEnchantment.Compulsion);

        #region public override IEnumerable<Descriptor> Descriptors { get; }
        public override IEnumerable<Descriptor> Descriptors
        {
            get
            {
                yield return new MindAffecting();
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
                yield return new MaterialComponent();
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
            yield return new AwarenessAim(@"Humanoid", @"Humanoid", new FixedRange(1), new FixedRange(1), new NearRange(), new CreatureTypeTargetType<HumanoidType>());
            yield break;
        }
        public bool AllowsSpellResistance => true;
        public bool IsHarmless => false;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            var _target = deliver.TargetingProcess.Targets[0] as AwarenessTarget;
            var _critter = _target.Target as Creature;
            if (_critter.AdvancementLog.NumberPowerDice <= 4)
            {
                SpellDef.DeliverDurable(deliver, _target, 0);
            }
            else
            {
                deliver.Notify(@"Target PD exceeds spell capacity.", @"Failed", false);
            }
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDurableMagicEffects(apply);
        }
        #endregion

        #region IActionFilter Members
        public bool SuppressAction(object source, CoreActionBudget budget, CoreAction action)
            => true;
        #endregion

        #region IDurableSpellMode Members
        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            var _critter = target as Creature;
            var _dazed = new Condition(Condition.Dazed, source);
            _critter.Conditions.Add(_dazed);
            if (_critter.Conditions.Contains(_dazed))
            {
                _critter.Actions.Filters.Add(_dazed, (IActionFilter)this);
            }

            return _dazed;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            var _critter = target as Creature;
            var _dazed = source.ActiveAdjunctObject as Condition;
            if (_critter.Conditions.Contains(_dazed))
            {
                _critter.Actions.Filters.Remove(source.ActiveAdjunctObject);
                _critter.Conditions.Remove(_dazed);
            }
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
            => @"Save.Will";

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Round()));
        #endregion

        #region ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource,Interaction workSet, string saveKey)
        {
            return new SaveMode(SaveType.Will, SaveEffect.Negates, SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
        }
        #endregion
    }
}
