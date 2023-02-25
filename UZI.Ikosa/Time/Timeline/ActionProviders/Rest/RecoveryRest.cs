using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class RecoveryRest : AdjunctGroup
    {
        public RecoveryRest()
            : base(typeof(RecoveryRest))
        {
        }

        public IEnumerable<NaturalHealing> NaturalHealers 
            => Members.OfType<NaturalHealing>().ToList();

        public IEnumerable<MentalRest> SpellSlotRechargers 
            => Members.OfType<MentalRest>().ToList();

        public override void ValidateGroup() { }
    }
}
