using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class SurfaceContainer : GroupMasterAdjunct, IProcessFeedback
    {
        public SurfaceContainer(SurfaceGroup surface)
            : base(typeof(SurfaceGroup), surface)
        {
        }

        public SurfaceGroup Surface => Group as SurfaceGroup;
        public ICoreObject CoreObject => Anchor as ICoreObject;

        public override bool CanAnchor(IAdjunctable newAnchor)
            => (newAnchor is ICoreObject) && base.CanAnchor(newAnchor);

        public override object Clone()
            => new SurfaceContainer(Surface);

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

        #region IProcessFeedback
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(GetLocatorsData);
        }

        public void HandleInteraction(Interaction workSet)
        {
            // FEEDBACK ONLY!
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => (interactType == typeof(GetLocatorsData))
            && (existingHandler is GetLocatorsHandler);

        public void ProcessFeedback(Interaction workSet)
        {
            // successfully added this locator to the gathering set?
            if (workSet.InteractData is GetLocatorsData _gld
                && workSet.Feedback.OfType<ValueFeedback<bool>>().Any(_vfb => _vfb.Value))
            {
                // if any were retrieved, then add the object also...
                var _move = workSet.Source as MovementBase;
                Parallel.ForEach(
                    Surface?.Contained.Select(_p => _p.CoreObject),
                    (_crt => _gld.AddObject(_crt, _move)));

                // movement sourced from something else increases cost
                if (_move.CoreObject != CoreObject)
                {
                    _gld.SetCost(CoreObject, 1);
                }
            }
        }
        #endregion
    }
}
