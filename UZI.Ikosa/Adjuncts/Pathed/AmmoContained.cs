using System;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Ranged;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Applied to any ammoset in an ammo container</summary>
    [Serializable]
    public class AmmoContained : Pathed
    {
        public AmmoContained(IAmmunitionBundle source)
            : base(source)
        {
        }

        public IAmmunitionBundle Bundle
            => Source as IAmmunitionBundle;

        public override bool IsProtected
            => true;

        public override object Clone()
        {
            return new AmmoContained(Bundle);
        }

        public override IAdjunctable GetPathParent()
            => Bundle;

        public override string GetPathPartString()
            => $@"{((ICoreObject)Bundle).Name}";

        public override void UnPath()
        {
        }
    }
}