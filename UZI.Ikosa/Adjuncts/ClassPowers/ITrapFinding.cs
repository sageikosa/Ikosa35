using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    public interface ITrapFinding
    {
        /// <summary>Note: still need to match or beat difficulty</summary>
        bool CanFindTrap(ICoreObject coreObj);

        /// <summary>Note: still need to match or beat difficulty</summary>
        bool CanDisableTrap(ICoreObject coreObj);
    }
}