using Uzi.Core;
using Uzi.Ikosa.Actions.Steps;

namespace Uzi.Ikosa.Actions
{
    public interface IApplyPowerCapable<PowerSrc> : ICapability
        where PowerSrc: IPowerActionSource
    {
        // TODO: get general modes into this...
        void ApplyPower(PowerApplyStep<PowerSrc> step);
    }
}
