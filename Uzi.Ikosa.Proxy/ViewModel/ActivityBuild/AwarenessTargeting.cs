using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class AwarenessTargeting : AimTargeting<AwarenessAimInfo, AwarenessTargetInfo>
    {
        #region ctor()
        public AwarenessTargeting(ActivityInfoBuilder builder, AwarenessAimInfo aimMode)
            : base(builder, aimMode)
        {
            _Choices = new ObservableCollection<AwarenessChoice>();
            _TargetBuilders = new ObservableCollection<AwarenessTargetInfoBuilder>();
            SyncSelectableAwarenesses();
        }
        #endregion

        #region data
        private ObservableCollection<AwarenessChoice> _Choices;
        private ObservableCollection<AwarenessTargetInfoBuilder> _TargetBuilders;
        #endregion

        #region public void SyncSelectableAwarenesses()
        /// <summary>Ensure that up to the maximum number of awarenesses are presented to be selected (one at a time)</summary>
        public void SyncSelectableAwarenesses()
        {
            // currently have none unselected, but have more capacity
            if (Targets.Count < AimingMode.MaximumAimingModes)
            {
                if (Targets.All(_t => _t.TargetID.HasValue))
                {
                    var _newTarget = new AwarenessTargetInfo
                    {
                        Key = AimingMode.Key,
                        TargetID = null
                    };
                    Targets.Add(_newTarget);
                    _TargetBuilders.Add(new AwarenessTargetInfoBuilder(this, _newTarget));
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

        public ObservableCollection<AwarenessChoice> AwarenessChoices
            => _Choices;

        #region public void SetAwarenessChoices(List<AwarenessChoice> choices)
        public void SetAwarenessChoices(List<AwarenessChoice> choices)
        {
            // same action, synchronize targets...
            foreach (var _rmv in (from _ch in _Choices
                                  where !choices.Any(_c => _c.Awareness.ID == _ch.Awareness.ID)
                                  select _ch).ToList())
            {
                // remove target builders that are no longer available
                _Choices.Remove(_rmv);
            }

            foreach (var _add in (from _c in choices
                                  where !_Choices.Any(_ch => _ch.Awareness.ID == _c.Awareness.ID)
                                  select _c).ToList())
            {
                // add target builders that are now available
                _Choices.Add(_add);
            }

            FillEmptyChoices();
        }

        private void FillEmptyChoices()
        {
            if (_Choices.Any() && _TargetBuilders.All(_tb => _tb.AwarenessChoice == null))
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
                    _TargetBuilders[_tx].AwarenessChoice = _Choices[_cx];
                    SyncSelectableAwarenesses();

                    // step forward in both lists
                    _tx++;
                    _cx++;

                } while ((_tx < _TargetBuilders.Count) && ((_cx < _Choices.Count) || AimingMode.AllowDuplicates));
            }
        }
        #endregion

        /// <summary>Targets wrapped in builders</summary>
        public ObservableCollection<AwarenessTargetInfoBuilder> TargetBuilders
            => _TargetBuilders;

        public override bool IsReady
            => Targets.Count(_t => _t.TargetID.HasValue) >= AimingMode.MinimumAimingModes;

        protected override void SyncAimMode(AwarenessAimInfo aimMode)
        {
            _AimingMode = aimMode;
            SyncSelectableAwarenesses();
            FillEmptyChoices();
        }

        protected override void SetAimTargets(List<AwarenessTargetInfo> targets)
        {
            // NOP
            if (TargetBuilders.Any() && targets.Any())
            {
                var _builder = TargetBuilders[0];
                var _target = targets.FirstOrDefault();
                _builder.AwarenessChoice = AwarenessChoices.FirstOrDefault(_ac => _ac.Awareness.ID == _target.TargetID);
            }
        }
    }
}
