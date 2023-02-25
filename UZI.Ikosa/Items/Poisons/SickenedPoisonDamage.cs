using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class SickenedPoisonDamage : PoisonDamage
    {
        public SickenedPoisonDamage(Roller timeAmount, TimeUnit timeUnit)
            : base()
        {
            _TimeAmount = timeAmount;
            _TimeUnit = timeUnit;
        }

        #region data
        private Roller _TimeAmount;
        private TimeUnit _TimeUnit;
        #endregion

        public Roller TimeAmount => _TimeAmount;
        public TimeUnit TimeUnit => _TimeUnit;

        public override string Name => $@"Sickened for {TimeAmount.ToString()} {TimeUnit.PluralName}";

        public override IEnumerable<DamageInfo> ApplyDamage(object source, CoreStep step, Creature critter)
        {
            var _timeVal = TimeAmount.RollValue(critter.ID, @"Sickened (Poison)", TimeUnit.PluralName);
            return ApplyDamage(source, step, critter, new int[] { _timeVal });
        }

        public override IEnumerable<DamageInfo> ApplyDamage(object source, CoreStep step, Creature critter, int[] rollValue)
        {
            var _amount = rollValue[0];
            var _sickened = new Sickened(source);
            var _endTime = critter.GetLocated().Locator.Map.CurrentTime + (TimeUnit.BaseUnitFactor * _amount);
            var _expiry = new Expiry(_sickened, _endTime, TimeValTransition.Leaving, Minute.UnitFactor);
            critter.AddAdjunct(_expiry);
            yield return new ConditionDamageInfo
            {
                Amount = _amount,
                Condition = Condition.Sickened,
                TimeUnit = TimeUnit.ValueName(_amount)
            };
            yield break;
        }

        public override IEnumerable<PoisonRoll> GetRollers()
        {
            yield return new PoisonRoll
            {
                Key = @"Sickened.Time",
                Roller = TimeAmount,
                Name = $@"Sickened in {TimeUnit.PluralName}"
            };
            yield break;
        }
    }
}
