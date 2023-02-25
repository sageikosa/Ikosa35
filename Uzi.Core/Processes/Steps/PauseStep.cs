using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    [Serializable]
    public class PauseStep : PreReqListStepBase, IStoppableStep
    {
        public PauseStep()
            : base((CoreProcess)null)
        {
            _PendingPreRequisites.Enqueue(new WaitReleasePrerequisite(typeof(PauseStep)));
        }

        public void StopStep()
        {
            DispensedPrerequisites.OfType<WaitReleasePrerequisite>().FirstOrDefault()?
                .MergeFrom(new WaitReleasePrerequisiteInfo());
        }

        protected override bool OnDoStep()
            => true;
    }
}
