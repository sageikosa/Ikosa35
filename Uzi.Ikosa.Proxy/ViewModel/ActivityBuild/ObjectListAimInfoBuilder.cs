using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class ObjectListAimInfoBuilder : ViewModelBase
    {
        public ObjectListAimInfoBuilder(ObjectListTargeting targetBuilder, AimTargetInfo target)
        {
            _Target = target;
            _TargetBuilder = targetBuilder;
            _Selected = ObjectListTargeting.Unselected;
        }

        #region private data
        private AimTargetInfo _Target;
        private ObjectListTargeting _TargetBuilder;
        private CoreInfo _Selected;
        #endregion

        public AimTargetInfo Target => _Target;
        public ObjectListTargeting TargetBuilder => _TargetBuilder;

        public CoreInfo SelectedObject
        {
            get { return _Selected; }
            set
            {
                _Selected = value;
                if (value != null)
                {
                    _Target.TargetID = _Selected.ID;
                }
                _TargetBuilder.SyncSelectableObjects();
                DoPropertyChanged(nameof(SelectedObject));
                _TargetBuilder.SetIsReady();
            }
        }
    }
}
