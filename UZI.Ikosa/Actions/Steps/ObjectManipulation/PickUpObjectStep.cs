using Uzi.Core.Contracts;
using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class PickUpObjectStep : CoreStep
    {
        #region construction
        public PickUpObjectStep(CoreActivity activity, HoldingSlot slot, ICoreObject coreObject)
            : base(activity)
        {
            _Slot = slot;
            _Obj = coreObject;
        }

        public PickUpObjectStep(CoreStep predecessor, HoldingSlot slot, ICoreObject coreObject)
            : base(predecessor)
        {
            _Slot = slot;
            _Obj = coreObject;
        }
        #endregion

        #region data
        private HoldingSlot _Slot;
        private ICoreObject _Obj;
        #endregion

        public HoldingSlot HoldingSlot => _Slot;
        public ICoreObject ICoreObject => _Obj;
        public CoreActivity Activity => Process as CoreActivity;

        protected override bool OnDoStep()
        {
            // if cannot lift off the ground, then cannot perform
            var _critter = HoldingSlot.Creature;

            if (ManipulateTouch.CanManipulateTouch(_critter, ICoreObject))
            {
                if (_critter.ObjectLoad.CanAdd(ICoreObject))
                {
                    var _pickUpData = new PickUp(_critter);
                    var _interact = new Interaction(_critter, HoldingSlot, ICoreObject, _pickUpData);
                    ICoreObject.HandleInteraction(_interact);

                    var _info = GetInfoData.GetInfoFeedback(ICoreObject, _critter);
                    if (_interact.Feedback.OfType<PickUpFeedback>().Any())
                    {
                        #region picked up
                        if (_info != null)
                        {
                            EnqueueNotify(new RefreshNotify(true, false, true, true, false), Activity.Actor.ID);
                            //new RefreshNotify { Message = @"Picked up", Items = true, Creature = true }, 
                            //_info);

                        }
                        else
                        {
                            EnqueueNotify(new RefreshNotify(true, false, true, true, false), Activity.Actor.ID);
                            //new RefreshNotify { Message = @"Picked up", Items = true, Creature = true });
                        }
                        #endregion
                    }
                    else
                    {
                        #region did not pick up
                        if (_info != null)
                        {
                            Activity?.Terminate(new Description(@"Picked up", @"Could not pick up"), _info);
                        }
                        else
                        {
                            Activity?.Terminate(@"Could not pick up");
                        }
                        #endregion
                    }
                }
                else
                {
                    Process.IsActive = false;
                    AppendFollowing(Activity.GetActivityResultNotifyStep(@"Too heavy"));
                }
            }
            else
            {
                Process.IsActive = false;
                AppendFollowing(Activity.GetActivityResultNotifyStep(@"Cannot touch"));
            }

            return true;
        }

        protected override StepPrerequisite OnNextPrerequisite() => null;
        public override bool IsDispensingPrerequisites => false;
    }
}
