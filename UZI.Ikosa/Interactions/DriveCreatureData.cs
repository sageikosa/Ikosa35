using System;
using Uzi.Core;
using Uzi.Ikosa.Fidelity;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>Creature is the actor performing the driving</summary>
    [Serializable]
    public class DriveCreatureData : InteractData
    {
        /// <summary>Creature is the actor performing the driving</summary>
        public DriveCreatureData(Creature critter, IDriveCreaturePowerDef ability)
            : base(critter)
        {
            DriveCreatureAbility = ability;
        }

        public IDriveCreaturePowerDef DriveCreatureAbility { get; private set; }
    }
}
