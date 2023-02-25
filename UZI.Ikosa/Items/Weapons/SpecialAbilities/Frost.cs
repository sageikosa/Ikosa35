using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Ikosa.Actions;
using Uzi.Core.Dice;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [Serializable]
    [MagicAugmentRequirement(10, typeof(CraftMagicArmsAndArmorFeat))] // TODO: chill metal, ice storm
    public class Frost : WeaponExtraDamage
    {
        public Frost(object source)
            : base(source, 1, 0)
        {
        }

        public override bool CanUseOnRanged => true;

        public override IEnumerable<DamageRollPrerequisite> DamageRollers(Interaction workSet)
        {
            if (PoweredUp)
                yield return
                    new EnergyDamageRollPrerequisite(typeof(Frost), workSet, @"Cold", @"Frost", new DieRoller(6), 
                    false, @"Frost", 0, EnergyType.Cold);
            yield break;
        }

        public override object Clone()
            => new Frost(Source);

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return new Info { Message = @"Frost: 1d6 Cold" };
                yield break;
            }
        }

        public override bool Equals(Adjunct other)
            => other is Frost;
    }
}
