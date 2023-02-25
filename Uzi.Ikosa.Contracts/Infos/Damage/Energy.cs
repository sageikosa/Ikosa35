using System;
using System.Runtime.Serialization;
namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    [Serializable]
    public enum EnergyType
    {
        [EnumMember]
        Acid,
        [EnumMember]
        Cold,
        [EnumMember]
        Electric,
        [EnumMember]
        Fire,
        [EnumMember]
        Sonic,
        [EnumMember]
        Positive,
        [EnumMember]
        Negative,
        [EnumMember]
        Force,
        [EnumMember]
        Unresistable
    }
}