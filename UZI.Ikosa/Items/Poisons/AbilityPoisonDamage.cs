using System;
using System.Collections.Generic;
using Uzi.Ikosa.Adjuncts;
using Uzi.Core.Dice;
using Uzi.Ikosa.Contracts;
using Uzi.Core;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class AbilityPoisonDamage : PoisonDamage
    {
        public AbilityPoisonDamage(string mnemonic, Roller damageAmount)
            : base()
        {
            Mnemonic = mnemonic;
            DamageAmount = damageAmount;
        }

        public string Mnemonic { get; private set; }
        public Roller DamageAmount { get; private set; }
        public override string Name => $@"{DamageAmount} {Mnemonic}";

        public override IEnumerable<DamageInfo> ApplyDamage(object source, CoreStep step, Creature critter)
            => ApplyDamage(source, step, critter, new int[] { DamageAmount.RollValue(critter.ID, $@"{Mnemonic} Damage (Poison)", @"Ability Damage") });

        public override IEnumerable<DamageInfo> ApplyDamage(object source, CoreStep step, Creature critter, int[] rollValue)
        {
            var _amount = rollValue[0];
            critter.Abilities[Mnemonic].AddDamage(_amount, source);
            yield return new AbilityDamageInfo
            {
                Amount = _amount,
                AbilityMnemonic = Mnemonic,
                IsDrain = false
            };
            yield break;
        }

        public override IEnumerable<PoisonRoll> GetRollers()
        {
            yield return new PoisonRoll
            {
                Key = $@"Damage.{Mnemonic}",
                Roller = DamageAmount,
                Name = $@"{Mnemonic} Damage"
            };
            yield break;
        }
    }
}