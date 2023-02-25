using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class CoreInfoListTargeting : AimTargeting<CoreListAimInfo, CoreInfoTargetInfo>
    {
        #region ctor()
        public CoreInfoListTargeting(ActivityInfoBuilder builder, CoreListAimInfo aimMode)
            : base(builder, aimMode)
        {
            _TargetBuilders = new ObservableCollection<CoreInfoListAimInfoBuilder>();
            for (var _tx = 0; _tx < AimingMode.MinimumAimingModes; _tx++)
            {
                var _newTarget = new CoreInfoTargetInfo
                {
                    Key = AimingMode.Key,
                    TargetID = null,
                    CoreInfo = null
                };
                Targets.Add(_newTarget);
                _TargetBuilders.Add(new CoreInfoListAimInfoBuilder(this, _newTarget));
            }
            _Infos = new ObservableCollection<CoreInfo>(Latest());
        }

        static CoreInfoListTargeting()
        {
            Unselected = new CoreInfo
            {
                ID = Guid.Empty,
                Message = @"-- Select --"
            };
        }
        #endregion

        #region data
        private ObservableCollection<CoreInfoListAimInfoBuilder> _TargetBuilders;
        private readonly ObservableCollection<CoreInfo> _Infos;
        public static readonly CoreInfo Unselected;
        #endregion

        public ObservableCollection<CoreInfo> Infos
            => _Infos;

        private IEnumerable<CoreInfo> Latest()
        {
            yield return Unselected;
            foreach (var _slct in AimingMode.ObjectInfos)
            {
                yield return _slct;
            }
            yield break;
        }

        #region public void SyncInfos()
        public void SyncInfos()
        {
            var _latest = Latest().ToList();
            var _existing = Infos.ToList();

            // remove existing not in current
            foreach (var _rmv in (from _e in _existing
                                  where !_latest.Any(_c => _c.CompareKey == _e.CompareKey)
                                  select _e).ToList())
                Infos.Remove(_rmv);

            // update existing
            foreach (var _updt in (from _e in _existing
                                   join _c in _latest
                                   on _e.CompareKey equals _c.CompareKey
                                   select new { Exist = _e, Current = _c }).ToList())
            {
                // NOP...
            }

            // add current not in existing
            foreach (var _add in (from _c in _latest
                                  where !_existing.Any(_e => _e.CompareKey == _c.CompareKey)
                                  select _c).ToList())
                Infos.Add(_add);
        }
        #endregion

        #region public void SyncSelectableInfos()
        /// <summary>Ensure that up to the maximum number of objects are presented to be selected (on at a time)</summary>
        public void SyncSelectableInfos()
        {
            // currently have none unselected, but have more capacity
            if (Targets.Count < AimingMode.MaximumAimingModes)
            {
                if (!Targets.Any(_t => _t.CoreInfo == null))
                {
                    var _newTarget = new CoreInfoTargetInfo
                    {
                        Key = AimingMode.Key,
                        TargetID = null,
                        CoreInfo = null
                    };
                    Targets.Add(_newTarget);
                    _TargetBuilders.Add(new CoreInfoListAimInfoBuilder(this, _newTarget));
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
        public ObservableCollection<CoreInfoListAimInfoBuilder> CoreInfoBuilders
            => _TargetBuilders;

        public override bool IsReady
            => Targets.Count(_t => _t.TargetID != null) >= AimingMode.MinimumAimingModes;

        #region public override void SyncAimMode(CoreListAimInfo aimMode)
        protected override void SyncAimMode(CoreListAimInfo aimMode)
        {
            _AimingMode = aimMode;
            SyncSelectableInfos();
            SyncInfos();
        }
        #endregion

        protected override void SetAimTargets(List<CoreInfoTargetInfo> targets)
        {
            if (CoreInfoBuilders.Any() && targets.Any())
            {
                var _builder = CoreInfoBuilders[0];
                var _target = targets.FirstOrDefault();
                _builder.SelectedInfo = Infos.FirstOrDefault(_ci => _ci.ID == _target.CoreInfo.ID);
            }
        }
    }
}
