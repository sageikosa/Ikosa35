using System;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    #region public enum DamageType
    [DataContract(Namespace = Statics.Namespace)]
    [Flags]
    /// <summary>Can work as a a bit mask!</summary>
    public enum DamageType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Bludgeoning = 1,       // B=1
        [EnumMember]
        Piercing = 2,          // P=2
        [EnumMember]
        BludgeonAndPierce = 3, // P|B=2|1=3
        [EnumMember]
        Slashing = 4,          // S=4
        [EnumMember]
        BludgeonAndSlash = 5,  // S|B=4|1=5
        [EnumMember]
        PierceAndSlash = 6,    // S|P=4|2=6
        [EnumMember]
        All = 7                // S|P|B=4|2|1=7
    }
    #endregion
}
