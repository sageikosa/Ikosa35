using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class UnconsciousnessPoisonDamage : PoisonDamage
    {
        public UnconsciousnessPoisonDamage(Roller timeAmount, TimeUnit timeUnit)
            : base()
        {
            TimeAmount = timeAmount;
            TimeUnit = timeUnit;
        }

        public Roller TimeAmount { get; private set; }
        public TimeUnit TimeUnit { get; private set; }
        public override string Name => $@"Unconsciousness for {TimeAmount.ToString()} {TimeUnit.PluralName}";

        public override IEnumerable<DamageInfo> ApplyDamage(object source, CoreStep step, Creature critter)
        {
            var _timeVal = TimeAmount.RollValue(critter.ID, @"Unconscious (Poison)", TimeUnit.PluralName);
            return ApplyDamage(source, step, critter, new int[] { _timeVal });
        }

        public override IEnumerable<DamageInfo> ApplyDamage(object source, CoreStep step, Creature critter, int[] rollValue)
        {
            var _amount = rollValue[0];
            var _uEffect =
                new UnconsciousEffect(source, critter.GetLocated().Locator.Map.CurrentTime + (TimeUnit.BaseUnitFactor * _amount), TimeUnit.BaseUnitFactor);
            critter.AddAdjunct(_uEffect);
            yield return new ConditionDamageInfo
            {
                Amount = _amount,
                Condition = Condition.Unconscious,
                TimeUnit = TimeUnit.ValueName(_amount)
            };
            yield break;
        }

        public override IEnumerable<PoisonRoll> GetRollers()
        {
            yield return new PoisonRoll
            {
                Key = @"Unconscious.Time",
                Roller = TimeAmount,
                Name = $@"Unconsciousness in {TimeUnit.PluralName}"
            };
            yield break;
        }
    }
}
