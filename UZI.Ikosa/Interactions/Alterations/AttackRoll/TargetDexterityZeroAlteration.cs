using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Deltas;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>
    /// Dexterity goes to zero, so apply a to hit bonus (equivalent to an AR penalty)
    /// </summary>
    [Serializable]
    public class TargetDexterityZeroAlteration : AttackRollAlteration
    {
        public TargetDexterityZeroAlteration(AttackData attackData, Creature target)
            : base(attackData, typeof(DexterityOverride), 5 + ((IDelta)target.MaxDexterityToARBonus).Value, false)
        {
        }

        public override IEnumerable<Info> Information
        {
            get
            {
                yield return new Info { Message = string.Format(@"Target Dex=0: {0}", this.Modifier) };
                yield break;
            }
        }
    }
}
