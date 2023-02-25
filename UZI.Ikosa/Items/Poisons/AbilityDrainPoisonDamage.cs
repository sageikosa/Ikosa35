using System;
using System.Collections.Generic;
using Uzi.Core.Dice;
using Uzi.Core;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class AbilityDrainPoisonDamage : AbilityPoisonDamage
    {
        public AbilityDrainPoisonDamage(string mnemonic, Roller damageAmount)
            : base(mnemonic, damageAmount)
        {
        }

        public override string Name => $@"{base.Name} Drain";
        public override IEnumerable<DamageInfo> ApplyDamage(object source, CoreStep step, Creature critter)
        {
            var _rollVal = DamageAmount.RollValue(critter.ID, $@"{Mnemonic} Drain (Poison)", @"Ability Drain");
            return ApplyDamage(source, step, critter, new int[] { _rollVal });
        }

        public override IEnumerable<DamageInfo> ApplyDamage(object source, CoreStep step, Creature critter, int[] rollValue)
        {
            var _amount = rollValue[0];
            var _drain = new Delta(rollValue[0], typeof(Drain));
            critter.Abilities[Mnemonic].Deltas.Add(_drain);
            yield return new AbilityDamageInfo
            {
                Amount = _amount,
                AbilityMnemonic = Mnemonic,
                IsDrain = true
            };
            yield break;
        }

        public override IEnumerable<PoisonRoll> GetRollers()
        {
            yield return new PoisonRoll
            {
                Key = $@"Drain.{Mnemonic}",
                Roller = DamageAmount,
                Name = $@"{Mnemonic} Drain"
            };
            yield break;
        }
    }
}
