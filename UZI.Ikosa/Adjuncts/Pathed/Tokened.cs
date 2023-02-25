using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Ranged;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class Tokened : Pathed
    {
        public Tokened(CoreToken token)
            : base(token)
        {
        }

        public CoreToken Token { get { return Source as CoreToken; } }

        /// <summary>Tokened is controlled by being in a locator</summary>
        public override bool IsProtected { get { return true; } }

        public override object Clone() { return new Tokened(Token); }

        /// <summary>NULL (no path parent)</summary>
        public override IAdjunctable GetPathParent() { return null; }

        public override string GetPathPartString()
            => $@"{Token.Name}";

        public override void UnPath()
        {
            Eject();
        }
    }

    public static class TokenedHelper
    {
        /// <summary>
        /// Gets adjunct that binds this adjunctable to a locator.  
        /// Climbs anchorages and (nested) containers looking for a located adjunct.
        /// </summary>
        public static Tokened GetTokened(this IAdjunctable self)
        {
            // look for direct
            var _token = self.Adjuncts.OfType<Tokened>().FirstOrDefault();
            if (_token != null)
            {
                return _token;
            }

            // look for ammo container
            if (self is IAmmunitionBundle)
            {
                var _aContain = self.Adjuncts.OfType<AmmoContained>().FirstOrDefault();
                if (_aContain != null)
                {
                    return _aContain.Bundle.GetTokened();
                }
            }

            // look for object bound
            var _bound = self.Adjuncts.OfType<ObjectBound>().FirstOrDefault();
            if (_bound != null)
            {
                return _bound.Anchorage.GetTokened();
            }

            // look for contained objects
            var _contain = self.Adjuncts.OfType<Contained>().FirstOrDefault();
            if (_contain != null)
            {
                var _top = _contain.Container.FindTopContainer();
                if (_top != null)
                {
                    return _top.GetTokened();
                }
            }

            // look for items in item slots
            var _slotted = self.Adjuncts.OfType<Slotted>().FirstOrDefault();
            if (_slotted != null)
            {
                return _slotted.ItemSlot.Creature.GetTokened();
            }

            var _mounted = self.Adjuncts.OfType<WieldMounted>().FirstOrDefault();
            if (_mounted != null)
            {
                return _mounted.MountSlot.Creature.GetTokened();
            }

            // look for objects being held
            var _held = self.Adjuncts.OfType<Held>().FirstOrDefault();
            if (_held != null)
            {
                return _held.HoldingWrapper.CreaturePossessor.GetTokened();
            }

            // look for objects attached
            var _attached = self.Adjuncts.OfType<Attached>().FirstOrDefault();
            if (_attached != null)
            {
                return _attached.AttachmentWrapper.CreaturePossessor.GetTokened();
            }

            // objects covering
            var _covering = self.Adjuncts.OfType<Covering>().FirstOrDefault();
            if (_covering != null)
            {
                return _covering.CoveringWrapper.CreaturePossessor.GetTokened();
            }
            return null;
        }
    }

}
