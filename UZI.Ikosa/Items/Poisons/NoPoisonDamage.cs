using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items
{
    public class NoPoisonDamage : PoisonDamage
    {
        public NoPoisonDamage() : base() { }

        public override IEnumerable<DamageInfo> ApplyDamage(object source, CoreStep step, Creature critter)
        {
            yield break;
        }

        public override IEnumerable<DamageInfo> ApplyDamage(object source, CoreStep step, Creature critter, int[] rollValue)
        {
            yield break;
        }

        public override string Name => @"None"; 

        public override IEnumerable<PoisonRoll> GetRollers()
        {
            yield return new PoisonRoll
            {
                Key = @"Damage.None",
                Roller = new ConstantRoller(0),
                Name = @"No Damage"
            };
            yield break;
        }
    }
}
