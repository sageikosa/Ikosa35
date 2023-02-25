using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Actions.Step
{
    [Serializable]
    public class RemoveLidStep : CoreStep
    {
        public RemoveLidStep(CoreActivity activity, HollowFurnishingLid lid)
            : base(activity)
        {
            _Lid = lid;
        }

        #region data
        private HollowFurnishingLid _Lid;
        #endregion

        public HollowFurnishingLid HollowFurnishingLid => _Lid;

        public override bool IsDispensingPrerequisites => false;
        protected override StepPrerequisite OnNextPrerequisite() => null;

        protected override bool OnDoStep()
        {
            if (Process is CoreActivity _activity
                && _activity.Actor is Creature _critter)
            {
                if (ManipulateTouch.CanManipulateTouch(_critter, HollowFurnishingLid))
                {
                    // if connected to a hollow furnishing
                    var _furnish = HollowFurnishingLid?.HollowFurnishing;
                    if (_furnish != null)
                    {
                        // unconnect it
                        HollowFurnishingLid.UnbindFromObject(_furnish);
                        if (!HollowFurnishingLid.IsLidding)
                        {
                            // and drop it in environment
                            Drop.DoDropEject(_furnish, HollowFurnishingLid);
                        }
                    }
                }
                else
                {
                    _activity.Terminate(new Info { Message = @"Unable to touch" });
                }
            }
            return true;
        }
    }
}
