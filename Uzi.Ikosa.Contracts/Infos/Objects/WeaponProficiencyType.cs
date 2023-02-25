using System;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    /// <summary>Natural, Simple, Martial, Exotic, NonProficient</summary>
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public enum WeaponProficiencyType
    {
        [EnumMember]
        Natural,
        [EnumMember]
        Simple,
        [EnumMember]
        Martial,
        [EnumMember]
        Exotic,
        [EnumMember]
        NonProficient
    }
}