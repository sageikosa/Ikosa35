using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Dice;
using Uzi.Ikosa.Interactions;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class HealthPointPoisonDamage : PoisonDamage
    {
        public HealthPointPoisonDamage(Roller damageAmount, bool nonLethal) :
            base()
        {
            DamageAmount = damageAmount;
            NonLethal = nonLethal;
        }

        public Roller DamageAmount { get; private set; }
        public override string Name => DamageAmount.ToString();
        public bool NonLethal { get; private set; }

        public override IEnumerable<DamageInfo> ApplyDamage(object source, CoreStep step, Creature critter)
            => ApplyDamage(source, step, critter, new int[] { DamageAmount.RollValue(critter.ID, @"Damage (Poison)", NonLethal ? @"Non-lethal" : @"Lethal") });

        public override IEnumerable<DamageInfo> ApplyDamage(object source, CoreStep step, Creature critter, int[] rollValue)
        {
            var _amount = rollValue[0];
            var _damage = new DamageData(_amount, NonLethal, @"Poison", 0);
            var _deliver = new DeliverDamageData(null, _damage.ToEnumerable(), true, false);
            var _dmgInteract = new StepInteraction(step, null, source, critter, _deliver);
            critter.HandleInteraction(_dmgInteract);
            if (_dmgInteract.Feedback.OfType<PrerequisiteFeedback>().Any())
            {
                new RetryInteractionStep(step, @"Retry", _dmgInteract);
            }
            return (null as DamageInfo).ToEnumerable();
        }

        public override IEnumerable<PoisonRoll> GetRollers()
        {
            yield return new PoisonRoll
            {
                Key = @"HealthPoint.Damage",
                Roller = DamageAmount,
                Name = @"HP Damage"
            };
            yield break;
        }
    }
}