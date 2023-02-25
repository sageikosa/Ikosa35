using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class FeatherFall : SpellDef, ISpellMode, IDurableCapable, ISaveCapable, IReactToStepCompleteCapable
    {
        public override string DisplayName => @"Feather Fall";
        public override string Description => @"One or more creatures fall at 60 feet per round.";
        public override MagicStyle MagicStyle => new Transformation();

        public override ActionTime ActionTime => new ActionTime(TimeType.Reactive);
        public override IEnumerable<SpellComponent> DivineComponents => new VerbalComponent().ToEnumerable();
        public override IEnumerable<SpellComponent> ArcaneComponents => new VerbalComponent().ToEnumerable();
        public override IEnumerable<ISpellMode> SpellModes => this.ToEnumerable();

        #region ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new AwarenessAim(@"Target", @"Target", FixedRange.One,
                new PowerLevelRange(1), new NearRange(),
                new ITargetType[] { new CreatureTargetType(), new ObjectTargetType() });
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => true;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            var _activity = deliver.TargetingProcess as CoreActivity;
            var _targets = (from _aware in _activity.Targets.OfType<AwarenessTarget>()
                            where _aware.Key.Equals(@"Target", StringComparison.OrdinalIgnoreCase)
                            select _aware).Cast<AimTarget>().ToList();

            // get maximum
            var _tAim = _activity.Action.AimingMode(_activity).FirstOrDefault(_m => _m.Key == @"Target");
            var _level = _activity.Action.CoreActionClassLevel(deliver.Actor, this);
            var _avail = _tAim?.MaximumAimingModes.EffectiveRange(deliver.Actor, _level);

            // count down by size extensions
            var _final = new List<AimTarget>();
            foreach (var (_target, _sizer) in _targets.Select(_t => (_t, _t.Target as ISizable)))
            {
                _avail -= _sizer.Sizer.Size.Order switch
                {
                    1 => 2,
                    2 => 4,
                    3 => 8,
                    4 => 16,
                    _ => 1,
                };
                if (_avail >= 0)
                {
                    _final.Add(_target);
                }
                else
                {
                    break;
                }
            }
            if (_final.Any())
            {
                SpellDef.DeliverDurableInCluster(deliver, _final, true, 20, 0);
            }
            else
            {
                deliver.Notify(@"No targets within spell capacity.", @"Failed", false);
            }
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDurableMagicEffects(apply);
        }
        #endregion

        #region IDurableMode Members
        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            if (source is MagicPowerEffect _spellEffect)
            {
                var _slowFall = new SlowFallEffect(_spellEffect);
                target.AddAdjunct(_slowFall);
                return _slowFall;
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            (source.ActiveAdjunctObject as SlowFallEffect)?.Eject();
        }

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
        {
            yield break;
        }

        public IEnumerable<int> DurableSubModes => 0.ToEnumerable();

        public bool IsDismissable(int subMode)
            => false;

        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode)
            => @"Save.Will";

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Round(), 1));
        #endregion

        #region ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource,Interaction workSet, string saveKey)
        {
            return new SaveMode(SaveType.Will, SaveEffect.Negates,
                SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
        }
        #endregion

        public bool WillReactToStepComplete(Creature actor, CoreStep step)
            => ((step is FallingStep _fallStep)
            && _fallStep.BaseFallMovement.IsUncontrolled
            && (actor.Awarenesses[_fallStep.BaseFallMovement.CoreObject.ID] >= Senses.AwarenessLevel.Aware));
    }
}
