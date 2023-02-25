using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    /// <summary>Generic base for aim targeting</summary>
    /// <typeparam name="AimMode">AimingModeInfo</typeparam>
    /// <typeparam name="TargetType">AimTargetInfo</typeparam>
    public abstract class AimTargeting<AimMode, TargetType> : AimTargetingBase
        where AimMode : AimingModeInfo
        where TargetType : AimTargetInfo
    {
        /// <summary>Generic base for aim targeting</summary>
        protected AimTargeting(ActivityInfoBuilder builder, AimMode aimingMode) :
            base(builder)
        {
            _AimingMode = aimingMode;
            _Targets = new ObservableCollection<TargetType>();
        }

        #region data
        protected AimMode _AimingMode;
        private ObservableCollection<TargetType> _Targets;
        #endregion

        public AimMode AimingMode => _AimingMode;
        public ObservableCollection<TargetType> Targets => _Targets;

        /// <summary>specific types and keys must match</summary>
        public override bool IsSameMode(AimingModeInfo aimMode)
            => (aimMode is AimMode) 
            ? (aimMode as AimMode)?.Key == _AimingMode?.Key
            : false;

        public override void SyncMode(AimingModeInfo aimMode)
            => SyncAimMode(aimMode as AimMode);

        protected abstract void SyncAimMode(AimMode aimMode);

        public override void SetTargets(List<AimTargetInfo> targets)
            => SetAimTargets(targets.OfType<TargetType>().ToList());

        protected abstract void SetAimTargets(List<TargetType> targets);

        public int TargetCount 
            => _Targets.Count;

        public override IEnumerable<AimTargetInfo> FinishedTargets 
            => _Targets;
    }
}
