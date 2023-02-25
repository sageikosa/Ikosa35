using System;

namespace Uzi.Core
{
    public interface IAlterInteraction : IControlChange<Activation>
    {
        bool CanAlterInteraction(Interaction workSet);
        bool WillDestroyInteraction(Interaction workSet, ITacticalContext context);
        bool IsActive { get; }
    }
}
