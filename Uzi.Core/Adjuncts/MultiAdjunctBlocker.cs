using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Core
{
    [Serializable]
    public class MultiAdjunctBlocker : Adjunct, IInteractHandler
    {
        public MultiAdjunctBlocker(object source, string reason, params Type[] adjunctTypes)
            : base(source)
        {
            _Blocked = adjunctTypes.ToList();
            _Reason = reason;
        }

        #region state
        private readonly List<Type> _Blocked;
        private readonly string _Reason;
        #endregion

        public IEnumerable<Type> Blocked => _Blocked.Select(_b => _b);
        public string Reason => _Reason;

        public override object Clone()
            => new MultiAdjunctBlocker(Source, Reason, _Blocked.ToArray());

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

        #region IInteractHandler Members

        public virtual void HandleInteraction(Interaction workSet)
        {
            if (IsActive
                && workSet?.InteractData is AddAdjunctData _data)
            {
                // cannot add
                if (_Blocked.Any(_b => _b.IsAssignableFrom(_data.Adjunct.GetType())))
                    workSet.Feedback.Add(new ValueFeedback<bool>(this, false));
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
