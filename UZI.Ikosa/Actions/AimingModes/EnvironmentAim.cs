using System;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// Aim at part of the environment (such as solid or liquid material)
    /// </summary>
    [Serializable]
    public class EnvironmentAim : VolumeAim
    {
        public EnvironmentAim(string key, string displayName, Range minModes, Range maxModes, Range range, Range cubeSize)
            : base(key, displayName, minModes, maxModes, range, cubeSize)
        {
        }

        // TODO: indictate environment material types
        public override Core.Contracts.AimingModeInfo ToAimingModeInfo(CoreAction action, CoreActor actor)
        {
            return ToInfo<EnvironmentAimInfo>(action,actor);
        }
    }
}
