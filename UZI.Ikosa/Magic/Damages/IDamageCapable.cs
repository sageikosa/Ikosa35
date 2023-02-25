using System.Collections.Generic;
using Uzi.Core;

namespace Uzi.Ikosa.Magic
{
    public interface IDamageCapable : ICapability
    {
        /// <summary>Enumerates SubModes, typically used for display and review</summary>
        IEnumerable<int> DamageSubModes { get; }

        /// <summary>Gets damage rules for a specific subMode of a spell</summary>
        /// <param name="subMode">subMode to reference (typically 0 or higher)</param>
        /// <returns></returns>
        IEnumerable<DamageRule> GetDamageRules(int subMode, bool isCriticalHit);

        /// <summary>Get save mode for the sub mode</summary>
        /// <param name="subMode">subMode to reference (typically 0 or higher)</param>
        /// <returns></returns>
        string DamageSaveKey(Interaction workSet, int subMode);

        bool CriticalFailDamagesItems(int subMode);
    }
}
