using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    /// <summary>
    /// <para>Return CharacterStringTargetInfo</para>
    /// <para>Left | Right</para>
    /// </summary>
    [DataContract(Namespace = Statics.Namespace)]
    public class SlideAimInfo : AimingModeInfo
    {
        [DataMember]
        public bool CanSlideLeft { get; set; }
        [DataMember]
        public bool CanSlideRight { get; set; }
    }
}
