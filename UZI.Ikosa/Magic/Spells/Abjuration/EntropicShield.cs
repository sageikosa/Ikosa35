using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class EntropicShield : SpellDef, ISpellMode, IDurableCapable, IProcessFeedback
    {
        public override string DisplayName => @"Entropic Shield";
        public override string Description => @"20% miss chance against ranged attacks";
        public override MagicStyle MagicStyle => new Abjuration();

        #region public override IEnumerable<SpellComponent> ArcaneComponents { get; }
        public override IEnumerable<SpellComponent> ArcaneComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
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

        #region ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new PersonalAim(@"Self", actor);
            yield break;
        }

        public bool AllowsSpellResistance => false;
        public bool IsHarmless => true;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurable(deliver, deliver.TargetingProcess.Targets[0], 0);
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
            (target as Creature)?.AddIInteractHandler(this);
            return this;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            (target as Creature)?.RemoveIInteractHandler(source.ActiveAdjunctObject as IInteractHandler);
        }

        public bool IsDismissable(int subMode) { return true; }
        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode) { return string.Empty; }
        public DurationRule DurationRule(int subMode) { return new DurationRule(DurationType.Span, new SpanRulePart(1, new Minute(), 1)); }

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
        {
            yield break;
        }
        #endregion

        #region IInteractHandler Members
        public void HandleInteraction(Interaction workSet)
        {
            return;
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(RangedAttackData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            // put just before the attack handler (so it is processes feedback before the transit attack handler)
            if (typeof(CreatureAttackHandler) == existingHandler.GetType())
                return true;
            return false;
        }
        #endregion

        #region IAlterFeedback Members
        public void ProcessFeedback(Interaction workSet)
        {
            // if we didn't hit, there's no need to check miss chance
            var _feedback = workSet.Feedback.OfType<AttackFeedback>().FirstOrDefault();
            if (!(_feedback?.Hit ?? false))
                return;

            // roll miss chance
            var _score = DieRoller.RollDie(workSet.Target.ID, 100, DisplayName, @"Miss Chance");
            var _miss = new MissChanceAlteration(workSet.InteractData, this, _score);
            workSet.InteractData.Alterations.Add(workSet.Target, _miss);
            if (_score <= 20)
            {
                // missed!
                _feedback.Hit = false;
                _feedback.CriticalHit = false;
            }
        }
        #endregion
    }
}
