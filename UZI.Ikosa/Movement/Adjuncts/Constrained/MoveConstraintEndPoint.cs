using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Movement.Adjuncts
{
    [Serializable]
    public class MoveConstraintEndPoint : GroupMemberAdjunct, IInteractHandler
    {
        public MoveConstraintEndPoint(object source, AdjunctGroup group)
            : base(source, group)
        {
        }

        public override object Clone()
            => new MoveConstraintEndPoint(Source, Group);

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            // move challenger add
            (Anchor as IInteractHandlerExtendable)?.AddIInteractHandler(this);
            base.OnActivate(source);
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            base.OnDeactivate(source);

            // move challenger eject
            (Anchor as IInteractHandlerExtendable)?.RemoveIInteractHandler(this);
        }
        #endregion

        public void HandleInteraction(Interaction workSet)
        {
            if (workSet?.InteractData is PreRelocateData _preRelocate)
            {
                // TODO: need to make an opposed strength check to move...outside of zone defined by holder of rope
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(PreRelocateData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            if (interactType == typeof(PreRelocateData))
            {
                return true;
            }

            return false;
        }
    }
}
