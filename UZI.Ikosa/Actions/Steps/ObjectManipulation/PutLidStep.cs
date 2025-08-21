using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class PutLidStep : CoreStep
    {
        public PutLidStep(CoreActivity activity, HollowFurnishingLid lid, HollowFurnishing furnishing)
            : base(activity)
        {
            _Lid = lid;
            _Furnishing = furnishing;
        }

        #region data
        private HollowFurnishingLid _Lid;
        private HollowFurnishing _Furnishing;
        #endregion

        public HollowFurnishingLid HollowFurnishingLid => _Lid;
        public HollowFurnishing HollowFurnishing => _Furnishing;

        public override bool IsDispensingPrerequisites => false;
        protected override StepPrerequisite OnNextPrerequisite() => null;

        protected override bool OnDoStep()
        {
            if (!(HollowFurnishingLid?.IsLidding ?? true)
                && (HollowFurnishing != null))
            {
                if (Process is CoreActivity _activity
                    && _activity.Actor is Creature _critter)
                {
                    if (ManipulateTouch.CanManipulateTouch(_critter, HollowFurnishingLid)
                        && ManipulateTouch.CanManipulateTouch(_critter, HollowFurnishing))
                    {
                        // not already holding...
                        if (!_critter.ObjectLoad.Contains(HollowFurnishingLid)
                            && (_critter.CarryingCapacity.HeavyLoadLimit < HollowFurnishingLid.Weight))
                        {
                            _activity.EnqueueActivityResultOnStep(this, @"Too heavy to lift");
                            return true;
                        }

                        // remove from whatever path is connecting it to gameplay
                        HollowFurnishingLid.UnPath();
                        if (!HollowFurnishingLid.IsPathed())
                        {
                            HollowFurnishingLid.BindToObject(HollowFurnishing);
                        }
                    }
                    else
                    {
                        _activity.Terminate(new Info { Message = @"Unable to touch" });
                    }
                }
            }
            return true;
        }
    }
}
