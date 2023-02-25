using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Core
{
    /// <summary>Used to bind an object to an anchorage.  Technically, a mooring; as ObjectBound adjunct is fused to the container.</summary>
    [Serializable]
    public class ObjectBound : Pathed
    {
        /// <summary>Construct with the anchorage (target), then add the adjunct to the thing that needs to be anchored.</summary>
        public ObjectBound(IAnchorage source)
            : base(source)
        {
        }

        /// <summary>Conceptual container that holds the anchor (the anchorage)</summary>
        public IAnchorage Anchorage
            => Source as IAnchorage;

        /// <summary>ObjectBound is protected, indicating that object bound shouldn't be removed via RemoveAdjunct</summary>
        public override bool IsProtected
            => true;

        public override bool CanAnchor(IAdjunctable newAnchor)
            => Anchorage.CanAcceptAnchor(newAnchor);

        public override bool CanUnAnchor()
            => Anchorage.CanEjectAnchor(Anchor);

        public override void UnPath()
        {
            Anchor.UnbindFromObject(Anchorage);
        }

        protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        {
            if (oldAnchor != null)
            {
                // when the adjunct is removed from an adjunctable, let the container know it
                Anchorage.EjectAnchor(oldAnchor);
            }
            if (Anchor != null)
            {
                // when the adjunct is added to an adjunctable, let the container know it
                Anchorage.AcceptAnchor(Anchor);
            }
            base.OnAnchorSet(oldAnchor, oldSetting);
        }

        public override object Clone()
            => new ObjectBound(Anchorage);

        public override IAdjunctable GetPathParent()
            => Anchorage;

        public override string GetPathPartString()
            => $@"{Anchorage.Name}";
    }

    public static class ObjectBoundHelper
    {
        public static IEnumerable<IAnchorage> GetObjectBindings(this IAdjunctable self)
            => self.Adjuncts.OfType<ObjectBound>().Where(_a => _a.IsActive).Select(_a => _a.Anchorage);

        /// <summary>Enumerates all active anchorages (upwards) for this object, recursing as needed.</summary>
        public static IEnumerable<IAdjunctable> GetObjectBoundAnchorages(this IAdjunctable self)
        {
            foreach (var _anchorable in self.GetObjectBindings())
            {
                yield return _anchorable;
                foreach (var _aob in _anchorable.GetObjectBoundAnchorages())
                {
                    yield return _aob;
                }
            }
            yield break;
        }

        public static void BindToObject(this IAdjunctable self, IAnchorage target)
        {
            var _bind = new ObjectBound(target);
            self.AddAdjunct(_bind);
        }

        public static void UnbindFromObject(this IAdjunctable self, IAnchorage target, bool pruneAsNeeded = true)
        {
            foreach (var _ob in self.Adjuncts
                .OfType<ObjectBound>()
                .Where(_a => _a.IsActive && _a.Anchorage == target)
                .ToList())
            {
                _ob.Eject();

                // not supposed to prune, but no other object binding adjuncts
                // NOTE: if something is doubly bound, it doesn't need to be remembered if we sever only one binding
                if (!pruneAsNeeded && !self.Adjuncts.OfType<ObjectBound>().Any(_a => _a.IsActive))
                {
                    // TODO: preserve somehow if object not bound to something else...
                }
            }
        }
    }
}
