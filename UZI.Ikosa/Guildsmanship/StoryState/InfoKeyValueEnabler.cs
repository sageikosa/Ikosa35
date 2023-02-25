using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class InfoKeyValueEnabler : ValueEnabler
    {
        private Guid _InfoID;

        public InfoKeyValueEnabler(Description description, Guid id)
            : base(description)
        {
            _InfoID = id;
        }

        public Guid InfoKeyID { get => _InfoID; set => _InfoID = value; }

        public override bool Enablesvalue(TeamTracker tracker, IEnumerable<Creature> creatures)
            => tracker.HasInfoKey(InfoKeyID, creatures);
    }
}
