using System;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    /// <summary>Used to signal in SysStatus that the receiver should refresh client information</summary>
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class RefreshNotify : SysNotify
    {
        public RefreshNotify(bool creature, bool sensorHost, bool awareness, bool items, bool actions)
            : base(null)
        {
            RefreshFlags =
                (creature ? RefreshFlags.Creature : RefreshFlags.None) |
                (sensorHost ? RefreshFlags.SensorHost : RefreshFlags.None) |
                (items ? RefreshFlags.Items : RefreshFlags.None) |
                (actions ? RefreshFlags.Actions : RefreshFlags.None) |
                (awareness ? RefreshFlags.Awarenesses : RefreshFlags.None);

        }

        [DataMember]
        public RefreshFlags RefreshFlags { get; set; }
    }
}
