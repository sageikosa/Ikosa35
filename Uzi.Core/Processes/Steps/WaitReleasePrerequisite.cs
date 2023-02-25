using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    [Serializable]
    public class WaitReleasePrerequisite : StepPrerequisite
    {
        public WaitReleasePrerequisite(object source)
            : base(source, nameof(WaitReleasePrerequisite), @"Pause")
        {
            _Ready = false;
        }

        #region data
        private bool _Ready;
        #endregion

        public override CoreActor Fulfiller => null;
        public override bool IsReady => _Ready;
        public override bool FailsProcess => false;

        public override void MergeFrom(PrerequisiteInfo info)
        {
            if (info is WaitReleasePrerequisiteInfo)
            {
                _Ready = true;
            }
        }

        public override PrerequisiteInfo ToPrerequisiteInfo(CoreStep step)
            => ToInfo<WaitReleasePrerequisiteInfo>(step);
    }
}
