using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class QuantitySelectAimInfoBuilder : ViewModelBase
    {
        public QuantitySelectAimInfoBuilder(QuantitySelectTargeting targetBuilder, QuantitySelectTargetInfo target)
        {
            _Target = target;
            _TargetBuilder = targetBuilder;
            _Selected = QuantitySelectTargeting.Unselected;
            _Quantities = new List<int>();
        }

        #region data
        private QuantitySelectTargetInfo _Target;
        private QuantitySelectTargeting _TargetBuilder;
        private QuantitySelectorInfo _Selected;
        private List<int> _Quantities;
        #endregion

        public QuantitySelectTargetInfo Target => _Target;
        public QuantitySelectTargeting TargetBuilder => _TargetBuilder;

        public QuantitySelectorInfo SelectedInfo
        {
            get { return _Selected; }
            set
            {
                if (_Selected != value)
                {
                    _Quantities = new List<int>();
                }
                _Selected = value;
                if (value != null)
                {
                    _Target.Selector = _Selected;
                    _Quantities = new List<int>(Enumerable.Range(_Selected.MinimumSelection, _Selected.MaximumSelection));
                }
                _TargetBuilder.SyncSelectableBuilders();
                DoPropertyChanged(nameof(SelectedInfo));
                DoPropertyChanged(nameof(Quantities));
                _TargetBuilder.SetIsReady();
            }
        }

        public IEnumerable<int> Quantities
            => _Quantities.Select(_q => _q);
    }
}
