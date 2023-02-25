using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class OnSurface : GroupMemberAdjunct, IProcessFeedback, IPathDependent
    {
        public OnSurface(SurfaceGroup surface)
            : base(typeof(SurfaceGroup), surface)
        {
        }

        public SurfaceGroup Surface { get { return Group as SurfaceGroup; } }
        public ICoreObject CoreObject => Anchor as ICoreObject;

        public override bool CanAnchor(IAdjunctable newAnchor)
            => (newAnchor is ICoreObject) && base.CanAnchor(newAnchor);

        public override object Clone()
            => new OnSurface(Surface);

        protected override void OnActivate(object source)
        {
            CoreObject?.AddIInteractHandler(this);
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            CoreObject?.RemoveIInteractHandler(this);
            base.OnDeactivate(source);
        }

        #region IPathDependent Members

        public override void PathChanged(Pathed source)
        {
            if (source is Located)
            {
                // see if we are directly locatable
                var _loc = Anchor.Adjuncts.OfType<Located>().FirstOrDefault();
                if (_loc == null)
                {
                    // not locatable, then eject...
                    Eject();
                    return;
                }
            }
            base.PathChanged(source);
        }

        #endregion

        #region IProcessFeedback
        public void ProcessFeedback(Interaction workSet)
        {
            // successfully added this locator to the gathering set?
            if (workSet.InteractData is GetLocatorsData _gld
                && workSet.Feedback.OfType<ValueFeedback<bool>>().Any(_vfb => _vfb.Value))
            {
                // things on surface add cost (via weight)
                _gld.SetCost(CoreObject, 1);
            }
        }

        public void HandleInteraction(Interaction workSet)
        {
            // FEEDBACK ONLY!
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(GetLocatorsData);
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => (interactType == typeof(GetLocatorsData))
            && (existingHandler is GetLocatorsHandler);

        #endregion
    }
}
