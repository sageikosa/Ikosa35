using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;

namespace Uzi.Ikosa.Items.Weapons
{
    [Serializable]
    public class KiFocus : WeaponSpecialAbility
    {
        public KiFocus(object source)
            : base(source, 1, 0)
        {
        }

        public override object Clone()
        {
            return new KiFocus(Source);
        }

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return new Description(Name, @"Ki strike may be made through weapon");
                yield break;
            }
        }

        public override bool Equals(Adjunct other)
            => other is KiFocus;
    }
}
