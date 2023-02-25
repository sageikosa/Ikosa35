using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Ranged;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>
    /// Used to associate adjuncts with a locator
    /// </summary>
    [Serializable]
    public class Located : Tokened
    {
        public Located(Locator locator)
            : base(locator)
        {
        }

        public Locator Locator { get { return Token as Locator; } }

        public override object Clone() { return new Located(Locator); }

        public override string GetPathPartString()
            => $@"{Locator.Name}";

        public override void UnPath()
        {
            Locator.MapContext.Remove(Locator);
        }
    }

    public static class LocatedHelper
    {
        /// <summary>
        /// Gets adjunct that binds this adjunctable to a locator.  
        /// Climbs anchorages and (nested) containers looking for a located adjunct.
        /// </summary>
        public static Located GetLocated(this IAdjunctSet self)
        {
            // look for direct
            var _loc = self.Adjuncts.OfType<Located>().FirstOrDefault();
            if (_loc != null)
            {
                return _loc;
            }

            // look for ammo container
            if (self is IAmmunitionBundle)
            {
                var _aContain = self.Adjuncts.OfType<AmmoContained>().FirstOrDefault();
                if (_aContain != null)
                {
                    return _aContain.Bundle.GetLocated();
                }
            }

            // look for object bound
            var _bound = self.Adjuncts.OfType<ObjectBound>().FirstOrDefault();
            if (_bound != null)
            {
                return _bound.Anchorage.GetLocated();
            }

            // look for contained objects
            var _contain = self.Adjuncts.OfType<Contained>().FirstOrDefault();
            if (_contain != null)
            {
                var _top = _contain.Container.FindTopContainer();
                if (_top != null)
                {
                    return _top.GetLocated();
                }
            }

            // look for items in item slots
            var _slotted = self.Adjuncts.OfType<Slotted>().FirstOrDefault();
            if (_slotted != null)
            {
                return _slotted.ItemSlot.Creature.GetLocated();
            }

            var _mounted = self.Adjuncts.OfType<WieldMounted>().FirstOrDefault();
            if (_mounted != null)
            {
                return _mounted.MountSlot.Creature.GetLocated();
            }

            // look for objects being held
            var _held = self.Adjuncts.OfType<Held>().FirstOrDefault();
            if (_held != null)
            {
                return _held.HoldingWrapper.CreaturePossessor.GetLocated();
            }

            // look for objects attached
            var _attached = self.Adjuncts.OfType<Attached>().FirstOrDefault();
            if (_attached != null)
            {
                return _attached.AttachmentWrapper.CreaturePossessor.GetLocated();
            }

            // objects covering
            var _covering = self.Adjuncts.OfType<Covering>().FirstOrDefault();
            if (_covering != null)
            {
                return _covering.CoveringWrapper.CreaturePossessor.GetLocated();
            }
            return null;
        }

        public static Located GetDirectLocated(this IAdjunctable self)
        {
            return self.Adjuncts.OfType<Located>().FirstOrDefault();
        }

        /// <summary>Get planar presence of object</summary>
        public static PlanarPresence GetPlanarPresence(this IAdjunctSet self)
            => self?.GetLocated()?.Locator.PlanarPresence ?? PlanarPresence.None;
    }
}
