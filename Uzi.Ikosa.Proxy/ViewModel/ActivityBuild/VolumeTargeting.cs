using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class VolumeTargeting : AimTargeting<VolumeAimInfo, CubicTargetInfo>
    {
        public VolumeTargeting(ActivityInfoBuilder builder, VolumeAimInfo aimMode)
            : base(builder, aimMode)
        {
            // TODO: seed with any queued cells
        }

        // TODO:

        public override bool IsReady
        {
            get
            {
                return false;
            }
        }

        protected override void SyncAimMode(VolumeAimInfo aimMode)
        {
            _AimingMode = aimMode;
        }

        protected override void SetAimTargets(List<CubicTargetInfo> targets)
        {
        }
    }
}
