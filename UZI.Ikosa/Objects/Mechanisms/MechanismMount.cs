using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class MechanismMount : LocatableObject, IAnchorage, IActionProvider
    {
        public MechanismMount(string name)
            : base(name, true)
        {
            _Connected = [];
            _COCtrl = new ChangeController<ICoreObject>(this, null);
            _GeometricSize = new GeometricSize(1, 1, 1);
            _MountFace = AnchorFace.ZLow;
            Length = 1;
            Height = 1;
            Width = 1;
        }

        #region data
        private List<ICoreObject> _Connected;
        private ChangeController<ICoreObject> _COCtrl;
        private GeometricSize _GeometricSize;
        private double _ZOff;
        private double _YOff;
        private double _XOff;
        private int _Pivot;
        private AnchorFace _MountFace;
        #endregion

        protected override void InitInteractionHandlers()
        {
            AddIInteractHandler(new MechanismMountObserveHandler());
            AddIInteractHandler(new MechanismMountVisualHandler());
            base.InitInteractionHandlers();
        }

        public override Info GetInfo(CoreActor actor, bool baseValues)
            => null;

        public override Info MergeConnectedInfos(Info fetchedInfo, CoreActor actor)
            => fetchedInfo;

        #region public void SetMountSize(IGeometricSize size)
        public void SetMountSize(IGeometricSize size)
        {
            _GeometricSize = new Visualize.GeometricSize(size);

            // alter locator when size changes
            var _loc = this.GetLocated()?.Locator;
            if (_loc != null)
            {
                // relocate if size changed
                var _geom = _loc.GeometricRegion;
                var _region = new Cubic(new CellPosition(_geom.LowerZ, _geom.LowerY, _geom.LowerX), _GeometricSize);
                _loc.Relocate(_region, _loc.PlanarPresence);
            }
        }
        #endregion

        public override IGeometricSize GeometricSize
            => new GeometricSize(_GeometricSize);

        public override Sizer Sizer
            => new ObjectSizer(Size.Medium, this);

        public AnchorFace MountFace
        {
            get => _MountFace;
            set
            {
                _MountFace = value;
                DoPropertyChanged(nameof(MountFace));
                SetMountSize(GeometricSize);
            }
        }

        public double ZOffset
        {
            get => _ZOff;
            set
            {
                _ZOff = value;
                DoPropertyChanged(nameof(ZOffset));
                SetMountSize(GeometricSize);
            }
        }

        public double YOffset
        {
            get => _YOff;
            set
            {
                _YOff = value;
                DoPropertyChanged(nameof(YOffset));
                SetMountSize(GeometricSize);
            }
        }

        public double XOffset
        {
            get => _XOff;
            set
            {
                _XOff = value;
                DoPropertyChanged(nameof(XOffset));
                SetMountSize(GeometricSize);
            }
        }

        public int Pivot
        {
            get => _Pivot;
            set
            {
                _Pivot = (value % 8);
                DoPropertyChanged(nameof(Pivot));
                SetMountSize(GeometricSize);
            }
        }

        protected override string ClassIconKey
            => nameof(MechanismMount);

        public override bool IsTargetable => false;

        public override IEnumerable<ICoreObject> Connected
            => _Connected.Select(_c => _c);

        public IEnumerable<ICoreObject> Anchored
            => _Connected.Select(_c => _c);

        public Guid PresenterID => ID;

        #region public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            var _budget = budget as LocalActionBudget;

            // mechanism actions...
            foreach (var _action in this.AccessibleActions(_budget))
            {
                yield return _action;
            }

            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => GetInfoData.GetInfoFeedback(this, budget.Actor);

        #endregion

        #region IAnchorage Members
        public bool CanAcceptAnchor(IAdjunctable newAnchor)
            => (newAnchor is ICoreObject _core)
            && !_Connected.Contains(_core) && !_COCtrl.WillAbortChange(_core, @"Add");

        public bool CanEjectAnchor(IAdjunctable existingAnchor)
            => (existingAnchor is ICoreObject _core)
            && _Connected.Contains(_core) && !_COCtrl.WillAbortChange(_core, @"Remove");

        public void AcceptAnchor(IAdjunctable newAnchor)
        {
            if (newAnchor is ICoreObject _core)
            {
                if (CanAcceptAnchor(newAnchor))
                {
                    _COCtrl.DoPreValueChanged(_core, @"Add");
                    _Connected.Add(_core);

                    // track weight
                    _core.AddChangeMonitor(this);

                    _COCtrl.DoValueChanged(_core, @"Add");
                    DoPropertyChanged(nameof(Connected));
                    DoPropertyChanged(nameof(Anchored));
                }
            }
        }

        public void EjectAnchor(IAdjunctable existingAnchor)
        {
            if (existingAnchor is ICoreObject _core)
            {
                if (CanEjectAnchor(existingAnchor))
                {
                    _COCtrl.DoPreValueChanged(_core, @"Remove");
                    _Connected.Remove(_core);

                    // untrack weight
                    _core.RemoveChangeMonitor(this);

                    _COCtrl.DoValueChanged(_core, @"Remove");
                    DoPropertyChanged(nameof(Connected));
                    DoPropertyChanged(nameof(Anchored));
                }
            }
        }
        #endregion

        #region IControlChange<CoreObject> Members
        public void AddChangeMonitor(IMonitorChange<ICoreObject> monitor)
        {
            _COCtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<ICoreObject> monitor)
        {
            _COCtrl.RemoveChangeMonitor(monitor);
        }
        #endregion

        #region ILoadedObjects Members
        public IEnumerable<ICoreObject> AllLoadedObjects()
            => AllConnected(null);

        public bool ContentsAddToLoad
            => true;

        private void RecalcWeight() { Weight = (ContentsAddToLoad ? LoadWeight : 0); }

        public double TareWeight
        {
            get => 0;
            set => RecalcWeight();
        }

        public double LoadWeight
            => _Connected.Sum(bo => bo.Weight);
        #endregion

        #region IMonitorChange<Physical> Members

        public void PreTestChange(object sender, AbortableChangeEventArgs<Physical> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<Physical> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<Physical> args)
        {
            if (args.NewValue.PropertyType == Physical.PhysicalType.Weight)
            {
                RecalcWeight();
            }
        }

        #endregion
    }
}
