using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class FlatObjectManipulation : ActionBase
    {
        public FlatObjectManipulation(FlatPanel flatPanel, ActionTime time,
            string orderKey, string verb, bool flex)
            : base(flatPanel, time, true, false, orderKey)
        {
            _Verb = verb;
            _Flex = flex;
        }

        #region state
        protected bool _Flex;
        protected string _Verb;
        #endregion

        public FlatPanel FlatPanel => Source as FlatPanel;

        public override string Key
            => $@"FlatObject.{_Verb}";

        public override string DisplayName(CoreActor actor)
            => $@"{_Verb} flat object";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override bool WillClearStack(CoreActionBudget budget, CoreActivity activity)
            => true;

        public override string HeadsUpMode => _Flex ? @"FlexibleFlatObject" : @"FlatObject";

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _obs = ObservedActivityInfoFactory.CreateInfo(_Verb, activity.Actor, observer);
            _obs.Implement = GetInfoData.GetInfoFeedback(FlatPanel, observer);
            return _obs;
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield break;
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            activity?.EnqueueRegisterPreEmptively(Budget);
            return new HandleInteractionStep(activity, FlatPanel,
                new Interaction(activity.Actor, this, FlatPanel,
                new ObjectManipulateData(activity.Actor, _Verb)));
        }
    }
}
