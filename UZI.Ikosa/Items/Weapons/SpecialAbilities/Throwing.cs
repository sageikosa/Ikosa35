using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Feats;
using Uzi.Core;

namespace Uzi.Ikosa.Items.Weapons
{
    [Serializable]
    [MagicAugmentRequirement(5, typeof(CraftMagicArmsAndArmorFeat))] // TODO: magic stone
    public class Throwing : WeaponSpecialAbility, IThrowableWeapon
    {
        public Throwing(object source)
            : base(source, 1, 0)
        {
        }

        public override object Clone() { return new Throwing(Source); }

        #region IRangedSource Members

        public int RangeIncrement { get { return 10; } }
        public int MaxRange { get { return 50; } }

        #endregion

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return new Info { Message = @"Throwing: Weapon may be thrown without penalty" };
                yield break;
            }
        }

        public IWeaponHead MainHead => null;
        public override bool Equals(Adjunct other)
            => other is Throwing;
    }
}