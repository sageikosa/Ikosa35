using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class ItemValueEnabler : ValueEnabler
    {
        private readonly HashSet<Guid> _IDs;

        public ItemValueEnabler(Description description)
            : base(description)
        {
            _IDs = new HashSet<Guid>();
        }

        public HashSet<Guid> ItemIDs => _IDs;

        public override bool Enablesvalue(TeamTracker tracker, IEnumerable<Creature> creatures)
            => creatures.Any(_c => _IDs.Any(_id => _c.IkosaPosessions.Contains(_id)));
    }
}
