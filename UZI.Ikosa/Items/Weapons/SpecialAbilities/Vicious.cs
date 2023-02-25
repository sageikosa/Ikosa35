using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Ikosa.Actions;
using Uzi.Core;
using Uzi.Core.Dice;

namespace Uzi.Ikosa.Items.Weapons
{
    [Serializable]
    public class Vicious : WeaponExtraDamage
    {
        public Vicious(object source)
            : base(source, 1, 0)
        {
        }

        public override IEnumerable<DamageRollPrerequisite> DamageRollers(Interaction workSet)
        {
            var _roll = new DamageRollPrerequisite(typeof(Vicious), workSet, @"Vicious", @"Vicious", new DiceRoller(2, 6),
                false, false, @"Vicious", 0);
            yield return _roll;
            // TODO: 1d6 back to wielder as well (as AttackResult...)
            yield break;
        }

        public override object Clone()
            => new Vicious(Source);

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                // TODO: description
                yield return new Description(
                    Name,
                    new string[]
                    {
                        @"+2d6 damage to target (on a critical)",
                        @"1d6 damage to wield (on a critical)"
                    });
                yield break;
            }
        }

        public override bool Equals(Adjunct other)
            => other is Vicious;
    }
}
