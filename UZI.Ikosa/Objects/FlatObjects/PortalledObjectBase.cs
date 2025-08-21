using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Tactical;
using Uzi.Core;
using System.Collections.ObjectModel;
using Uzi.Visualize;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public abstract class PortalledObjectBase : ObjectBase, IFlatObjectSide
    {
        // TODO: use AnchorableObject as base

        #region ctor()
        protected PortalledObjectBase(string name, Material objectMaterial, double thickness)
            : base(name, objectMaterial)
        {
            _Thickness = thickness;
            MaxStructurePoints = Convert.ToInt32(Convert.ToDouble(objectMaterial.StructurePerInch) * 12d / thickness);
            StructurePoints = MaxStructurePoints;
            _Connected = [];
            _COCtrl = new ChangeController<ICoreObject>(this, null);
        }
        #endregion

        #region data
        private Collection<ICoreObject> _Connected;
        private double _Width;
        private double _Height;
        private double _Thickness;
        private ChangeController<ICoreObject> _COCtrl;
        private double _TareWeight;
        #endregion

        public void SetName(string name)
            => Name = name;

        public override double Width { get => _Width; set { _Width = value; } }
        public override double Height { get => _Height; set { _Height = value; } }
        public double Thickness { get => _Thickness; set { _Thickness = value; } }

        public override IEnumerable<ICoreObject> Connected
            => _Connected.Select(_c => _c);

        public override IGeometricSize GeometricSize => Sizer.Size.CubeSize();

        public override bool IsTargetable => false;

        public bool IsContextMenuOnly => true;

        public virtual string SoundDescription => GetType().Name.ToLower();

        public void SetMaxStructurePoints(int maxPts)
            => DoSetMaxStructurePoints(maxPts);

        #region public virtual int StructurePoints {get; set;}
        public override int StructurePoints
        {
            get
            {
                return _StrucPts;
            }
            set
            {
                if (_StrucPts <= _MaxStrucPts)
                {
                    _StrucPts = value;
                }
                else
                {
                    _StrucPts = _MaxStrucPts;
                }

                DoPropertyChanged(nameof(StructurePoints));

                if (_StrucPts <= 0)
                {
                    this.DoDestruction();
                    this.UnPath();
                    this.UnGroup();
                }
            }
        }
        #endregion

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

        public IEnumerable<ICoreObject> Anchored
        {
            get
            {
                foreach (var _c in _Connected)
                {
                    yield return _c;
                }

                yield break;
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

        private void RecalcWeight() { Weight = _TareWeight + (ContentsAddToLoad ? LoadWeight : 0); }

        public double TareWeight
        {
            get => _TareWeight;
            set
            {
                _TareWeight = value;
                RecalcWeight();
            }
        }

        public double LoadWeight
            => _Connected.Sum(bo => bo.Weight);
        #endregion

        public override Info MergeConnectedInfos(Info fetchedInfo, CoreActor actor)
            => fetchedInfo;

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

        public abstract object Clone();

        public override IEnumerable<string> IconKeys
        {
            get
            {
                // provide any overrides
                foreach (var _iKey in IconKeyAdjunct.GetIconKeys(this))
                {
                    yield return _iKey;
                }

                // material class combination
                yield return $@"{ObjectMaterial?.Name}_{ClassIconKey}";

                // ... and then the class key
                yield return ClassIconKey;
                yield break;
            }
        }
    }
}
