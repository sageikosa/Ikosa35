using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Actions;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Core.Dice;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [Serializable]
    [MagicAugmentRequirement(5, typeof(CraftMagicArmsAndArmorFeat))] // TODO: blindness/deafness
    public class Thundering : WeaponExtraDamage
    {
        public Thundering(object source)
            : base(source, 1, 0)
        {
        }

        public override bool CanUseOnRanged => true;

        public override IEnumerable<DamageRollPrerequisite> DamageRollers(Interaction workSet)
        {
            if ((workSet.Feedback.Where(_f => _f.GetType() == typeof(AttackFeedback)).FirstOrDefault() as AttackFeedback).CriticalHit)
            {
                var _roll = new EnergyDamageRollPrerequisite(typeof(Thundering), workSet, @"Sonic", @"Thundering",
                    new DiceRoller(AttackSource.CriticalDamageFactor.QualifiedValue(workSet) - 1, 8), 
                    false, @"Thundering", 0, EnergyType.Sonic);
                yield return _roll;
                // TODO: target must save (Fort DC 14) or be permanently deafened
            }
            yield break;
        }

        public override object Clone()
            => new Thundering(Source);

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return new Description(Name, @"1d8 (or more) sonic damage on critical hit");
                //yield return new Description(this.Name, @"On critical hit, save or be deafened");
                yield break;
            }
        }

        public override bool Equals(Adjunct other)
            => other is Thundering;
    }
}
