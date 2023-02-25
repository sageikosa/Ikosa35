using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class PersonalAimInfoBuilder : ViewModelBase
    {
        public PersonalAimInfoBuilder(PersonalAimTargeting targetBuilder, AimTargetInfo target)
        {
            _Target = target;
            _TargetBuilder = targetBuilder;
            _Selected = null;
        }

        #region private data
        private AimTargetInfo _Target;
        private PersonalAimTargeting _TargetBuilder;
        private AwarenessInfo _Selected;
        #endregion

        public AimTargetInfo Target { get { return _Target; } }
        public PersonalAimTargeting TargetBuilder { get { return _TargetBuilder; } }

        public AwarenessInfo SelectedPerson
        {
            get { return _Selected; }
            set
            {
                _Selected = value;
                if (value != null)
                {
                    _Target.TargetID = _Selected.ID;
                }
                _TargetBuilder.SyncSelectablePersons();
                DoPropertyChanged(@"SelectedPerson");
                _TargetBuilder.SetIsReady();
            }
        }
    }
}
