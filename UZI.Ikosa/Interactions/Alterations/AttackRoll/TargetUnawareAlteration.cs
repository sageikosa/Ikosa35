using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Deltas;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>
    /// use when the target is unaware of the attacker (target blinded or attacker invisible)
    /// </summary>
    [Serializable]
    public class TargetUnawareAlteration : AttackRollAlteration
    {
        /// <summary>
        /// +2 to hit if the defender is unaware of the attacker
        /// </summary>
        public TargetUnawareAlteration(AttackData attackData)
            : base(attackData, typeof(Unaware), 2, false)
        {
        }

        public override IEnumerable<Info> Information
        {
            get
            {
                yield return new Info { Message = string.Format(@"Target Unaware: {0}", this.Modifier) };
                yield break;
            }
        }
    }
}
