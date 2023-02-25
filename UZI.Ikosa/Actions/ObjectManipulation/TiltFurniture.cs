using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class TiltFurniture : ActionBase
    {
        public TiltFurniture(Furnishing furnishing)
            : base(furnishing, new ActionTime(TimeType.Brief), true, false, @"200")
        {
        }

        public Furnishing Furnishing => Source as Furnishing;

        public override string Key
            => @"Furnishing.Tilt";

        public override string DisplayName(CoreActor actor)
            => @"Tilt furnishing";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override string HeadsUpMode => @"Furniture";

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _obs = ObservedActivityInfoFactory.CreateInfo(@"Tilt", activity.Actor, observer);
            _obs.Implement = GetInfoData.GetInfoFeedback(Furnishing, observer);
            return _obs;
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new TiltAim(@"Tilt", $@"Tilt {Furnishing.GetInfo(activity.Actor, false)}");
            yield break;
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            activity?.EnqueueRegisterPreEmptively(Budget);

            // TODO: may cause a cascade fall over or stop via leaning

            // perform rotation
            return new HandleInteractionStep(activity, Furnishing,
                new Interaction(activity.Actor, this, Furnishing,
                new ObjectManipulateData(activity.Actor,
                activity.GetFirstTarget<CharacterStringTarget>(@"Tilt").CharacterString)));
        }
    }
}
