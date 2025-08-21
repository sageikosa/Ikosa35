using System;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>
    /// Applied to any object directly in a container
    /// </summary>
    [Serializable]
    public class Contained : Pathed
    {
        public Contained(IObjectContainer source)
            : base(source)
        {
        }

        public IObjectContainer Container { get { return Source as IObjectContainer; } }

        /// <summary>Contained is controlled by being in a container</summary>
        public override bool IsProtected { get { return true; } }

        public override object Clone()
        {
            return new Contained(Container);
        }

        public override IAdjunctable GetPathParent() { return Container; }

        public override string GetPathPartString()
            => $@"{Container.Name}";

        public override void UnPath()
        {
            Container.Remove(Anchor as ICoreObject);
        }
    }

    public static class ContainedHelper
    {
        /// <summary>True if the this object is directly contained within a container</summary>
        public static bool IsContained(this ICoreObject self)
        {
            return self.GetContained() != null;
        }

        /// <summary>Gets the adjunct that indicates what this object is directly contained within</summary>
        public static Contained GetContained(this ICoreObject self)
        {
            return self.Adjuncts.OfType<Contained>().FirstOrDefault();
        }

        #region public static Contained ContainedWithin(this ICoreObject self)
        /// <summary>get the adjunct representing the nearest container that holds this object (if at all)</summary>
        public static Contained ContainedWithin(this ICoreObject self)
        {
            // if directly contained
            var _contained = self.GetContained();
            if (_contained != null)
            {
                return _contained;
            }

            // in an ammo container
            var _aContained = self.Adjuncts.OfType<AmmoContained>().FirstOrDefault();
            if (_aContained != null)
            {
                return _aContained.Bundle.ContainedWithin();
            }

            // object bound
            var _bound = self.Adjuncts.OfType<ObjectBound>().FirstOrDefault();
            if (_bound != null)
            {
                return _bound.Anchorage.ContainedWithin();
            }

            // look for items in item slots
            var _slotted = self.Adjuncts.OfType<Slotted>().FirstOrDefault();
            if (_slotted != null)
            {
                return _slotted.ItemSlot.Creature.GetContained();
            }

            var _mounted = self.Adjuncts.OfType<WieldMounted>().FirstOrDefault();
            if (_mounted != null)
            {
                return _mounted.MountSlot.Creature.GetContained();
            }

            // look for objects being held
            var _held = self.Adjuncts.OfType<Held>().FirstOrDefault();
            if (_held != null)
            {
                return _held.HoldingWrapper.CreaturePossessor.GetContained();
            }

            // look for objects attached
            var _attached = self.Adjuncts.OfType<Attached>().FirstOrDefault();
            if (_attached != null)
            {
                return _attached.AttachmentWrapper.CreaturePossessor.GetContained();
            }

            // objects covering
            var _covering = self.Adjuncts.OfType<Covering>().FirstOrDefault();
            if (_covering != null)
            {
                return _covering.CoveringWrapper.CreaturePossessor.GetContained();
            }
            return null;
        }
        #endregion

        /// <summary>Climbs containers looking for a container that will accept the object</summary>
        public static IObjectContainer FindAcceptableContainer(this ICoreObject self, IObjectContainer start)
        {
            // if the initial contained can hold the object, OK...
            if (start.CanHold(self))
            {
                return start;
            }

            // otherwise look for the container it is within
            var _contained = start.ContainedWithin();
            if (_contained != null)
            {
                return self.FindAcceptableContainer(_contained.Container);
            }
            return null;
        }

        /// <summary>Climbs containers looking for a container that is not itself contained</summary>
        public static IObjectContainer FindTopContainer(this IObjectContainer self)
        {
            // otherwise look for the container it is within
            var _contained = self.ContainedWithin();
            if (_contained == null)
            {
                return self;
            }
            else
            {
                return _contained.Container.FindTopContainer();
            }
        }
    }
}
