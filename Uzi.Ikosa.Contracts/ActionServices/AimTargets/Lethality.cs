using System;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    /// <summary>NormallyLethal | AlwaysLethal | AlwaysNonLethal | NormallyNonLethal</summary>
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public enum Lethality
    {
        [EnumMember]
        NormallyLethal,
        [EnumMember]
        AlwaysLethal,
        [EnumMember]
        AlwaysNonLethal,
        [EnumMember]
        NormallyNonLethal
    }
}
