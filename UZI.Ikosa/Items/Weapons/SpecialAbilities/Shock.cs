using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Ikosa.Actions;
using Uzi.Core.Dice;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Magic.Spells;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [Serializable]
    [MagicAugmentRequirement(8, typeof(CraftMagicArmsAndArmorFeat), typeof(LightningBolt))] // TODO: call lightning
    public class Shock : WeaponExtraDamage
    {
        public Shock()
            : base(typeof(Shock), 1, 0)
        {
        }

        public override bool CanUseOnRanged => true;

        public override IEnumerable<DamageRollPrerequisite> DamageRollers(Interaction workSet)
        {
            if (PoweredUp)
            {
                yield return
                    new EnergyDamageRollPrerequisite(typeof(Shock), workSet, @"Electric", @"Shock", new DieRoller(6),
                    false, @"Shock", 0, EnergyType.Electric);
            }

            yield break;
        }

        public override object Clone()
            => new Shock();

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return new Info { Message = @"Shock: 1d6 Electric" };
                yield break;
            }
        }

        public override bool Equals(Adjunct other)
            => other is Shock;
    }
}
