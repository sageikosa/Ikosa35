using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class OptionAimOptionBuilder : ViewModelBase
    {
        public OptionAimOptionBuilder(OptionTargeting targetBuilder, OptionTargetInfo target)
        {
            _Target = target;
            _TargetBuilder = targetBuilder;
            _Selected = OptionTargeting.Unselected;
        }

        #region private data
        private OptionTargetInfo _Target;
        private OptionTargeting _TargetBuilder;
        private OptionAimOption _Selected;
        #endregion

        public OptionTargetInfo Target => _Target;
        public OptionTargeting TargetBuilder => _TargetBuilder;

        public OptionAimOption SelectedOption
        {
            get { return _Selected; }
            set
            {
                _Selected = value;
                if (value != null)
                {
                    _Target.OptionKey = value.Key;
                    _Target.OptionName = value.Name;
                    _Target.OptionDescription = value.Description;
                }
                _TargetBuilder.SyncSelectableOptions();
                DoPropertyChanged(nameof(SelectedOption));
                _TargetBuilder.SetIsReady();
            }
        }
    }
}
