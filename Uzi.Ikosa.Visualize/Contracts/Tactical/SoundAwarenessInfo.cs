using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.Namespace)]
    public class SoundAwarenessInfo
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public Vector3D Vector { get; set; }

        [DataMember]
        public List<SoundInfo> Stream { get; set; }

        [DataMember]
        public double Magnitude { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public double Range { get; set; }

        [DataMember]
        public double TimeFade { get; set; }

    }
}
