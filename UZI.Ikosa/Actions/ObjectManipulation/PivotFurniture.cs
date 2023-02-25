using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class PivotFurniture : ActionBase
    {
        public PivotFurniture(Furnishing furnishing, ActionTime time)
            : base(furnishing, time, true, false, @"201")
        {
            if (time.ActionTimeType == TimeType.Brief)
            {
                _TurnsLeft = ObjectGrabbedPivotData.GetPivots(furnishing);
            }
            else
            {
                _TurnsLeft = 0;
            }
        }

        #region data
        private int _TurnsLeft;
        #endregion

        public Furnishing Furnishing => Source as Furnishing;
        public int TurnsLeft { get => _TurnsLeft; set => _TurnsLeft = value; }

        public override string Key
            => @"Furnishing.Pivot";

        public override string DisplayName(CoreActor actor)
            => @"Pivot furnishing";

        public override bool IsStackBase(CoreActivity activity)
            => (TimeCost.ActionTimeType != TimeType.SubAction) && (_TurnsLeft > 0);

        public override bool WillClearStack(CoreActionBudget budget, CoreActivity activity)
            => TimeCost.ActionTimeType != TimeType.SubAction;

        public override string HeadsUpMode => @"Furniture";

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _obs = ObservedActivityInfoFactory.CreateInfo(@"Pivot", activity.Actor, observer);
            _obs.Implement = GetInfoData.GetInfoFeedback(Furnishing, observer);
            return _obs;
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new RotateAim(@"Pivot", $@"Pivot {Furnishing.GetInfo(activity.Actor, false)}");
            yield break;
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            // decrease turns left
            var _pivot = ((Budget?.TopActivity?.Action as PivotFurniture) ?? this);
            _pivot.TurnsLeft--;

            activity?.EnqueueRegisterPreEmptively(Budget);

            // TODO: may cause a cascade fall over or stop via leaning

            // perform rotation
            return new HandleInteractionStep(activity, Furnishing,
                new Interaction(activity.Actor, this, Furnishing,
                new ObjectManipulateData(activity.Actor,
                activity.GetFirstTarget<CharacterStringTarget>(@"Pivot").CharacterString)));
        }
    }
}
