using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;

namespace Uzi.Ikosa.Items.Weapons
{
    [Serializable]
    public class Returning : WeaponSpecialAbility
    {
        public Returning(object source)
            : base(source, 1, 0)
        {
        }

        public override bool CanUseOnMelee { get { return false; } }
        public override bool CanUseOnRanged { get { return true; } }

        public override object Clone()
        {
            return new Returning(Source);
        }

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return new Description(Name, @"Returns to wielder after a ranged attack");
                yield break;
            }
        }

        public override bool Equals(Adjunct other)
            => other is Returning;
    }
}
