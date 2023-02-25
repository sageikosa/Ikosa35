using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class FascinatedEffect : PredispositionBase, IActionSkip, IInteractHandler
    {
        public FascinatedEffect(object source)
            : base(source)
        {
            _Penalty = new Delta(-4, typeof(FascinatedEffect));
        }

        // TODO: things that allow the fascinated creature another save

        private Delta _Penalty;

        public override string Description
            => @"Fascinated";

        protected override void OnActivate(object source)
        {
            var _critter = Anchor as Creature;
            _critter.Conditions.Add(new Condition(Condition.Fascinated, this));
            _critter.Actions.Filters.Add(this, (IActionFilter)this);
            _critter.AddIInteractHandler(this);
            _critter.Skills.Skill<ListenSkill>().Deltas.Add(_Penalty);
            _critter.Skills.Skill<SpotSkill>().Deltas.Add(_Penalty);
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            var _critter = Anchor as Creature;
            _critter.Conditions.Remove(_critter.Conditions[Condition.Fascinated, this]);
            _critter.Actions.Filters.Remove(this);
            _critter.RemoveIInteractHandler(this);
            _Penalty.DoTerminate();
            base.OnDeactivate(source);
        }

        #region IActionFilter Members
        public bool SuppressAction(object source, CoreActionBudget budget, CoreAction action)
        {
            // unconscious creatures get no actions
            if ((Creature)budget.Actor == (Creature)Anchor)
            {
                // TODO: may allow self-stabilizing action...
                return true;
            }
            return false;
        }
        #endregion

        public virtual void SnapOutOfIt()
        {
            var _effect = Source as MagicPowerEffect;
            if ((_effect != null) && (Anchor != null))
            {
                Anchor.RemoveAdjunct(_effect);
            }
            if (Anchor != null)
            {
                Anchor.RemoveAdjunct(this);
            }
        }

        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
            var _atk = workSet.InteractData as AttackData;
            if (_atk != null)
            {
                SnapOutOfIt();
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(MeleeAttackData);
            yield return typeof(ReachAttackData);
            yield return typeof(RangedAttackData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => true;

        #endregion

        public override object Clone()
            => new FascinatedEffect(Source);
    }
}
