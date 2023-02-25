using System;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    /// <summary>Light, medium or heavy</summary>
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public enum ArmorProficiencyType
    {
        [EnumMember]
        None = -1,
        [EnumMember]
        Light,
        [EnumMember]
        Medium,
        [EnumMember]
        Heavy
    }
}
