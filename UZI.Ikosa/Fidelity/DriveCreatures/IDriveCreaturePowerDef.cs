using Uzi.Ikosa.Magic;
using Uzi.Core;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Fidelity
{
    public interface IDriveCreaturePowerDef : ISuperNaturalPowerActionDef
    {
        /// <summary>Filter for creatures affected by the driving power</summary>
        ICreatureFilter CreatureFilter { get; }
        CoreStep CreateStep(PowerBurstCapture<SuperNaturalPowerActionSource> burst, int powerLevel, int critterPowerLevel, AimTarget target);
    }
}
