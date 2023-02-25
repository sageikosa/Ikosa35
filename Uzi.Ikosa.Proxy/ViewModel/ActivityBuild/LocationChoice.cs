using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Visualize;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class LocationChoice : ViewModelBase
    {
        public LocationChoice(LocationTargeting targetBuilder)
        {
            _TargetBuilder = targetBuilder;
        }

        #region data
        private LocationTargeting _TargetBuilder;
        private ICellLocation _TargetCell;
        #endregion

        public LocationTargeting Targeting => _TargetBuilder;

        public ICellLocation TargetCell { get { return _TargetCell; } set { _TargetCell = value; } }
    }
}
