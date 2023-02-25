using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Movement;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// Keyed target expected to be AnchorVectorTarget.
    /// </summary>
    [Serializable]
    public class MovementAim : AimingMode
    {
        /// <summary>
        /// Keyed target expected to be AnchorVectorTarget.
        /// </summary>
        public MovementAim(string key, string displayName)
            : base(key, displayName, FixedRange.One, FixedRange.One)
        {
        }

        public override IEnumerable<AimTarget> GetTargets(CoreActor actor, CoreAction action,
            AimTargetInfo[] infos, IInteractProvider provider)
        {
            // regular single step headings
            var _any = false;
            foreach (var _hdng in SelectedTargets<HeadingTargetInfo>(actor, action, infos, false))
            {
                yield return new HeadingTarget(this.Key, _hdng.Heading, _hdng.UpDownAdjust);
                _any = true;
            }

            // no duplication of destination aim targets
            if (!_any)
            {
                // multi-step destinations
                foreach (var _multi in SelectedTargets<MultiStepDestinationInfo>(actor, action, infos))
                    yield return new MultiStepDestinationTarget(this.Key, _multi.Offset);
            }

            // step index variable
            yield return new ValueTarget<int>(MovementTargets.StepIndex, 0);
            yield break;
        }

        public override AimingModeInfo ToAimingModeInfo(CoreAction action, CoreActor actor)
        {
            var _info = ToInfo<MovementAimInfo>(action, actor);
            return _info;
        }
    }
}
