using System;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Implies no target type, therefore target geometry only</summary>
    [Serializable]
    public class CellAttackAim : AttackAim
    {
        /// <summary>Implies no target type, therefore target geometry only</summary>
        public CellAttackAim(string key, string displayName, Lethality lethality, int criticalStart, IRangedSourceProvider provider, 
            Range minModes, Range maxModes, Range range)
            : base(key, displayName, AttackImpact.Touch, lethality, false, criticalStart, provider, minModes, maxModes, range, null)
        {
        }
    }
}
