using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Deltas;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>Attacker gets bonus for attacking the top of the target</summary>
    [Serializable]
    public class HigherGroundAlteration : AttackRollAlteration
    {
        public HigherGroundAlteration(AttackData attackData, int bonus)
            : base(attackData, typeof(HigherGround), bonus, true)
        {
        }

        public override IEnumerable<Info> Information
        {
            get
            {
                yield return new Info { Message = string.Format(@"Higher Ground: {0}", this.Modifier) };
                yield break;
            }
        }
    }
}