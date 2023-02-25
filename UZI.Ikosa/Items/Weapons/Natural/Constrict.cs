using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Items.Weapons.Natural
{
    [Serializable]
    public class Constrict : WeaponSecondaryAttackResult
    {
        // TODO: part of weapon, or part of grapple rules

        public Constrict(object source)
            : base(source, 0, 0)
        {
        }

        public override object Clone()
            => new Constrict(Source);

        public override IEnumerable<Info> IdentificationInfos 
            => (new Info { Message = @"Constrict" }).ToEnumerable();

        public override void AttackResult(StepInteraction deliverDamageInteraction)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<StepPrerequisite> AttackResultPrerequisites(Interaction workSet)
        {
            throw new NotImplementedException();
        }

        public override bool IsDamageSufficient(StepInteraction final)
        {
            throw new NotImplementedException();
        }
    }
}
