using System;
using System.Collections.Generic;
using Uzi.Core;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class JumpingDown : Adjunct, IInteractHandler
    {
        public JumpingDown()
            : base(typeof(JumpingDown))
        {
        }

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            var _critter = Anchor as Creature;
            if (_critter != null)
            {
                _critter.AddIInteractHandler(this);
            }
        }

        protected override void OnDeactivate(object source)
        {
            var _critter = Anchor as Creature;
            if (_critter != null)
            {
                _critter.RemoveIInteractHandler(this);
            }
            base.OnDeactivate(source);
        }

        public override object Clone()
        {
            return new JumpingDown();
        }

        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
            if (workSet != null)
            {
                var _add = workSet.InteractData as AddAdjunctData;
                if (_add?.Adjunct is Falling)
                {
                    // deliver change...
                    var _fall = _add.Adjunct as Falling;
                    _fall.FallMovement.FallReduce += 1;

                    // ...and terminate
                    Eject();
                }
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(AddAdjunctData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return true;
        }

        #endregion
    }
}
