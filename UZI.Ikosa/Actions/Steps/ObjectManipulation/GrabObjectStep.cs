using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Interactions.Action;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class GrabObjectStep : CoreStep
    {
        public GrabObjectStep(CoreActivity activity, CoreObject coreObject)
            : base(activity)
        {
            _CoreObject = coreObject;
        }

        #region state
        private CoreObject _CoreObject;
        #endregion

        public CoreObject CoreObject => _CoreObject;

        public override bool IsDispensingPrerequisites => false;
        protected override StepPrerequisite OnNextPrerequisite() => null;

        protected override bool OnDoStep()
        {
            if (Process is CoreActivity _activity
                && _activity.Actor is Creature _critter)
            {
                if (ManipulateTouch.CanManipulateTouch(_critter, CoreObject))
                {
                    // already have a target?
                    var _grabTarget = CoreObject.Adjuncts.OfType<ObjectGrabbed>().FirstOrDefault();

                    // ensure we have a group to work with
                    var _grabGroup = _grabTarget?.ObjectGrabGroup ?? new ObjectGrabGroup();
                    if (_grabTarget == null)
                    {
                        // no target? create one
                        _grabTarget = new ObjectGrabbed(_grabGroup);
                        CoreObject.AddAdjunct(_grabTarget);
                    }

                    // add grabber to grab group
                    _activity?.Actor.AddAdjunct(new ObjectGrabber(_grabGroup));

                    // return status of attempt
                    _activity.EnqueueActivityResultOnStep(this, @"Currently grabbing");
                }
                else
                {
                    var _info = GetInfoData.GetInfoFeedback(CoreObject, _critter);
                    if (_info != null)
                    {
                        _activity.Terminate(new Info { Message = @"Unable to touch" }, _info);
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
