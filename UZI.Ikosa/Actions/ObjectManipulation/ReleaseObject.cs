using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class ReleaseObject : ActionBase
    {
        public ReleaseObject(ICoreObject coreObject, string orderKey)
            : base(coreObject as IActionSource, new ActionTime(TimeType.Free), false, false, orderKey)
        {
        }

        public CoreObject CoreObject => Source as CoreObject;

        public override string HeadsUpMode => @"*";

        public override string Key
            => @"CoreObject.Release";

        public override string DisplayName(CoreActor actor)
            => @"Release from grab";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _obs = ObservedActivityInfoFactory.CreateInfo(@"Grab", activity.Actor, observer);
            _obs.Implement = GetInfoData.GetInfoFeedback(CoreObject, observer);
            return _obs;
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield break;
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            var _gTarget = CoreObject.Adjuncts.OfType<ObjectGrabbed>().FirstOrDefault();
            if (_gTarget != null)
            {
                activity?.Actor.Adjuncts.OfType<ObjectGrabber>()
                    .FirstOrDefault(_g => _g.ObjectGrabGroup == _gTarget.ObjectGrabGroup)
                    ?.Eject();
                return activity.GetActivityResultNotifyStep(@"No longer grabbing");
            }
            return null;
        }
    }
}
