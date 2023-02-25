using System;
using Uzi.Core;
using System.Collections.ObjectModel;

namespace Uzi.Ikosa
{
    /// <summary>
    /// Used by objects that can be disabled (must provide a DisableDifficulty, and will usually handle DisableMechanismInteraction
    /// </summary>
    public interface IDisableable : IActivatableObject, IControlChange<DisableFail>, IActionSource
    {
        ActionTime ActionTime { get; }

        /// <summary>Reported time cost (rather than actual time cost)</summary>
        string TimeCostString { get; }

        /// <summary>When failure to disable occurs, this is called</summary>
        void FailedDisable(CoreActivity activity);

        /// <summary>Difficulty for disabling</summary>
        Deltable DisableDifficulty { get; }

        /// <summary>List of those who believe the mechanism is disabled</summary>
        Collection<Guid> ConfusedDisablers { get; }

        /// <summary>Typically looks for any DisabledObject adjuncts</summary>
        bool IsDisabled { get; }
    }

    [Serializable]
    public struct DisableFail
    {
    }
}
