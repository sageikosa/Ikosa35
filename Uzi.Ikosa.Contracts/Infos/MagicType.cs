using System;
using System.Runtime.Serialization;
namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    /// <summary>Arcane or Divine</summary>
    public enum MagicType
    {
        [EnumMember]
        Divine,
        [EnumMember]
        Arcane
    }
}