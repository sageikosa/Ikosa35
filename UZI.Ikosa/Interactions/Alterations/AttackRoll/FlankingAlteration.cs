using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Deltas;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>
    /// When used in a qualifier: flanker is the actor, and the passive flanking partner.  
    /// The attacker is in the AttackData.
    /// </summary>
    [Serializable]
    public class FlankingAlteration : AttackRollAlteration
    {
        /// <summary>
        /// When used in a qualifier: flanker is the actor, and the passive flanking partner.  
        /// The attacker is in the AttackData.
        /// </summary>
        public FlankingAlteration(AttackData attackData, int bonus, Creature flanker)
            : base(attackData, typeof(Flanking), bonus, true)
        {
            _Flanker = flanker;
        }

        private Creature _Flanker;

        public Creature Flanker => _Flanker;

        public override IEnumerable<Info> Information
        {
            get
            {
                yield return new Info { Message = $@"Flanking: {Modifier}" };
                yield break;
            }
        }
    }
}
