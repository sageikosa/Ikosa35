using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;

namespace Uzi.Ikosa.Items.Weapons
{
    [Serializable]
    public class Distance : WeaponSpecialAbility
    {
        public Distance(object source)
            : base(source, 1, 0)
        {
        }

        public override bool CanUseOnMelee { get { return false; } }
        public override bool CanUseOnRanged { get { return true; } }

        public override object Clone()
        {
            return new Distance(Source);
        }

        // TODO: add range increment delta

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                // TODO: description?
                yield return new Info { Message = @"Distance: Range increment doubled" };
                yield break;
            }
        }

        public override bool Equals(Adjunct other)
            => other is Distance;
    }
}