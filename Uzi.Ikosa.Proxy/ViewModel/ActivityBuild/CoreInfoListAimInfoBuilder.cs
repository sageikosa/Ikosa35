using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class CoreInfoListAimInfoBuilder : ViewModelBase
    {
        public CoreInfoListAimInfoBuilder(CoreInfoListTargeting targetBuilder, CoreInfoTargetInfo target)
        {
            _Target = target;
            _TargetBuilder = targetBuilder;
            _Selected = CoreInfoListTargeting.Unselected;
        }

        #region data
        private CoreInfoTargetInfo _Target;
        private CoreInfoListTargeting _TargetBuilder;
        private CoreInfo _Selected;
        #endregion

        public CoreInfoTargetInfo Target => _Target;
        public CoreInfoListTargeting TargetBuilder => _TargetBuilder;

        public CoreInfo SelectedInfo
        {
            get { return _Selected; }
            set
            {
                _Selected = value;
                if (value != null)
                {
                    _Target.CoreInfo = _Selected;
                }
                _TargetBuilder.SyncSelectableInfos();
                DoPropertyChanged(nameof(SelectedInfo));
                _TargetBuilder.SetIsReady();
            }
        }
    }
}
