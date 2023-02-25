using System;
using System.Collections.Generic;

namespace Uzi.Core
{
    [Serializable]
    public class AdjunctBlocker<AdjType> : Adjunct, IInteractHandler
        where AdjType : Adjunct
    {
        public AdjunctBlocker(object source)
            : base(source)
        {
        }

        public override object Clone()
            => new AdjunctBlocker<AdjType>(Source);

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            if (source == this)
            {
                (Anchor as CoreObject)?.AddIInteractHandler(this);
            }
        }

        protected override void OnDeactivate(object source)
        {
            if (source == this)
            {
                (Anchor as CoreObject)?.RemoveIInteractHandler(this);
            }
            base.OnDeactivate(source);
        }

        protected virtual bool WillBlock(AdjType adjunct)
            => true;

        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
            if (IsActive
                && (workSet?.InteractData as AddAdjunctData)?.Adjunct is AdjType _attempt
                && WillBlock(_attempt))
            {
                // cannot add
                workSet.Feedback.Add(new ValueFeedback<bool>(this, false));
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(AddAdjunctData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => true;

        #endregion
    }
}
