using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public abstract class Conveyance : ObjectBase, IAnchorage, IActionSource,
        ITacticalActionProvider, IAlternateRelocate, ICloneable
    {
        protected Conveyance(string name, Material objectMaterial)
            : base(name, objectMaterial)
        {
            _Orient = new ConveyanceOrientation(this);
            _Connected = new List<ICoreObject>();
            _COCtrl = new ChangeController<ICoreObject>(this, null);
        }

        protected override void InitInteractionHandlers()
        {
            AddIInteractHandler(new ConveyanceVisualHandler());
            AddIInteractHandler(new ConveyanceObserveHandler());
            base.InitInteractionHandlers();
        }

        // NOTE: basic idea: borrow furnishing concepts, design constrain to single cell
        // NOTE: might come back to furnishing and back-port/abstract ideas

        #region data
        // anchorage
        private double _TareWeight;
        private List<ICoreObject> _Connected;
        private ChangeController<ICoreObject> _COCtrl;

        private ConveyanceOrientation _Orient;
        #endregion

        #region clone support
        protected void CopyFrom(Conveyance copyFrom)
        {
            // CoreObject physical
            Weight = copyFrom.Weight;
            Length = copyFrom.Length;
            Width = copyFrom.Width;
            Height = copyFrom.Height;

            // furnishing
            TareWeight = copyFrom.TareWeight;
            MaxStructurePoints = copyFrom.MaxStructurePoints;
            StructurePoints = copyFrom.StructurePoints;
            Masterwork = copyFrom.Masterwork;
            ExtraSoundDifficulty.BaseValue = copyFrom.ExtraSoundDifficulty.BaseValue;
            DoesSupplyConcealment = copyFrom.DoesSupplyConcealment;
            DoesSupplyTotalConcealment = copyFrom.DoesSupplyTotalConcealment;
            DoesBlocksLineOfEffect = copyFrom.DoesBlocksLineOfEffect;
            DoesBlocksLineOfDetect = copyFrom.DoesBlocksLineOfDetect;
            BlocksMove = copyFrom.BlocksMove;
            DoesHindersMove = copyFrom.DoesHindersMove;
            DoesBlocksSpread = copyFrom.DoesBlocksSpread;

            // TODO: compartments and other mechanisms?
        }

        public abstract object Clone();
        #endregion

        /// <summary>Directly connected objects</summary>
        public override IEnumerable<ICoreObject> Connected
            => _Connected.Select(_c => _c);

        public ConveyanceOrientation Orientation => _Orient;
        public override bool IsTargetable => true;

        public ObjectPresenter ObjectPresenter
            => (ObjectPresenter)this.GetLocated()?.Locator;

        // IAlternateRelocate
        #region public (Cubic Cube, Vector3D Offset) GetRelocation(IGeometricRegion region)
        public (Cubic Cube, System.Windows.Media.Media3D.Vector3D Offset) GetRelocation(IGeometricRegion region, Locator locator)
        {
            // immobile?
            if (this.HasActiveAdjunct<Immobile>())
            {
                // cannot move if immobile
                return (null, default);
            }

            var _vol = new ConveyanceVolume(this);
            var _size = Orientation.SnappableSize;

            // if we don't have a cubic, make a cubic from the region
            if (!(region is Cubic))
                region = region.ContainingCube(region);

            var _fit = _vol.GetCubicFit(region as Cubic, _size);
            if (_fit.Cube == null)
            {
                var _region = locator.GeometricRegion;
                if (_region != null)
                {
                    var _curr = new GeometricSize(_region);
                    if (!_curr.SameSize(_size))
                    {
                        var _off = locator.IntraModelOffset;
                        var _faces = AnchorFaceList.None;
                        if (_off.Z < -3.5) _faces = _faces.Add(AnchorFace.ZLow);
                        if (_off.Y < -3.5) _faces = _faces.Add(AnchorFace.YLow);
                        if (_off.X < -3.5) _faces = _faces.Add(AnchorFace.XLow);
                        if (_off.Z > 3.5) _faces = _faces.Add(AnchorFace.ZHigh);
                        if (_off.Y > 3.5) _faces = _faces.Add(AnchorFace.YHigh);
                        if (_off.X > 3.5) _faces = _faces.Add(AnchorFace.XHigh);
                        var _newCube = (region as Cubic).OffsetCubic(_faces.ToAnchorFaces().ToArray());
                        _fit = _vol.GetCubicFit(_newCube, _size);
                    }
                }
            }
            return _fit;
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
            => _Connected.Select(_c => _c);
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

        private void RecalcWeight()
            => Weight = _TareWeight + (ContentsAddToLoad ? LoadWeight : 0);

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
                RecalcWeight();
        }

        #endregion

        #region Actiony Stuff
        // IActionSource Members
        public IVolatileValue ActionClassLevel
            => new Deltable(1);

        public IEnumerable<CoreAction> GetTacticalActions(CoreActionBudget budget)
        {
            // allow conveyance to be grabbed
            if (budget.Actor is Creature _critter)
            {
                var _budget = budget as LocalActionBudget;
                if (_budget?.CanPerformBrief ?? false)
                {
                    // if critter and furniture are not already so engaged...
                    var _grabbed = Adjuncts.OfType<ObjectGrabbed>()
                        .FirstOrDefault(_gt => _gt.ObjectGrabGroup.Members.Any(_m => _m.Anchor == _critter));
                    if (_grabbed == null)
                    {
                        yield return new GrabObject(this, @"801");
                    }
                    else
                    {
                        foreach (var _act in _grabbed.GetActions(budget))
                            yield return _act;
                    }
                }
            }
            yield break;
        }

        public bool IsContextMenuOnly => true;

        // IActionProvider Members
        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
            => this.AccessibleActions(budget as LocalActionBudget).Union(GetTacticalActions(budget));

        public abstract IEnumerable<CoreAction> GetGrabbedActions(CoreActionBudget budget);
        #endregion

        public override Info MergeConnectedInfos(Info fetchedInfo, CoreActor actor)
            => fetchedInfo;
    }
}
