using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Time
{
    /// <summary>
    /// Represents a mode that has duration, and can thus be activated and Deactivated.
    /// </summary>
    public interface IDurableCapable : ICapability
    {
        /// <summary>Enumerates SubModes, typically used for display and review</summary>
        IEnumerable<int> DurableSubModes { get; }

        /// <summary>
        /// Activates a durable mode, returning object used when deactivating
        /// </summary>
        object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource);

        /// <summary>
        /// Deactivates a durable mode, typically using the object established during activation
        /// </summary>
        void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource);

        /// <summary>Indicates whether the caster can dismiss the spell at will</summary>
        bool IsDismissable(int subMode);

        /// <summary>Spells saving value and result of success</summary>
        string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode);

        /// <summary>Rule governing how long the spell normally lasts</summary>
        DurationRule DurationRule(int subMode);

        IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact);
    }
}
