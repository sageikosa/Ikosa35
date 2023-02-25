using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>
    /// Added to anchor of the durable magic effect.  
    /// Monitors the effect, and if it is ejected, the control group is also terminated.
    /// </summary>
    [Serializable]
    public class DismissibleMagicEffectTarget : GroupMemberAdjunct, IProcessFeedback
    {
        /// <summary>
        /// Added to anchor of the durable magic effect.  
        /// Monitors the effect, and if it is ejected, the control group is also terminated.
        /// </summary>
        public DismissibleMagicEffectTarget(DurableMagicEffect effect, DismissibleMagicEffectControl group)
            : base(effect, group)
        {
        }

        public DurableMagicEffect DurableMagicEffect => Source as DurableMagicEffect;
        public DismissibleMagicEffectControl Control => Group as DismissibleMagicEffectControl;

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            (Anchor as IInteractHandlerExtendable)?.AddIInteractHandler(this);
        }

        protected override void OnDeactivate(object source)
        {
            (Anchor as IInteractHandlerExtendable)?.RemoveIInteractHandler(this);
            base.OnDeactivate(source);
        }

        public override object Clone()
            => new DismissibleMagicEffectTarget(DurableMagicEffect, Control);

        // IProcessFeedback (RemoveAdjunctData)
        public IEnumerable<Type> GetInteractionTypes()
            => typeof(RemoveAdjunctData).ToEnumerable();

        public void HandleInteraction(Interaction workSet)
        {
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => true;

        public void ProcessFeedback(Interaction workSet)
        {
            if (workSet.InteractData is RemoveAdjunctData _remove
                && _remove.DidRemove(workSet)
                && (_remove.Adjunct == DurableMagicEffect))
            {
                // done if durable magic effect has ended
                Control.EjectMembers();
            }
        }
    }
}
