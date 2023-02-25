using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class TargetConditionAlteration : AttackRollAlteration
    {
        public TargetConditionAlteration(AttackData attackData, object conditionName, int modifier, bool informAttacker)
            : base(attackData, conditionName, modifier, informAttacker)
        {
        }
        public string ConditionName => (string)Source;

        public override IEnumerable<Info> Information
        {
            get
            {
                yield return new Info { Message = string.Format(@"Target Condition[{1}]: {0}", this.Modifier, this.ConditionName) };
                yield break;
            }
        }
    }
}
