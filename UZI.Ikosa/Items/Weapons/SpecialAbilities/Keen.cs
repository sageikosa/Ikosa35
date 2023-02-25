using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [Serializable]
    [MagicAugmentRequirement(10, typeof(CraftMagicArmsAndArmorFeat))] // TODO: keen edge
    public class Keen : WeaponSpecialAbility
    {
        public Keen(object source)
            : base(source, 1, 0)
        {
        }

        protected override bool OnCanBind(IWeapon weapon, IWeaponHead head)
        {
            if (head != null)
            {
                foreach (DamageType _dmgType in head.DamageTypes)
                {
                    if ((_dmgType == DamageType.Piercing) ||
                        (_dmgType == DamageType.Slashing) ||
                        (_dmgType == DamageType.PierceAndSlash) ||
                        (_dmgType == DamageType.BludgeonAndPierce) ||
                        (_dmgType == DamageType.BludgeonAndSlash) ||
                        (_dmgType == DamageType.All))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private Delta _CritRange;

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            var _wpnHead = Anchor as IWeaponHead;
            _CritRange = new Delta(1, typeof(Enhancement), @"Keen");
            _wpnHead.CriticalRangeFactor.Deltas.Add(_CritRange);
        }

        protected override void OnDeactivate(object source)
        {
            _CritRange.DoTerminate();
            base.OnDeactivate(source);
        }

        public override object Clone()
        {
            return new Keen(Source);
        }

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return new Description(Name, @"Critical Threat Range Increased");
                yield break;
            }
        }

        public override bool Equals(Adjunct other)
            => other is Keen;
    }
}
