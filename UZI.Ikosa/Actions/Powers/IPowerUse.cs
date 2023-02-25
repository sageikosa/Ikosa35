using Uzi.Ikosa.Actions.Steps;
using Uzi.Core;
using System;

namespace Uzi.Ikosa.Actions
{
    public interface IPowerUse<PowerActSrc> 
        where PowerActSrc : IPowerActionSource
    {
        void ActivatePower(PowerActivationStep<PowerActSrc> step);
        void ApplyPower(PowerApplyStep<PowerActSrc> step);
        ICapabilityRoot CapabilityRoot { get; }
        PowerActSrc PowerActionSource { get; }
        PowerAffectTracker PowerTracker { get; }
    }
}
