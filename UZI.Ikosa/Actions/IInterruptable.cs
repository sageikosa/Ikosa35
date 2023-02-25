using Uzi.Core;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Action may be interruptable</summary>
    public interface IInterruptable
    {
        /// <summary>Signal to action that it was interrupted.  May cause loss of resources being used.</summary>
        void Interrupted();
    }

    /// <summary>Any damage will interrupt the action</summary>
    public interface IDamageInterrupts : IInterruptable
    {
    }

    /// <summary>Damage or distractions will interrupt the action on concentration check failure</summary>
    public interface IDistractable : IInterruptable
    {
        Deltable ConcentrationBase { get; }
    }
}
