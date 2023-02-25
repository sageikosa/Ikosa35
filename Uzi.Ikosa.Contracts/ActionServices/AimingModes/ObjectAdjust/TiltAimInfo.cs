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
    /// <para>Push | Pull | Clock | CounterClock</para>
    /// </summary>
    [DataContract(Namespace = Statics.Namespace)]
    public class TiltAimInfo : AimingModeInfo
    {
    }
}
