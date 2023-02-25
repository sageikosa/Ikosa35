using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class RestEffect : ActorStateBase, IInteractHandler
    {
        public RestEffect(object source)
            : base(source)
        {
        }

        public override object Clone()
            => new RestEffect(Source);

        protected override void OnActivate(object source)
        {
            Critter?.AddIInteractHandler(this);

            // notify
            NotifyStateChange(false, true, true);
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            var _critter = Critter;
            if (_critter != null)
            {
                _critter.RemoveIInteractHandler(this);

                // remove any programmed rest action
                if (_critter.GetLocalActionBudget() is LocalActionBudget _budget
                    && _budget.NextActivity?.Action is Rest)
                {
                    _budget.NextActivity = null;
                }
            }

            // notify
            NotifyStateChange(true, false, false);
            base.OnDeactivate(source);
        }

        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
            // any damage, regardless of whether it is "actual" ends the rest
            if (workSet.InteractData is IDeliverDamage _damage)
            {
                Anchor.RemoveAdjunct(this);
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(DeliverDamageData);
            yield return typeof(SaveableDamageData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => existingHandler switch
            {
                DamageReductionHandler _ => true,
                EnergyResistanceHandler _ => true,
                EvasionHandler _ => true,
                _ => false,
            };
        #endregion
    }
}
