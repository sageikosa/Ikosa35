using System;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// Aim mode indicating the need to make a touch or ranged touch [AimingMode]
    /// </summary>
    [Serializable]
    public class TouchAim : AttackAim
    {
        #region Construction
        public TouchAim(string key, string displayName, Lethality lethality, int criticalStart, IRangedSourceProvider provider, 
            Range minModes, Range maxModes, Range range, params ITargetType [] validTargetType)
            : base(key, displayName, AttackImpact.Touch, lethality, true, criticalStart, provider, minModes, maxModes, range, validTargetType)
        {
        }
        #endregion
    }
}
