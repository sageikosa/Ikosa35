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
    [MagicAugmentRequirement(10, typeof(CraftMagicArmsAndArmorFeat), typeof(Fireball), typeof(PillarOfFire))] // TODO: flameblade
    public class Flaming : WeaponExtraDamage
    {
        public Flaming()
            : base(typeof(Flaming), 1, 0)
        {
        }

        public override bool CanUseOnRanged => true;

        public override IEnumerable<DamageRollPrerequisite> DamageRollers(Interaction workSet)
        {
            if (PoweredUp)
            {
                yield return
                    new EnergyDamageRollPrerequisite(typeof(Flaming), workSet, @"Fire", @"Flaming", new DieRoller(6),
                    false, @"Flaming", 0, EnergyType.Fire);
            }

            yield break;
        }

        public override object Clone()
            => new Flaming();

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return new Info { Message = @"Flaming: 1d6 Fire" };
                yield break;
            }
        }

        public override bool Equals(Adjunct other)
            => other is Flaming;
    }
}
