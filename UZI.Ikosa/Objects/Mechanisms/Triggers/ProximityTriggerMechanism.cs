using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class ProximityTriggerMechanism : TriggerMechanism
    {
        #region ctor()
        public ProximityTriggerMechanism(string name, Material material, int disableDifficulty,
            PostTriggerState postState, Size minSize, double range, MechanismMount mount)
            : base(name, material, disableDifficulty, postState)
        {
            _MinSize = minSize.Order;
            _Range = range;

            // this mechanism is the master of a group
            var _ptGroup = new ProximityTriggerGroup();
            var _ptMaster = new ProximityTriggerMaster(_ptGroup);
            AddAdjunct(_ptMaster);

            // connect to a mechanism mount...
            var _ptTarget = new ProximityTriggerTarget(_ptGroup);
            mount?.AddAdjunct(_ptTarget);
            ObjectSizer.NaturalSize = Size.Small;
        }
        #endregion

        public ProximityTriggerMaster ProximityTriggerMaster
            => Adjuncts.OfType<ProximityTriggerMaster>().FirstOrDefault();

        #region public MechanismMount MechanismMount { get; set; }
        public MechanismMount MechanismMount
        {
            get => ProximityTriggerMaster?.ProximityTriggerGroup.ProximityTriggerTarget?.MechanismMount;
            set
            {
                var _target = ProximityTriggerMaster?.ProximityTriggerGroup.ProximityTriggerTarget;
                if (_target?.MechanismMount != null)
                {
                    // stop monitoring the old one, and de-link it
                    _target.Eject();
                }

                // link the new one
                value?.AddAdjunct(_target);
            }
        }
        #endregion

        #region data
        private int _MinSize;
        private double _Range;
        #endregion

        protected override string ClassIconKey => nameof(ProximityTriggerMechanism);

        public int MinimumSize
        {
            get => _MinSize;
            set
            {
                _MinSize = value;
                DoPropertyChanged(nameof(MinimumSize));
            }
        }

        public double Range
        {
            get => _Range;
            set
            {
                _Range = value;
                DoPropertyChanged(nameof(Range));
                ProximityTriggerMaster?.ProximityTriggerGroup?.ProximityTriggerTarget?.RefreshCapture();
            }
        }
    }

    [Serializable]
    public class ProximityTriggerGroup : TriggerMountGroup<ProximityTriggerMechanism>
    {
        public ProximityTriggerGroup()
            : base(typeof(ProximityTriggerGroup))
        {
        }

        public ProximityTriggerMaster ProximityTriggerMaster => Master as ProximityTriggerMaster;
        public ProximityTriggerTarget ProximityTriggerTarget => Target as ProximityTriggerTarget;

        public override void ValidateGroup()
            => this.ValidateMasteredPlanarLink();
    }

    [Serializable]
    public class ProximityTriggerMaster : TriggerMountMaster<ProximityTriggerMechanism>
    {
        public ProximityTriggerMaster(ProximityTriggerGroup group)
            : base(group)
        {
        }

        public override bool CanAnchor(IAdjunctable newAnchor)
            => (newAnchor is ProximityTriggerMechanism) && base.CanAnchor(newAnchor);

        public ProximityTriggerGroup ProximityTriggerGroup
            => TriggerGroup as ProximityTriggerGroup;

        public override object Clone()
            => new ProximityTriggerMaster(ProximityTriggerGroup);
    }

    [Serializable]
    public class ProximityTriggerTarget : TriggerMountTarget<ProximityTriggerMechanism>, ILocatorZone,
        IMonitorChange<Size>
    {
        public ProximityTriggerTarget(ProximityTriggerGroup group)
            : base(group)
        {
            _SizeCtrlrs = new Dictionary<Sizer, Locator>();
            _Capture = null;
        }

        #region data
        private Dictionary<Sizer, Locator> _SizeCtrlrs;
        private LocatorCapture _Capture;
        #endregion

        public override bool CanAnchor(IAdjunctable newAnchor)
            => (newAnchor is MechanismMount) && base.CanAnchor(newAnchor);

        #region public void RefreshCapture()
        public void RefreshCapture()
        {
            if (_Capture != null)
            {
                // remove old capture
                _Capture?.RemoveZone();
                ReleaseAllMonitoring();
            }

            var _mech = ProximityTriggerGroup.Master?.TriggerMechanism;
            if (_mech != null)
            {
                var _loc = Anchor.GetLocated()?.Locator;
                if (_loc != null)
                {
                    // define new capture
                    _Capture = new LocatorCapture(_loc.MapContext, this,
                        new Geometry(new SphereBuilder(Convert.ToInt32(_mech.Range / 5)), _loc, false), _loc,
                        this, true, _loc.PlanarPresence);
                }
            }
        }
        #endregion

        #region OnActivate/OnDeactivate: watch for "captured" locator weight changes
        protected override void OnActivate(object source)
        {
            RefreshCapture();
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            // remove capture
            _Capture?.RemoveZone();
            _Capture = null;
            ReleaseAllMonitoring();
            base.OnDeactivate(source);
        }
        #endregion

        public MechanismMount MechanismMount
            => Anchor as MechanismMount;

        public ProximityTriggerGroup ProximityTriggerGroup
            => TriggerGroup as ProximityTriggerGroup;

        public override object Clone()
            => new ProximityTriggerTarget(ProximityTriggerGroup);

        /// <summary>Check to see if triggering should occur, and if so, call DoTrigger() for the mechanism</summary>
        public override void CheckTriggering()
        {
            // NOTE: only check when something changes in environment, not when trigger is (re-)activated
        }

        #region Monitor/Release Size
        private void MonitorSize(Locator locator)
        {
            // monitor changes
            if ((locator?.ICore is ISizable _sizer)
                && !_SizeCtrlrs.ContainsKey(_sizer.Sizer))
            {
                _SizeCtrlrs.Add(_sizer.Sizer, locator);
                _sizer.Sizer.AddChangeMonitor(this);
            }
        }

        private void ReleaseMonitor(Locator locator)
        {
            // monitor changes
            if ((locator.ICore is ISizable _sizer)
                && _SizeCtrlrs.ContainsKey(_sizer.Sizer))
            {
                _SizeCtrlrs.Remove(_sizer.Sizer);
                _sizer.Sizer.RemoveChangeMonitor(this);
            }
        }

        private void ReleaseAllMonitoring()
        {
            // just in case: removing zone should clean these all up anyway
            foreach (var _kvp in _SizeCtrlrs.ToList())
            {
                ReleaseMonitor(_kvp.Value);
            }
        }
        #endregion

        private void TestTrigger(Locator locator)
        {
            var _mech = ProximityTriggerGroup.Master?.TriggerMechanism;
            if (_mech != null)
            {
                if ((locator?.ICore is ISizable _sizer)
                    && _sizer.Sizer.Size.Order >= _mech.MinimumSize)
                {
                    _mech.DoTrigger(locator.ToEnumerable());
                }
            }
        }

        // ILocatorZone
        public void Start(Locator locator)
            => MonitorSize(locator);

        public void End(Locator locator)
            => ReleaseMonitor(locator);

        public void Capture(Locator locator)
            => MonitorSize(locator);

        public void Release(Locator locator)
            => ReleaseMonitor(locator);

        public void Enter(Locator locator)
        {
            MonitorSize(locator);
            TestTrigger(locator);
        }

        public void Exit(Locator locator)
        {
            TestTrigger(locator);
            ReleaseMonitor(locator);
        }

        public void MoveInArea(Locator locator, bool followOn)
        {
            MonitorSize(locator);
            TestTrigger(locator);
        }

        // IMonitorChange<Size>
        public void PreTestChange(object sender, AbortableChangeEventArgs<Size> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<Size> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<Size> args)
        {
            if ((sender is Sizer _sizer)
                && _SizeCtrlrs.ContainsKey(_sizer))
            {
                TestTrigger(_SizeCtrlrs[_sizer]);
            }
        }
    }
}
