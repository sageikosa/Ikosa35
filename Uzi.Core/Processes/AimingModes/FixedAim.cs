using System;
using System.Collections.Generic;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    /// <summary>
    /// Used to supply a non-selectable aiming mode needed for action processing.
    /// </summary>
    [Serializable]
    public class FixedAim : AimingMode
    {
        public FixedAim(AimTarget target, string key, string displayName)
            : base(key, displayName, FixedRange.One, FixedRange.One)
        {
            _Target = target;
        }

        private AimTarget _Target;
        public AimTarget Target { get { return _Target; } }

        public override IEnumerable<AimTarget> GetTargets(CoreActor actor, CoreAction action,
            AimTargetInfo[] infos, IInteractProvider provider)
        {
            yield return Target;
            yield break;
        }

        public override AimingModeInfo ToAimingModeInfo(CoreAction action, CoreActor actor)
        {
            var _info = ToInfo<FixedAimInfo>(action, actor);
            return _info;
        }
    }
}
