using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Materials;
using System.Collections.ObjectModel;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Packaging;
using Uzi.Visualize;
using Uzi.Ikosa.Senses;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Objects
{
    /// <summary>
    /// Represents an object container that can be closed.
    /// </summary>
    [Serializable]
    public class CloseableContainerObject : ObjectBase, IOpenable, IAnchorage, ITacticalActionProvider, ICorePart, IActionSource
    {
        #region Construction
        public CloseableContainerObject(string name, Material material, ContainerObject container, bool blocksLight, int openNumber)
            : base(name, material)
        {
            _OpenNumber = openNumber;
            _OpenState = this.GetOpenStatus(null, this, 0);
            _OCtrl = new ChangeController<OpenStatus>(this, _OpenState);
            _Connected = new Collection<ICoreObject>();
            _COCtrl = new ChangeController<ICoreObject>(this, null);

            // anchor container to this...
            _Container = container;
            _Container.BindToObject(this);
            if (blocksLight)
            {
                // this will control light propagation
                _LightBlocker = new FindAdjunctDataExcluderHandler();
                _Container.AddIInteractHandler(_LightBlocker);
                _LightBlocker.Exclusions.Add(typeof(Illumination)); // start off closed
                _LightBlocker.Exclusions.Add(typeof(MagicDark));
            }

            // sizer can affect maximum load weight
            ObjectSizer.Containers.Add(container);
        }
        #endregion

        #region data
        private int _OpenNumber;
        private OpenStatus _OpenState;
        private double _OpeningWeight = 0d;
        private ContainerObject _Container;
        private Collection<ICoreObject> _Connected;
        private ChangeController<ICoreObject> _COCtrl;
        private FindAdjunctDataExcluderHandler _LightBlocker = null;
        #endregion

        protected override void InitInteractionHandlers()
        {
            AddIInteractHandler(new CloseableContainerHandler());
            base.InitInteractionHandlers();
        }

        public bool BlocksLight
            => _LightBlocker != null;

        public override IGeometricSize GeometricSize => Sizer.Size.CubeSize();

        /// <summary>The container within...</summary>
        public ContainerObject Container => _Container;

        public bool IsContextMenuOnly => true;

        public int OpenNumber => _OpenNumber;

        /// <summary>Directly connected objects</summary>
        public override IEnumerable<ICoreObject> Connected
            => _Connected.Select(_c => _c);

        #region public override IEnumerable<ICoreObject> Accessible(ICoreObject principal)
        public override IEnumerable<ICoreObject> Accessible(ICoreObject principal)
        {
            // this is for recursive loop detection, since no hierarchical integrity is enforced
            if (OpenState.IsClosed)
            {
                // see if the actor is somewhere in the container...odd, but possible
                if (Container.Connected.Contains(principal))
                {
                    // NOTE: this only checks for direct inclusion, if actor is nested deep, this won't find him!
                    // assume access the container (though nested containers may also check for inclusion
                    yield return Container;
                }
                else
                {
                    // only the anchored things on the surface (exclude anchored container)
                    foreach (var _obj in _Connected.Where(_o => _o != _Container))
                    {
                        yield return _obj;
                    }
                }
            }
            else
            {
                // everything inside and out if it is open (all connected stuff and recursive contents)
                foreach (var _core in base.Accessible(principal))
                    yield return _core;
            }
        }
        #endregion

        #region IOpenable Members

        #region IControlChange<OpenStatus> Members
        private ChangeController<OpenStatus> _OCtrl;
        public void AddChangeMonitor(IMonitorChange<OpenStatus> monitor)
        {
            _OCtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<OpenStatus> monitor)
        {
            _OCtrl.RemoveChangeMonitor(monitor);
        }
        #endregion

        protected virtual void OnOpenState()
        {
            // NOTE: possibly search for light in the container before forcing a re-light?
            var _loc = Locator.FindFirstLocator(this);
            if (_loc != null && BlocksLight)
            {
                var _groups = _loc.GetLocalCellGroups().ToList();

                var _keys = new HashSet<RoomTrackerKey>(_groups.SelectMany(_g => RoomTrackerKey.GetKeys(_g)).Distinct());
                foreach (var _z in (from _z in _loc.MapContext.LocatorZones.AllCaptures()
                                    where RoomTrackerKey.GetKeys(_z.Geometry.Region).Any(_k => _keys.Contains(_k))
                                    select _z).ToList())
                {
                    _z.Geometry.RecalcGeometry();
                }

                var _notifiers = _groups
                    .SelectMany(_g => _g.NotifyLighting())
                    .Distinct()
                    .ToList();

                AwarenessSet.RecalculateAllSensors(_loc.Map, _notifiers, false);
            }
        }

        public bool CanChangeOpenState(OpenStatus testValue)
            => !_OCtrl.WillAbortChange(testValue);

        public OpenStatus OpenState
        {
            get { return _OpenState; }
            set
            {
                if (!_OCtrl.WillAbortChange(value))
                {
                    _OCtrl.DoPreValueChanged(value);
                    _OpenState = value;
                    if (BlocksLight)
                    {
                        if (_OpenState.IsClosed && !_LightBlocker.Exclusions.Contains(typeof(Illumination)))
                        {
                            _LightBlocker.Exclusions.Add(typeof(Illumination));
                            _LightBlocker.Exclusions.Add(typeof(MagicDark));
                        }
                        else if (!_OpenState.IsClosed)
                        {
                            _LightBlocker.Exclusions.Remove(typeof(Illumination));
                            _LightBlocker.Exclusions.Remove(typeof(MagicDark));
                        }
                    }
                    _OCtrl.DoValueChanged(value);
                    DoPropertyChanged(nameof(OpenState));
                    OnOpenState();
                }
            }
        }

        public double OpenWeight
        {
            get { return _OpeningWeight; }
            set
            {
                _OpeningWeight = value;
                DoPropertyChanged(@"OpenWeight");
            }
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

        /// <summary>Directly connected objects (except for the wrapped container)</summary>
        public IEnumerable<ICoreObject> Anchored
        {
            get
            {
                foreach (var _obj in Connected.Where(_o => !_o.Equals(_Container)))
                    yield return _obj;
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
        {
            // anchored objects are part of load, the container's contents and the container itself
            foreach (var _core in _Connected.AsEnumerable().Union(_Container.AllLoadedObjects()))
                yield return _core;
            yield return Container;
            yield break;
        }

        public bool ContentsAddToLoad
            => true;

        private void RecalcWeight() { Weight = _TareWeight + (ContentsAddToLoad ? LoadWeight : 0); }

        private double _TareWeight;
        public double TareWeight
        {
            get { return _TareWeight; }
            set
            {
                _TareWeight = value;
                RecalcWeight();
            }
        }

        public double LoadWeight
            => _Connected.Sum(bo => bo.Weight);
        #endregion

        #region IActionProvider Members

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            // accessible actions...
            var _budget = budget as LocalActionBudget;
            return this.AccessibleActions(_budget).Union(GetTacticalActions(budget));
        }

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

        #region ITacticalActionProvider Members

        public IEnumerable<CoreAction> GetTacticalActions(CoreActionBudget budget)
        {
            var _budget = budget as LocalActionBudget;

            // openable
            if (_budget?.CanPerformBrief ?? false)
                yield return new OpenCloseAction(this, this, @"101");
            yield break;
        }

        #endregion

        protected override string ClassIconKey
            => nameof(CloseableContainerObject);

        public override bool IsTargetable => true;

        public override Info MergeConnectedInfos(Info fetchedInfo, CoreActor actor)
            => fetchedInfo;
    }

    [Serializable]
    public class CloseableContainerHandler : IProcessFeedback
    {
        public CloseableContainerHandler()
        {
        }

        #region IProcessFeedback Members

        public void ProcessFeedback(Interaction workSet)
        {
            if (workSet.InteractData is VisualPresentationData)
            {
                if (workSet.Target is CloseableContainerObject _container)
                {
                    foreach (var _vmBack in workSet.Feedback.OfType<VisualModelFeedback>())
                    {
                        _vmBack.ModelPresentation.ExternalValues[$@"Open.{_container.OpenNumber}"]
                            = _container.OpenState.IsClosed ? 0 : 1;
                    }
                }
            }
        }

        #endregion

        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(VisualPresentationData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return true;
        }

        #endregion
    }

    [Serializable]
    public class CloseableContainerBinder : ObjectBound, IProcessFeedback
    {
        public CloseableContainerBinder(IAnchorage source)
            : base(source)
        {
        }

        public CloseableContainerObject CloseableContainerObject => Anchor as CloseableContainerObject;

        protected override void OnActivate(object source)
        {
            (Anchorage as ICoreObject)?.AddIInteractHandler(this);
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            (Anchorage as ICoreObject)?.RemoveIInteractHandler(this);
            base.OnDeactivate(source);
        }

        public override object Clone()
            => new CloseableContainerBinder(Anchorage);

        #region IProcessFeedback Members

        public void ProcessFeedback(Interaction workSet)
        {
            var _container = CloseableContainerObject;
            if (_container != null)
            {
                if (workSet.InteractData is VisualPresentationData)
                {
                    var _target = workSet.Target as IAdjunctable;
                    if (_container.GetObjectBoundAnchorages().Contains(workSet.Target as IAdjunctable))
                    {
                        foreach (var _vmBack in workSet.Feedback.OfType<VisualModelFeedback>())
                        {
                            _vmBack.ModelPresentation.ExternalValues[$@"Open.{_container.OpenNumber}"]
                                = _container.OpenState.IsClosed ? 0 : 1;
                        }
                    }
                }
            }
        }

        #endregion

        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(VisualPresentationData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return true;
        }

        #endregion
    }
}