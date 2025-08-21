using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public abstract class AnchorableObject : ObjectBase, IAnchorage
    {
        // TODO: use for portalledObjectBase also
        protected AnchorableObject(string name, Material material)
            : base(name, material)
        {
            StructurePoints = MaxStructurePoints;
            _Connected = [];
            _COCtrl = new ChangeController<ICoreObject>(this, null);
        }

        #region state
        private List<ICoreObject> _Connected;
        private ChangeController<ICoreObject> _COCtrl;
        private double _TareWeight;
        #endregion

        public override IEnumerable<ICoreObject> Connected
            => _Connected.Select(_c => _c);

        #region public virtual int StructurePoints {get; set;}
        public override int StructurePoints
        {
            get => _StrucPts;
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

        // IAnchorage Members
        public virtual bool CanAcceptAnchor(IAdjunctable newAnchor)
            => (newAnchor is ICoreObject _core)
            && !_Connected.Contains(_core) && !_COCtrl.WillAbortChange(_core, @"Add");

        public virtual bool CanEjectAnchor(IAdjunctable existingAnchor)
            => (existingAnchor is ICoreObject _core)
            && _Connected.Contains(_core) && !_COCtrl.WillAbortChange(_core, @"Remove");

        public IEnumerable<ICoreObject> Anchored
            => _Connected.Select(_c => _c);

        #region public void AcceptAnchor(IAdjunctable newAnchor)
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
        #endregion

        #region public void EjectAnchor(IAdjunctable existingAnchor)
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

        // IControlChange<CoreObject> Members
        public void AddChangeMonitor(IMonitorChange<ICoreObject> monitor)
            => _COCtrl.AddChangeMonitor(monitor);

        public void RemoveChangeMonitor(IMonitorChange<ICoreObject> monitor)
            =>
            _COCtrl.RemoveChangeMonitor(monitor);

        // ILoadedObjects Members
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

        // IMonitorChange<Physical> Members
        public virtual void PreTestChange(object sender, AbortableChangeEventArgs<Physical> args)
        {
        }

        public virtual void PreValueChanged(object sender, ChangeValueEventArgs<Physical> args)
        {
        }

        public virtual void ValueChanged(object sender, ChangeValueEventArgs<Physical> args)
        {
            if (args.NewValue.PropertyType == Physical.PhysicalType.Weight)
            {
                RecalcWeight();
            }
        }

        // etc ...
        public override Info MergeConnectedInfos(Info fetchedInfo, CoreActor actor)
            => fetchedInfo;

        #region public override IEnumerable<string> IconKeys { get; }
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
        #endregion
    }
}
