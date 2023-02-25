using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Ikosa.Contracts;
using Uzi.Visualize;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class LocationTargeting : AimTargeting<LocationAimInfo, LocationTargetInfo>
    {
        public LocationTargeting(ActivityInfoBuilder builder, LocationAimInfo aimMode)
            : base(builder, aimMode)
        {
            _Choices = new ObservableCollection<LocationChoice>();
            _TargetBuilders = new ObservableCollection<LocationTargetInfoBuilder>();
            SyncSelectableLocations();
        }

        #region data
        private ObservableCollection<LocationChoice> _Choices;
        private ObservableCollection<LocationTargetInfoBuilder> _TargetBuilders;
        #endregion

        #region public void SyncSelectableLocations()
        /// <summary>Ensure that up to the maximum number of attacks are presented to be selected (one at a time)</summary>
        public void SyncSelectableLocations()
        {
            // currently have none unselected, but have more capacity
            if (Targets.Count < AimingMode.MaximumAimingModes)
            {
                if (Targets.All(_t => _t.CellInfo != null))
                {
                    var _newTarget = new LocationTargetInfo
                    {
                        Key = AimingMode.Key,
                        TargetID = null,
                        CellInfo = null,
                        LocationAimMode = LocationAimMode.Cell
                    };

                    Targets.Add(_newTarget);
                    _TargetBuilders.Add(new LocationTargetInfoBuilder(this, _newTarget));
                    DoPropertyChanged(nameof(TargetBuilders));
                }
            }
            else
                while ((Targets.Count > AimingMode.MaximumAimingModes) && _TargetBuilders.Any())
                {
                    Targets.Remove(Targets.Last());
                    _TargetBuilders.Remove(_TargetBuilders.Last());
                }
        }
        #endregion

        public ObservableCollection<LocationChoice> LocationChoices
            => _Choices;

        #region public void SetLocationChoices(List<LocationChoice> choices)
        public void SetLocationChoices(List<LocationChoice> choices)
        {
            // same action, synchronize targets...
            foreach (var _rmv in (from _ch in _Choices
                                  where !choices.Any(_c => _c.TargetCell.IsCellEqual(_ch.TargetCell))
                                  select _ch).ToList())
            {
                // remove target builders that are no longer available
                _Choices.Remove(_rmv);
            }

            foreach (var _add in (from _c in choices
                                  where !_Choices.Any(_ch => _ch.TargetCell.IsCellEqual(_c.TargetCell))
                                  select _c).ToList())
            {
                // add target builders that are now available
                _Choices.Add(_add);
            }

            FillEmptyChoices();
        }

        private void FillEmptyChoices()
        {
            if (_Choices.Any() && _TargetBuilders.All(_tb => _tb.LocationChoice == null))
            {
                // assign indexers
                var _tx = 0;
                var _cx = 0;
                do
                {
                    // reset choices indexer
                    if (_cx >= _Choices.Count)
                        _cx = 0;

                    // set and sync available
                    _TargetBuilders[_tx].LocationChoice = _Choices[_cx];
                    SyncSelectableLocations();

                    // step forward in both lists
                    _tx++;
                    _cx++;

                } while ((_tx < _TargetBuilders.Count) && (_cx < _Choices.Count));
            }
        }
        #endregion

        public override bool IsReady
            => Targets.Count(_t => _t.CellInfo != null) >= AimingMode.MinimumAimingModes;

        /// <summary>Targets wrapped in builders</summary>
        public ObservableCollection<LocationTargetInfoBuilder> TargetBuilders
            => _TargetBuilders;

        protected override void SyncAimMode(LocationAimInfo aimMode)
        {
            _AimingMode = aimMode;
            SyncSelectableLocations();
            FillEmptyChoices();
        }

        protected override void SetAimTargets(List<LocationTargetInfo> targets)
        {
            // NOP
            if (TargetBuilders.Any() && targets.Any())
            {
                var _builder = TargetBuilders[0];
                var _target = targets.FirstOrDefault();
                _builder.LocationChoice = LocationChoices.FirstOrDefault(_lc => _lc.TargetCell.IsCellEqual(_target.CellInfo));
            }
        }
    }
}
