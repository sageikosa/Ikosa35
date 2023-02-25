using Uzi.Core.Contracts;
using System;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions.Action;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class StoreObjectStep : CoreStep
    {
        #region construction
        public StoreObjectStep(CoreActivity activity, IObjectContainer repository, HoldingSlot slot)
            : base(activity)
        {
            _Repository = repository;
            _Slot = slot;
        }

        public StoreObjectStep(CoreStep predecessor, IObjectContainer repository, HoldingSlot slot)
            : base(predecessor)
        {
            _Repository = repository;
            _Slot = slot;
        }
        #endregion

        #region private data
        private HoldingSlot _Slot;
        private IObjectContainer _Repository;
        #endregion

        public HoldingSlot HoldingSlot { get { return _Slot; } }
        public IObjectContainer Repository { get { return _Repository; } }
        public CoreActor Actor { get { return (Process as CoreActivity).Actor; } }

        protected override bool OnDoStep()
        {
            var _activity = Process as CoreActivity;
            var _obj = HoldingSlot.SlottedItem.BaseObject;
            if (_activity.Actor is Creature _critter
                && !ManipulateTouch.CanManipulateTouch(_critter, Repository))
            {
                AppendFollowing(_activity?.GetActivityResultNotifyStep(@"Cannot touch container"));
            }
            else
            {
                if (Repository.CanHold(_obj))
                {
                    // first, stop holding it...
                    HoldingSlot.SlottedItem.ClearSlots();
                    if (HoldingSlot.SlottedItem == null)
                    {
                        // got this far, now try to remove from direct object load
                        Actor.ObjectLoad.Remove(_obj, Repository);

                        // now, add to the repository
                        // TODO: need a store interaction for the repository?
                        Repository.Add(_obj);

                        EnqueueNotify(new RefreshNotify(true, false, true, true, true), Actor.ID);
                    }
                    else
                    {
                        AppendFollowing(_activity?.GetActivityResultNotifyStep(@"Cannot release"));
                    }
                }
                else
                {
                    AppendFollowing(_activity?.GetActivityResultNotifyStep(@"Container cannot hold"));
                }
            }

            // done
            return true;
        }

        protected override StepPrerequisite OnNextPrerequisite() { return null; }
        public override bool IsDispensingPrerequisites { get { return false; } }
    }
}
