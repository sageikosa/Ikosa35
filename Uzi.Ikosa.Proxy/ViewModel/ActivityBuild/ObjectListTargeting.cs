using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class ObjectListTargeting : AimTargeting<ObjectListAimInfo, AimTargetInfo>
    {
        #region ctor()
        public ObjectListTargeting(ActivityInfoBuilder builder, ObjectListAimInfo aimMode)
            : base(builder, aimMode)
        {
            _TargetBuilders = new ObservableCollection<ObjectListAimInfoBuilder>();
            for (var _tx = 0; (int)_tx < AimingMode.MinimumAimingModes; _tx++)
            {
                var _newTarget = new AimTargetInfo
                {
                    Key = AimingMode.Key,
                    TargetID = null
                };
                Targets.Add(_newTarget);
                _TargetBuilders.Add(new ObjectListAimInfoBuilder(this, _newTarget));
            }
            _Objects = new ObservableCollection<CoreInfo>(Latest());
        }

        static ObjectListTargeting()
        {
            Unselected = new CoreInfo
            {
                ID = Guid.Empty,
                Message = @"-- Select --"
            };
        }
        #endregion

        #region data
        private ObservableCollection<ObjectListAimInfoBuilder> _TargetBuilders;
        private readonly ObservableCollection<CoreInfo> _Objects;
        #endregion

        public static readonly CoreInfo Unselected;

        public ObservableCollection<CoreInfo> Objects
            => _Objects;

        private IEnumerable<CoreInfo> Latest()
        {
            yield return Unselected;
            foreach (var _slct in AimingMode.ObjectInfos)
            {
                yield return _slct;
            }
            yield break;
        }

        #region public void SyncObjects()
        public void SyncObjects()
        {
            var _latest = Latest().ToList();
            var _existing = Objects.ToList();

            // remove existing not in current
            foreach (var _rmv in (from _e in _existing
                                  where !_latest.Any(_c => _c.CompareKey == _e.CompareKey)
                                  select _e).ToList())
                Objects.Remove(_rmv);

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
                Objects.Add(_add);
        }
        #endregion

        #region public void SyncSelectableObjects()
        /// <summary>Ensure that up to the maximum number of objects are presented to be selected (on at a time)</summary>
        public void SyncSelectableObjects()
        {
            // currently have none unselected, but have more capacity
            if (Targets.Count < AimingMode.MaximumAimingModes)
            {
                if (!Targets.Any(_t => _t.TargetID == null))
                {
                    var _newTarget = new AimTargetInfo
                    {
                        Key = AimingMode.Key,
                        TargetID = null
                    };
                    Targets.Add(_newTarget);
                    _TargetBuilders.Add(new ObjectListAimInfoBuilder(this, _newTarget));
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
        public ObservableCollection<ObjectListAimInfoBuilder> ObjectBuilders
            => _TargetBuilders;

        public override bool IsReady
            => Targets.Count(_t => _t.TargetID != null) >= AimingMode.MinimumAimingModes;

        protected override void SyncAimMode(ObjectListAimInfo aimMode)
        {
            _AimingMode = aimMode;
            SyncSelectableObjects();
            SyncObjects();
        }

        protected override void SetAimTargets(List<AimTargetInfo> targets)
        {
            if (ObjectBuilders.Any() && targets.Any())
            {
                foreach (var _target in targets)
                {
                    var _idx = targets.IndexOf(_target);
                    if (ObjectBuilders.Count > _idx)
                    {
                        // only do stuff if we have capacity
                        var _builder = ObjectBuilders[_idx];
                        _builder.SelectedObject = Objects.FirstOrDefault(_obj => _obj.ID == _target.TargetID);

                        // try to expand capacity if necessary (more available and unselected)
                        SyncSelectableObjects();
                    }
                }
            }
        }
    }
}
