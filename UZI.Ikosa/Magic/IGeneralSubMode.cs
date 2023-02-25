using System.Collections.Generic;
using Uzi.Core;

namespace Uzi.Ikosa.Magic
{
    /// <summary>
    /// General purpose spell effects that do not add adjuncts, nor cause damage
    /// </summary>
    public interface IGeneralSubMode : ICapability
    {
        IEnumerable<int> GeneralSubModes { get; }
        string GeneralSaveKey(CoreTargetingProcess targetProcess, Interaction workSet, int subMode);
        IEnumerable<StepPrerequisite> GetGeneralSubModePrerequisites(int subMode, Interaction interact);
    }
}
