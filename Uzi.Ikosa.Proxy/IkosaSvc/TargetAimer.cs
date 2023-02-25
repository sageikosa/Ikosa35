using System.Collections.Generic;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Proxy.IkosaSvc
{
    /// <summary>
    /// Associates an AimInfo with one or more TargetInfos
    /// </summary>
    public class TargetAimer
    {
        public TargetAimer(AimingModeInfo aimingMode)
        {
            _AimMode = aimingMode;
            _Targets = new List<AimTargetInfo>();
        }

        #region private data
        private List<AimTargetInfo> _Targets;
        private AimingModeInfo _AimMode;
        #endregion

        /// <summary>Aiming Mode for these targets/// </summary>
        public AimingModeInfo AimingMode { get { return _AimMode; } }

        /// <summary>Controlled by the UI</summary>
        public bool IsReady { get; set; }

        /// <summary>Target holding spot</summary>
        public List<AimTargetInfo> Targets { get { return _Targets; } }

        public void Clear()
        {
            _Targets.Clear();
            this.IsReady = false;
        }

        public void SetSingle(AimTargetInfo target)
        {
            _Targets.Clear();
            _Targets.Add(target);
            this.IsReady = true;
        }
    }
}
