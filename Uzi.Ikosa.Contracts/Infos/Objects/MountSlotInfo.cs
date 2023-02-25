using System;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class MountSlotInfo : ItemSlotInfo
    {
        #region construction
        public MountSlotInfo()
            : base()
        {
        }
        #endregion

        [DataMember]
        public ObjectInfo MountedItem { get; set; }
        [DataMember]
        public ObjectInfo MountWrapper { get; set; }
    }
}
