using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Items.Materials;
using Uzi.Core;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Packaging;
using Uzi.Visualize;
using System.Diagnostics;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Objects
{
    /// <summary>
    /// Provides basic object containment.  May provide containment actions.
    /// </summary>
    [Serializable]
    public class ContainerObject : ObjectBase, IObjectContainer, ICorePart, ITacticalActionProvider
    {
        #region Construction
        public ContainerObject(string name, Material objectMaterial, bool actions, bool isContextMenuOnly)
            : base(name, objectMaterial)
        {
            _BaseObjCtrl = new ChangeController<ICoreObject>(this, null);
            _Objs = [];
            _AddsToLoad = true;
            if (actions)
            {
                AddAdjunct(new Containment(this));
            }

            _CtxOnly = isContextMenuOnly;
        }
        #endregion

        #region data
        private double _TareWeight;
        private double _MaxLoadWeight;
        protected bool _AddsToLoad;
        protected bool _CtxOnly;
        private ChangeController<ICoreObject> _BaseObjCtrl;
        private Collection<ICoreObject> _Objs;
        #endregion

        // IActionProvider Members
        protected IEnumerable<CoreAction> ContainerObjectActions(CoreActionBudget budget)
            => Adjuncts.GetActions(budget);

        public virtual IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
            => ContainerObjectActions(budget);

        public override Info MergeConnectedInfos(Info fetchedInfo, CoreActor actor)
            => fetchedInfo;

        public override IGeometricSize GeometricSize => Sizer.Size.CubeSize();

        public bool IsContextMenuOnly => _CtxOnly;

        #region ICollection<ICoreObject> Members
        public virtual void Add(ICoreObject item)
        {
            if (!_Objs.Contains(item)
                && !_BaseObjCtrl.WillAbortChange(item, @"Add")
                && ((item.Weight + LoadWeight) <= MaximumLoadWeight))
            {
                Debug.WriteLine($@"ContainerObject.Add({item.Name}) --> ALLOWED");
                _BaseObjCtrl.DoPreValueChanged(item, @"Add");
                _Objs.Add(item);
                item.AddAdjunct(new Contained(this));

                // track weight
                if (ContentsAddToLoad)
                {
                    item.AddChangeMonitor(this);
                }

                RecalcWeight();

                _BaseObjCtrl.DoValueChanged(item, @"Add");
                DoPropertyChanged(nameof(Connected));
                this.IncreaseSerialState();
            }
            else
            {
                Debug.WriteLine($@"ContainerObject.Add({item.Name}) --> BLOCKED");
            }
        }

        /// <summary>Should only use this internally, it isn't checking change controllers</summary>
        public bool Contains(ICoreObject item) { return _Objs.Contains(item); }
        public int Count { get { return _Objs.Count; } }

        /// <summary>Removes object, unbinds weight checking, removes Contained adjunct and recalcs weight</summary>
        public virtual bool Remove(ICoreObject item)
        {
            if (_Objs.Contains(item)
                && !_BaseObjCtrl.WillAbortChange(item, @"Remove"))
            {
                _BaseObjCtrl.DoPreValueChanged(item, @"Remove");
                _Objs.Remove(item);
                item.GetContained()?.Eject();

                // untrack weight
                if (ContentsAddToLoad)
                {
                    item.RemoveChangeMonitor(this);
                }

                RecalcWeight();

                _BaseObjCtrl.DoValueChanged(item, @"Remove");
                DoPropertyChanged(nameof(Connected));
                this.IncreaseSerialState();
                return true;
            }
            return false;
        }
        #endregion

        public IEnumerable<ICoreObject> Objects
            => _Objs.Select(_o => _o);

        #region IControlChange<ICoreObject> Members
        public void AddChangeMonitor(IMonitorChange<ICoreObject> monitor) { _BaseObjCtrl.AddChangeMonitor(monitor); }
        public void RemoveChangeMonitor(IMonitorChange<ICoreObject> monitor) { _BaseObjCtrl.RemoveChangeMonitor(monitor); }
        #endregion

        #region IObjectContainer Members
        public virtual double MaximumLoadWeight
        {
            get { return _MaxLoadWeight; }
            set
            {
                _MaxLoadWeight = value;
                DoPropertyChanged(nameof(MaximumLoadWeight));
            }
        }

        private void RecalcWeight() { Weight = _TareWeight + (ContentsAddToLoad ? LoadWeight : 0); }

        public virtual double TareWeight
        {
            get { return _TareWeight; }
            set
            {
                _TareWeight = value;
                RecalcWeight();
                DoPropertyChanged(nameof(TareWeight));
            }
        }

        public double LoadWeight
            => _Objs.Select(bo => bo.Weight).Sum();

        public virtual bool AddsToLoad { get => _AddsToLoad; set => _AddsToLoad = value; }
        public bool ContentsAddToLoad => _AddsToLoad;

        /// <summary>Recursively drill into contents and containers.  If the contents do not add to load, these do not appear.</summary>
        public IEnumerable<ICoreObject> AllLoadedObjects()
        {
            if (ContentsAddToLoad)
            {
                foreach (var _cObj in _Objs)
                {
                    yield return _cObj;
                }

                foreach (var _loaded in from _contain in _Objs.OfType<ILoadedObjects>()
                                        from _iC in _contain.AllLoadedObjects()
                                        select _iC)
                {
                    yield return _loaded;
                }
            }
            yield break;
        }
        #endregion

        /// <summary>CoreConnected objects and objects in the containment repository (directly)</summary>
        public override IEnumerable<ICoreObject> Connected
            => _Objs.Union(base.Connected);

        public bool CanHold(ICoreObject obj)
            => (!_Objs.Contains(obj)
            && ((StructurePoints > 0) || this.GetObjectBindings().Any())
            && !_BaseObjCtrl.WillAbortChange(obj, @"Add")
            && ((obj.Weight + LoadWeight) <= MaximumLoadWeight));

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
                var _load = _TareWeight + (ContentsAddToLoad ? LoadWeight : 0);

                // if the weight is now above the maximum load weight, the item that changed weight must be ejected
                if (_load > MaximumLoadWeight)
                {
                    if (sender is IObjectBase _object)
                    {
                        Remove(_object);

                        // if ejected from a container that is contained, try to place it in an outer container
                        var _newHolder = _object.FindAcceptableContainer(this);
                        if (_newHolder != null)
                        {
                            // move object to new container
                            _newHolder.Add(_object);
                            return;
                        }

                        // add object to locator (or create locator)
                        var _top = this.FindTopContainer();
                        if (_top != null)
                        {   // if ejected from an uncontained container
                            var _locator = Locator.FindFirstLocator(_top);
                            // ... or create a new locator (object presenter?)
                            var _location = new CellLocation(_locator.Location);
                            var _drop = new Drop(null, _locator?.Map, _location, true);
                            var _iAct = new Interaction(null, this, _object, _drop);
                            _object.HandleInteraction(_iAct);
                        }
                    }
                }
                else
                {
                    RecalcWeight();
                }
            }
        }
        #endregion

        #region ICorePart Members
        public IEnumerable<ICorePart> Relationships
        {
            get { yield break; }
        }

        public string TypeName
            => GetType().FullName;
        #endregion

        // IActionSource Members
        public IVolatileValue ActionClassLevel
            => new Deltable(1);

        // ITacticalActionProvider Members
        public virtual IEnumerable<CoreAction> GetTacticalActions(CoreActionBudget budget)
            => ContainerObjectActions(budget);

        public override Guid PresenterID
            => (this.GetObjectBindings().FirstOrDefault() as IAnchorage)?.ID ?? ID;

        protected override string ClassIconKey
            => nameof(ContainerObject);

        public override IEnumerable<string> IconKeys
        {
            get
            {
                var _parent = this.GetObjectBindings().FirstOrDefault();
                if (_parent != null)
                {
                    return _parent.IconKeys;
                }
                return base.IconKeys;
            }
        }

        public override bool IsTargetable => true;

        public override int StructurePoints
        {
            get => base.StructurePoints;
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
                    foreach (var _obj in _Objs.ToList())
                    {
                        Remove(_obj);
                        var _next = _obj.FindAcceptableContainer(this);
                        if (_next != null)
                        {
                            _next.Add(_obj);
                        }
                        else
                        {
                            Drop.DoDropEject(this, _obj);
                        }
                    }
                    this.UnPath();
                    this.UnGroup();
                }
            }
        }
    }
}