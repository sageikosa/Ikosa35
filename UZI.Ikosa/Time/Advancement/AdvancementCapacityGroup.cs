using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class AdvancementCapacityGroup : AdjunctGroup
    {
        public AdvancementCapacityGroup() 
            : base(typeof(AdvancementCapacityGroup))
        {
        }

        public IEnumerable<Creature> Candidates
            => Capacities.Select(_ac => _ac.Creature);

        public IEnumerable<AdvancementCapacity> Capacities
            => Members.OfType<AdvancementCapacity>().Select(_ac => _ac);

        public override void ValidateGroup() { }
    }
}
