using System.Collections.Generic;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public abstract class AimTargetingBase : ViewModelBase
    {
        protected AimTargetingBase(ActivityInfoBuilder builder)
        {
            _Builder = builder;
        }

        protected ActivityInfoBuilder _Builder;

        public ActivityInfoBuilder Builder => _Builder;

        public abstract bool IsReady { get; }

        public abstract bool IsSameMode(AimingModeInfo aimMode);

        public abstract void SyncMode(AimingModeInfo aimMode);

        public abstract IEnumerable<AimTargetInfo> FinishedTargets { get; }

        public abstract void SetTargets(List<AimTargetInfo> targets);

        /// <summary>Signals that the IsReady has changeed and informs the builder to signal it's IsReady flag as well</summary>
        public void SetIsReady()
        {
            DoPropertyChanged(nameof(IsReady));
            Builder.SetIsReady();
        }
    }
}
