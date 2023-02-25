using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Actions;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class ClimbSustainStep : PreReqListStepBase
    {
        public ClimbSustainStep(Qualifier qualifier, SuccessCheck check, string name)
            : base((CoreProcess)null)
        {
            // workset should include actor needing to make the save as the target
            _PendingPreRequisites.Enqueue(
                new SuccessCheckPrerequisite(this, qualifier, @"ClimbCheck", name, check, false));
        }

        protected override bool OnDoStep()
        {
            var _checkPre = AllPrerequisites<SuccessCheckPrerequisite>().FirstOrDefault();
            if (_checkPre != null)
            {
                var _id = _checkPre.Qualification.Target.ID;
                if (!_checkPre.Success)
                {
                    // target is the actor who needed to save
                    var _located = (_checkPre.Qualification.Target as Creature).GetLocated();
                    if (_located != null)
                    {
                        EnqueueNotify(new CheckResultNotify(_id, @"Movement", false, new Description(@"Climb", @"Failure")), _id);
                        AppendFollowing(new FallingStartStep(this, _located.Locator, 500, 0, 1));
                    }
                }
                else
                {
                    EnqueueNotify(new CheckResultNotify(_id, @"Movement", true, new Description(@"Climb", @"Success")), _id);
                }
            }
            return true;
        }
    }
}
