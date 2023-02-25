using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class QuantitySelectTargeting : AimTargeting<QuantitySelectAimInfo, QuantitySelectTargetInfo>
    {
        #region ctor()
        public QuantitySelectTargeting(ActivityInfoBuilder builder, QuantitySelectAimInfo aimMode)
            : base(builder, aimMode)
        {
            _TargetBuilders = new ObservableCollection<QuantitySelectAimInfoBuilder>();
            for (var _tx = 0; _tx < AimingMode.MinimumAimingModes; _tx++)
            {
                var _newTarget = new QuantitySelectTargetInfo
                {
                    Key = AimingMode.Key,
                    TargetID = null,
                    Selector = null
                };
                Targets.Add(_newTarget);
                _TargetBuilders.Add(new QuantitySelectAimInfoBuilder(this, _newTarget));
            }

            // setup initial selectors
            _Selectors = new ObservableCollection<QuantitySelectorInfo>(Latest());
        }

        static QuantitySelectTargeting()
        {
            Unselected = new QuantitySelectorInfo
            {
                CoreInfo = null,
                MinimumSelection = 0,
                MaximumSelection = 0,
                CurrentSelection = 0
            };
        }
        #endregion

        #region data
        private ObservableCollection<QuantitySelectAimInfoBuilder> _TargetBuilders;
        private readonly ObservableCollection<QuantitySelectorInfo> _Selectors;
        public static readonly QuantitySelectorInfo Unselected;
        #endregion

        public ObservableCollection<QuantitySelectorInfo> Selectors
            => _Selectors;

        private IEnumerable<QuantitySelectorInfo> Latest()
        {
            yield return Unselected;
            foreach (var _slct in AimingMode.Selectors)
            {
                yield return _slct;
            }
            yield break;
        }

        #region public void SyncSelectors()
        public void SyncSelectors()
        {
            var _latest = Latest().ToList();
            var _existing = Selectors.ToList();

            // remove existing not in current
            foreach (var _rmv in (from _e in _existing
                                  where !_latest.Any(_c => _c.CoreInfo?.CompareKey == _e.CoreInfo?.CompareKey)
                                  select _e).ToList())
                Selectors.Remove(_rmv);

            // update existing
            foreach (var _updt in (from _e in _existing
                                   join _c in _latest
                                   on _e.CoreInfo?.CompareKey equals _c.CoreInfo?.CompareKey
                                   select new { Exist = _e, Current = _c }).ToList())
            {
                if (_updt.Exist.MinimumSelection != _updt.Current.MinimumSelection)
                {
                    _updt.Exist.MinimumSelection = _updt.Current.MinimumSelection;
                }
                if (_updt.Exist.MaximumSelection != _updt.Current.MaximumSelection)
                {
                    _updt.Exist.MaximumSelection = _updt.Current.MaximumSelection;
                }
            }

            // add current not in existing
            foreach (var _add in (from _c in _latest
                                  where !_existing.Any(_e => _e.CoreInfo?.CompareKey == _c.CoreInfo?.CompareKey)
                                  select _c).ToList())
                Selectors.Add(_add);
        }
        #endregion

        #region public void SyncSelectableBuilders()
        /// <summary>Ensure that up to the maximum number of objects are presented to be selected (on at a time)</summary>
        public void SyncSelectableBuilders()
        {
            // currently have none unselected, but have more capacity
            if (Targets.Count < AimingMode.MaximumAimingModes)
            {
                if (!Targets.Any(_t => _t.Selector == null))
                {
                    var _newTarget = new QuantitySelectTargetInfo
                    {
                        Key = AimingMode.Key,
                        TargetID = null,
                        Selector = null
                    };
                    Targets.Add(_newTarget);
                    _TargetBuilders.Add(new QuantitySelectAimInfoBuilder(this, _newTarget));
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

        /// <summary>Targets wrapped in builders</summary>
        public ObservableCollection<QuantitySelectAimInfoBuilder> QuantitySelectBuilders
            => _TargetBuilders;

        public override bool IsReady
            => Targets.Count(_t => (_t.Selector?.CurrentSelection ?? 0) > 0) >= AimingMode.MinimumAimingModes;

        #region protected override void SyncAimMode(QuantitySelectAimInfo aimMode)
        protected override void SyncAimMode(QuantitySelectAimInfo aimMode)
        {
            _AimingMode = aimMode;
            SyncSelectableBuilders();
            SyncSelectors();
            DoPropertyChanged(nameof(Selectors));
        }
        #endregion

        protected override void SetAimTargets(List<QuantitySelectTargetInfo> targets)
        {
            // NOP?
        }
    }
}
