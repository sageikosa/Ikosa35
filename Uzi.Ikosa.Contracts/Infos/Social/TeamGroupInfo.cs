using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class TeamGroupInfo
    {
        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public CreatureTrackerInfo[] PrimaryCreatures { get; set; }
        [DataMember]
        public CreatureTrackerInfo[] AssociateCreatures { get; set; }

        public CreatureTrackerInfo FindCreature(Guid id)
            => PrimaryCreatures?.FirstOrDefault(_cti => _cti.ID == id);
    }
}
