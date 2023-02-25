using System;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    /// <summary>Attack Impact: Touch, Incorporeal or Penetrating</summary>
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public enum AttackImpact
    {
        [EnumMember]
        Touch,
        [EnumMember]
        Incorporeal,
        [EnumMember]
        Penetrating
    };

}
