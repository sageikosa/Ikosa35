using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using System.Collections.ObjectModel;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Items
{
    /// <summary>
    /// Represents a generalized container item that is expected to take up an item slot.
    /// ItemSizer is setup to handle container.
    /// </summary>
    [Serializable]
    public abstract class SlottedContainerItemBase : SlottedItemBase, IOpenable, IAnchorage, ITacticalActionProvider
    {
        #region Construction
        protected SlottedContainerItemBase(string name, string slotType, ContainerObject container, bool blocksLight)
            : base(name, slotType)
        {
            _OpenState = this.GetOpenStatus(null, this, 0);
            _OCtrl = new ChangeController<OpenStatus>(this, _OpenState);
            _Connected = [];
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

            // sizer can control maximum weight
            ItemSizer.Containers.Add(container);
        }
        #endregion

        #region Private Data
        private OpenStatus _OpenState;
        private double _OpeningWeight = 0d;
        private ContainerObject _Container;
        private Collection<ICoreObject> _Connected;
        private ChangeController<ICoreObject> _COCtrl;
        private FindAdjunctDataExcluderHandler _LightBlocker = null;
        #endregion

        public bool BlocksLight { get { return _LightBlocker != null; } }

        /// <summary>The container within...</summary>
        public ContainerObject Container { get { return _Container; } }

        /// <summary>Directly connected objects</summary>
        public override IEnumerable<ICoreObject> Connected
        {
            get
            {
                foreach (var _obj in _Connected)
                {
                    yield return _obj;
                }

                yield break;
            }
        }

        #region public override IEnumerable<ICoreObject> Accessible(ICoreObject principal)
        public override IEnumerable<ICoreObject> Accessible(ICoreObject principal)
        {
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
                    foreach (var _obj in _Connected)
                    {
                        if (_obj != _Container)
                        {
                            // the direct object itself
                            yield return _obj;
                        }
                    }
                }
            }
            else
            {
                // everything inside and out if it is open (all connected stuff and recursive contents)
                foreach (var _core in base.Accessible(principal))
                {
                    yield return _core;
                }
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
                {
                    yield return _obj;
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
        {
            // anchored objects are part of load, the container's contents and the container itself
            foreach (var _core in _Connected.AsEnumerable().Union(_Container.AllLoadedObjects()))
            {
                yield return _core;
            }

            yield return Container;
            yield break;
        }

        public bool ContentsAddToLoad { get { return true; } }

        /// <summary>Weight calculation + LoadWeight if necessary.  Setting to anything will recalculate.</summary>
        public override double Weight
        {
            get { return base.Weight; }
            set
            {
                base.Weight = 0;
                if (ContentsAddToLoad)
                {
                    CoreSetWeight(base.Weight + LoadWeight);
                }

                DoPropertyChanged(@"OpenWeight");
            }
        }

        /// <summary>Implemented as alias for BaseWeight</summary>
        public double TareWeight
        {
            get { return BaseWeight; }
            set
            {
                BaseWeight = value;
                Weight = 0;
                DoPropertyChanged(@"TareWeight");
            }
        }

        public double LoadWeight { get { return _Connected.Sum(bo => bo.Weight); } }
        #endregion

        #region IActionProvider Members
        public virtual IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            var _budget = budget as LocalActionBudget;
            return this.AccessibleActions(_budget).Union(GetTacticalActions(budget));
        }

        public bool IsContextMenuOnly => false;

        public Info GetProviderInfo(CoreActionBudget budget)
        {
            return GetInfoData.GetInfoFeedback(this, budget.Actor);
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
                Weight = 0;
            }
        }

        #endregion

        #region ITacticalActionProvider Members

        public IEnumerable<CoreAction> GetTacticalActions(CoreActionBudget budget)
        {
            var _budget = budget as LocalActionBudget;

            // openable
            if (_budget.CanPerformBrief)
            {
                yield return new OpenCloseAction(this, this, @"101");
            }

            yield break;
        }

        #endregion
    }
}
