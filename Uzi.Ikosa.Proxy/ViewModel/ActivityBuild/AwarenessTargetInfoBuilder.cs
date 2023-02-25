using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class AwarenessTargetInfoBuilder : ViewModelBase
    {
        public AwarenessTargetInfoBuilder(AwarenessTargeting targetBuilder, AwarenessTargetInfo target)
        {
            _Choice = null;
            _Target = target;
            _TargetBuilder = targetBuilder;
        }

        #region private data
        private AwarenessChoice _Choice;
        private AwarenessTargetInfo _Target;
        private AwarenessTargeting _TargetBuilder;
        #endregion

        public AwarenessTargetInfo Target => _Target;
        public AwarenessTargeting TargetBuilder => _TargetBuilder;

        #region public AwarenessChoice AwarenessChoice { get; set; }
        public AwarenessChoice AwarenessChoice
        {
            get
            {
                return _Choice;
            }
            set
            {
                _Choice = value;
                if (_Choice != null)
                {
                    Target.TargetID = _Choice.Awareness != null ? (Guid?)_Choice.Awareness.ID : null;
                }
                else
                {
                    Target.TargetID = null;
                }
                DoPropertyChanged(nameof(AwarenessChoice));
                _TargetBuilder.SyncSelectableAwarenesses();
                _TargetBuilder.SetIsReady();
            }
        }
        #endregion
    }
}
