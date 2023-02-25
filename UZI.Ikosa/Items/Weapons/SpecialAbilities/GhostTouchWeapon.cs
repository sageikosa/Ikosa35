using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [Serializable]
    public class GhostTouchWeapon : WeaponSpecialAbility
    {
        public GhostTouchWeapon(object source)
            : base(source, 1, 0)
        {
        }

        public override object Clone()
        {
            return new GhostTouchWeapon(Source);
        }
        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return new Description(
                    Name,
                    new string[]
                    {   
                        @"Effective against incorporeal targets",
                        @"Incorporeal actors may wield"
                    });
                yield break;
            }
        }

        public override bool Equals(Adjunct other)
            => other is GhostTouchWeapon;
    }
}
