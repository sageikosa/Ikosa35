using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// Aimed to create a wall-like effect
    /// </summary>
    [Serializable]
    public class WallAim : RangedAim
    {
        // TODO: wall maximum extents (max length, max height, max area [interrelations])
        // TODO: granularity (minimum wall units) and flexibility (circular/spherical)
        // TODO: wall side-polarity (anchor face...)
        public WallAim(string key, string displayName, Range minModes, Range maxModes, Range range)
            : base(key, displayName, minModes, maxModes, range)
        {
        }

        public override IEnumerable<AimTarget> GetTargets(CoreActor actor, CoreAction action, AimTargetInfo[] infos, IInteractProvider provider)
        {
            // must be within max range
            var _max = Range.EffectiveRange(actor, action.CoreActionClassLevel(actor, this));

            throw new NotImplementedException();
        }

        public override AimingModeInfo ToAimingModeInfo(CoreAction action, CoreActor actor)
        {
            throw new NotImplementedException();
        }
    }
}
