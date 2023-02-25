using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// Load from the environment [ActionBase (Brief)]
    /// </summary>
    [Serializable]
    public class LoadObject : ActionBase
    {
        /// <summary>
        /// Load from the environment [ActionBase (Brief)]
        /// </summary>
        public LoadObject(IObjectContainer repository, string orderKey)
            : base(repository, new ActionTime(TimeType.Brief), true, false, orderKey)

        {
        }

        public IObjectContainer Repository => (IObjectContainer)Source;

        public override string Key => @"Inventory.LoadObject";
        public override string DisplayName(CoreActor actor) => @"Load object into a container";

        public override bool IsStackBase(CoreActivity activity)
        {
            return false;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _obs = ObservedActivityInfoFactory.CreateInfo(@"Load", activity.Actor, observer, activity.Targets[0].Target as CoreObject);
            _obs.Implement = GetInfoData.GetInfoFeedback(Repository as CoreObject, observer);
            return _obs;
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            if ((activity.Targets[0].Target is ICoreObject _target)
                && (activity.Actor is Creature _critter))
            {
                // Need at least 1 free holding slot...
                var _holding = _critter.Body.ItemSlots.AllSlots
                    .OfType<HoldingSlot>().FirstOrDefault(_is => _is.SlottedItem == null);
                if (_holding != null)
                {
                    activity.EnqueueRegisterPreEmptively(Budget);
                    var _pickUp = new PickUpObjectStep(activity, _holding, _target);
                    new StoreObjectStep(_pickUp, Repository, _holding);
                    return _pickUp;
                }
                else
                {
                    return activity.GetActivityResultNotifyStep(@"No free holding slot");
                }
            }
            return activity.GetActivityResultNotifyStep(@"Nothing selected");
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            // TODO: object list, objects in Melee Range (excluding the Repository itself...)
            yield return new AwarenessAim(@"Object", @"Object to load", FixedRange.One, FixedRange.One, new MeleeRange(), new ObjectTargetType());
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}
