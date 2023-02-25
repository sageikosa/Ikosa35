using System;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Advancement
{
    /// <summary>Power Source for Spells, Super Natural Powers and Extraordinary Abilities</summary>
    public interface IPowerClass : IControlChange<Activation>, IQualifyDelta
    {
        string ClassName { get; }

        string ClassIconKey { get; }

        /// <summary>Power level checks and level-dependent calculations</summary>
        IVolatileValue ClassPowerLevel { get; }

        /// <summary>Allows dismissable spells to be validated by the caster</summary>
        Guid OwnerID { get; }

        string Key { get; }

        /// <summary>Indicates that the class powers are active</summary>
        bool IsPowerClassActive { get; set; }

        /// <summary>Provides the info for the power class</summary>
        PowerClassInfo ToPowerClassInfo();
    }
}
