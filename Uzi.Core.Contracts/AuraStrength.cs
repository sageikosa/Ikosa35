using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Uzi.Core.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public enum AuraStrength
    {
        [EnumMember]
        /// <summary>Useful for detection results</summary>
        None,
        [EnumMember]
        /// <summary>Lingering</summary>
        Dim,
        [EnumMember]
        Faint,
        [EnumMember]
        Moderate,
        [EnumMember]
        Strong,
        [EnumMember]
        Overwhelming
    }
}
