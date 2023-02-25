using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Deltas;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class CoverAlteration : AttackRollAlteration
    {
        // NOTE: since cover affects the attack roll, we calculate a negative bonus

        public CoverAlteration(AttackData attackData, int coverBonus)
            : base(attackData, typeof(Cover), 0 - coverBonus, true)
        {
        }

        public override IEnumerable<Info> Information
        {
            get
            {
                yield return new Info { Message = $@"Cover: {Modifier}" };
                yield break;
            }
        }
    }
}
