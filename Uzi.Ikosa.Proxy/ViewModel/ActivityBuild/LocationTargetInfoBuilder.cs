using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Ikosa.Contracts;
using Uzi.Visualize;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class LocationTargetInfoBuilder : ViewModelBase
    {
        public LocationTargetInfoBuilder(LocationTargeting targetBuilder, LocationTargetInfo target)
        {
            _Choice = null;
            _Target = target;
            _TargetBuilder = targetBuilder;
        }

        #region data
        private LocationChoice _Choice;
        private LocationTargetInfo _Target;
        private LocationTargeting _TargetBuilder;
        #endregion

        public LocationTargetInfo Target => _Target;
        public LocationTargeting TargetBuilder => _TargetBuilder;

        private void SyncFromProperty(string propName)
        {
            DoPropertyChanged(propName);
            _TargetBuilder.SyncSelectableLocations();
            _TargetBuilder.SetIsReady();
        }

        public LocationChoice LocationChoice
        {
            get
            {
                return _Choice;
            }
            set
            {
                _Choice = value;
                if (value != null)
                {
                    Target.CellInfo = new Visualize.Contracts.CellInfo(value.TargetCell);
                }
                else
                {
                    Target.CellInfo = null;
                }
                SyncFromProperty(nameof(LocationChoice));
            }
        }
    }
}
