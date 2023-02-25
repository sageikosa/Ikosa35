using System;
using System.Runtime.Serialization;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public enum EncumberanceVal : byte
    {
        /// <summary>Creature is below the light load limit and wearing nothing heavier than light armor.</summary>
        [EnumMember]
        Unencumbered,

        /// <summary>Creature is in the medium load range or wearing medium armor.</summary>
        [EnumMember]
        Encumbered,

        /// <summary>Creature is heavily loaded or wearing heavy armor.</summary>
        [EnumMember]
        GreatlyEncumbered,

        /// <summary>You're carrying too much!</summary>
        [EnumMember]
        Overloaded
    }
}
