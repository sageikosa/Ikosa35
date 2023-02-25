using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions.Action;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class ExtractNaturalItem : ActionBase
    {
        public ExtractNaturalItem(NaturalItemTrait itemTrait, ActionTime extractTime, string orderKey)
            : base(itemTrait, extractTime, true, true, orderKey)
        {
        }

        public NaturalItemTrait NaturalItemTrait => ActionSource as NaturalItemTrait;
        public override string Key => @"NaturalItem.Extract";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override bool WillClearStack(CoreActionBudget budget, CoreActivity activity)
            => (TimeCost.ActionTimeType == TimeType.Free)
            ? false
            : base.WillClearStack(budget, activity);

        public override string DisplayName(CoreActor actor)
            => $@"Extract {NaturalItemTrait.Item.GetKnownName(actor)}";

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory
                .CreateInfo(@"Extracting Item", activity.Actor, observer, NaturalItemTrait.Item);

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => ProvokesTarget && (NaturalItemTrait.Creature == potentialTarget);

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield break;
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            activity.EnqueueRegisterPreEmptively(Budget);
            return new ExtractNaturalItemStep(activity, this);
        }
    }

    [Serializable]
    public class ExtractNaturalItemStep : CoreStep
    {
        public ExtractNaturalItemStep(CoreActivity activity, ExtractNaturalItem extract)
            : base(activity)
        {
            _Extract = extract;
        }

        #region state
        private ExtractNaturalItem _Extract;
        #endregion

        public CoreActivity Activity => Process as CoreActivity;
        public override bool IsDispensingPrerequisites => false;
        protected override StepPrerequisite OnNextPrerequisite() => null;
        public override bool IsNewRoot => false;

        protected override bool OnDoStep()
        {
            if (Activity.Actor is Creature _critter)
            {
                if (ManipulateTouch.CanManipulateTouch(_critter, _Extract.NaturalItemTrait.Item))
                {
                    var _extract = new ExtractNaturalItemData(Activity.Actor, _Extract.NaturalItemTrait);
                    var _workSet = new Interaction(Activity.Actor, this, _Extract.NaturalItemTrait.Item, _extract);
                    _Extract.NaturalItemTrait.Item.HandleInteraction(_workSet);
                }
                else
                {
                    AppendFollowing(Activity.GetActivityResultNotifyStep(@"Cannot touch"));
                }
            }
            return true;
        }
    }
}
