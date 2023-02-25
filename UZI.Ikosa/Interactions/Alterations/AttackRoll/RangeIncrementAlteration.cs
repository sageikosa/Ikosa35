using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Interactions
{
    [Serializable, SourceInfo(@"Range Increments")]
    public class RangeIncrementAlteration : AttackRollAlteration
    {
        public RangeIncrementAlteration(RangedAttackData rangedAttack, int distance)
            : base(rangedAttack, typeof(RangeIncrementAlteration), 0 - (distance / rangedAttack.RangedSource.RangeIncrement) * 2, true)
        {
        }

        public override IEnumerable<Info> Information
        {
            get
            {
                yield return new Info { Message = string.Format(@"Range Increment: {0}", this.Modifier) };
                yield break;
            }
        }
    }
}
