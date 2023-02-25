using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.Namespace)]
    public class LocalCellGroupInfo : CubicInfo
    {
        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public string CellGroupName { get; set; }
        [DataMember]
        public bool IsDeepShadows { get; set; }
        [DataMember]
        public FreshnessTime Freshness { get; set; }
        [DataMember]
        public List<Guid> TouchingRooms { get; set; }
    }
}