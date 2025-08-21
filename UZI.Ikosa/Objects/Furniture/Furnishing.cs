using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Items.Materials;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Movement;
using Uzi.Visualize;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Actions;
using System.Diagnostics;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public abstract class Furnishing : CoreObject, IVisible, IAnchorage, IActionSource,
         ITacticalActionProvider, IObjectBase, IAlternateRelocate, ICloneable
    {
        #region ctor(...)
        protected Furnishing(string name, Material material)
            : base(name)
        {
            ObjectMaterial = material;
            _SoundDiff = new Deltable(0);
            _Sizer = new ObjectSizer(Size.Medium, this);
            _Orient = new FurnishingOrientation(this);
            _Connected = [];
            _COCtrl = new ChangeController<ICoreObject>(this, null);
        }

        #region protected override void InitInteractionHandlers()
        protected override void InitInteractionHandlers()
        {
            AddIInteractHandler(new ItemAttackHandler());
            AddIInteractHandler(new TransitAttackHandler());
            AddIInteractHandler(new SpellTransitHandler());
            AddIInteractHandler(new FurnishingVisualHandler());
            AddIInteractHandler(new FurnishingObserveHandler());
            base.InitInteractionHandlers();
        }
        #endregion
        #endregion

        #region data
        // anchorage
        private double _TareWeight;
        private List<ICoreObject> _Connected;
        private ChangeController<ICoreObject> _COCtrl;

        // IObjectBase
        protected int _StrucPts;
        protected int _MaxStrucPts;
        private Material _Material;
        private bool _MWork;
        private Deltable _SoundDiff;

        // ISizer
        protected ObjectSizer _Sizer;

        // used for model-presentation and ITacticalInquiry
        private FurnishingOrientation _Orient;

        // tactical
        private bool _Effect = true;
        private bool _Detect = false;
        private bool _Conceal = true;
        private bool _Total = false;
        #endregion

        public override bool AddAdjunct(Adjunct adjunct)
        {
            var _success = base.AddAdjunct(adjunct);
            if (_success && adjunct is Located _located)
            {
                // toggle OnSituated...
                Orientation.SetOrientation(null, null, null);
            }
            return _success;
        }

        #region constrain dimensions
        #region private void AlterLocator(IGeometricSize origSize, Action reversion)
        private void AlterLocator(IGeometricSize origSize, Action reversion)
        {
            // test!
            var _vol = new FurnishingVolume(this);
            var _loc = this.GetLocated()?.Locator;
            if (_loc?.GeometricRegion is Cubic _cube)
            {
                var _planar = _loc?.PlanarPresence ?? PlanarPresence.Material;
                var _size = Orientation.SnappableSize;
                var (_fitCube, _fitOffset) = _vol.GetCubicFit(_cube, _size,
                    Orientation.ZHighSnap, Orientation.YHighSnap, Orientation.XHighSnap, _planar);
                if (_fitCube == null)
                {
                    // revert!
                    reversion?.Invoke();
                }
                else
                {
                    // set intra-model offset of locator
                    if (_loc != null)
                    {
                        _loc.IntraModelOffset = _fitOffset;

                        // relocate if size changed
                        if (!origSize.SameSize(_fitCube))
                        {
                            _loc.Relocate(_fitCube, _loc.PlanarPresence);
                        }
                    }
                }
            }
        }
        #endregion

        protected override void CoreSetHeight(double value)
        {
            // less than 2 cells, and at least one other dimension is less than 2 cells
            if ((value <= 10d) && (value > 0)
                && ((Length <= 5) || (Width <= 5)))
            {
                var _old = Height;
                var _size = Orientation.SnappableSize;
                base.CoreSetHeight(value);
                AlterLocator(_size, () => base.CoreSetHeight(_old));
            }
        }

        protected override void CoreSetLength(double value)
        {
            if ((value <= 10d) && (value > 0)
                && ((Height <= 5) || (Width <= 5)))
            {
                var _old = Length;
                var _size = Orientation.SnappableSize;
                base.CoreSetLength(value);
                AlterLocator(_size, () => base.CoreSetLength(_old));
            }
        }

        protected override void CoreSetWidth(double value)
        {
            if ((value <= 10d) && (value > 0)
                && ((Length <= 5) || (Height <= 5)))
            {
                var _old = Width;
                var _size = Orientation.SnappableSize;
                base.CoreSetWidth(value);
                AlterLocator(_size, () => base.CoreSetWidth(_old));
            }
        }
        #endregion

        #region clone support
        protected void CopyFrom(Furnishing copyFrom)
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
            BlocksEffect = copyFrom.BlocksEffect;
            BlocksDetect = copyFrom.BlocksDetect;
            ProvidesConcealment = copyFrom.ProvidesConcealment;
            ProvidesTotalConcealment = copyFrom.ProvidesTotalConcealment;

            // TODO: compartments and other mechanisms?
        }

        public abstract object Clone();
        #endregion

        /// <summary>Directly connected objects</summary>
        public override IEnumerable<ICoreObject> Connected
            => _Connected.Select(_c => _c);

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

            var _vol = new FurnishingVolume(this);
            var _size = Orientation.SnappableSize;

            // if we don't have a cubic, make a cubic from the region
            if (!(region is Cubic))
            {
                region = region.ContainingCube(region);
            }

            var (_fitCube, _fitOffset) = _vol.GetCubicFit(region as Cubic, _size,
                Orientation.ZHighSnap, Orientation.YHighSnap, Orientation.XHighSnap,
                locator.PlanarPresence);
            if (_fitCube == null)
            {
                var _region = locator.GeometricRegion;
                if (_region != null)
                {
                    var _curr = new GeometricSize(_region);
                    if (!_curr.SameSize(_size))
                    {
                        var _off = locator.IntraModelOffset;
                        var _faces = AnchorFaceList.None;
                        if (_off.Z < -3.5)
                        {
                            _faces = _faces.Add(AnchorFace.ZLow);
                        }

                        if (_off.Y < -3.5)
                        {
                            _faces = _faces.Add(AnchorFace.YLow);
                        }

                        if (_off.X < -3.5)
                        {
                            _faces = _faces.Add(AnchorFace.XLow);
                        }

                        if (_off.Z > 3.5)
                        {
                            _faces = _faces.Add(AnchorFace.ZHigh);
                        }

                        if (_off.Y > 3.5)
                        {
                            _faces = _faces.Add(AnchorFace.YHigh);
                        }

                        if (_off.X > 3.5)
                        {
                            _faces = _faces.Add(AnchorFace.XHigh);
                        }

                        var _newCube = (region as Cubic).OffsetCubic(_faces.ToAnchorFaces().ToArray());
                        (_fitCube, _fitOffset) = _vol.GetCubicFit(_newCube, _size,
                            Orientation.ZHighSnap, Orientation.YHighSnap, Orientation.XHighSnap,
                            locator.PlanarPresence);
                    }
                }
            }
            return (_fitCube, _fitOffset);
        }
        #endregion

        public IGeometricSize GeometricSize
            => Orientation.SnappableSize;

        // IVisible
        public virtual bool IsVisible
            => Invisibility.IsVisible(this);

        public override bool IsTargetable => true;

        /// <summary>Basics: every hardness of 5 gives a fall reduce of 1 (10 feet)</summary>
        public virtual double FallReduce => Hardness / 5d;

        public virtual int MaxFallSpeed => 500;

        #region protected bool IsFaceAccessible(IGeometricRegion furnish, Locator actorLoc, AnchorFace face)
        protected bool IsFaceAccessible(IGeometricRegion furnish, Locator principalLoc, AnchorFace face)
        {
            var _principalRgn = principalLoc?.GeometricRegion;
            if (_principalRgn == null)
            {
                return false;
            }

            // TODO: may want to check actor movement...

            return face switch
            {
                AnchorFace.XLow => Orientation.IsFaceSnapped(AnchorFace.XLow)
                    ? _principalRgn.LowerX < furnish.LowerX
                    : _principalRgn.LowerX <= furnish.LowerX,
                AnchorFace.XHigh => Orientation.IsFaceSnapped(AnchorFace.XHigh)
                    ? _principalRgn.UpperX > furnish.UpperX
                    : _principalRgn.UpperX >= furnish.UpperX,
                AnchorFace.YLow => Orientation.IsFaceSnapped(AnchorFace.YLow)
                    ? _principalRgn.LowerY < furnish.LowerY
                    : _principalRgn.LowerY <= furnish.LowerY,
                AnchorFace.YHigh => Orientation.IsFaceSnapped(AnchorFace.YHigh)
                    ? _principalRgn.UpperY > furnish.UpperY
                    : _principalRgn.UpperY >= furnish.UpperY,
                AnchorFace.ZLow => Orientation.IsFaceSnapped(AnchorFace.ZLow)
                    ? _principalRgn.LowerZ < furnish.LowerZ
                    : _principalRgn.LowerZ <= furnish.LowerZ,
                AnchorFace.ZHigh => Orientation.IsFaceSnapped(AnchorFace.ZHigh)
                    ? _principalRgn.UpperZ > furnish.UpperZ
                    : _principalRgn.UpperZ >= furnish.UpperZ,
                _ => true,
            };
        }
        #endregion

        #region public override IEnumerable<ICoreObject> Accessible(ICoreObject principal)
        public override IEnumerable<ICoreObject> Accessible(ICoreObject principal)
        {
            // relative regions
            var _actorLoc = Locator.FindFirstLocator(principal);
            var _funishRgn = Locator.FindFirstLocator(this)?.GeometricRegion;

            foreach (var _obj in base.Accessible(principal))
            {
                if (_obj.HasAdjunct<ConnectedSides>())
                {
                    // anchor faces for connected sides
                    if ((from _cs in _obj.Adjuncts.OfType<ConnectedSides>()
                         from _s in _cs.Sides
                         select _s)
                        .Distinct()
                        .Select(_s => Orientation.GetAnchorFaceForSideIndex(_s))
                        .Any(_f => IsFaceAccessible(_funishRgn, _actorLoc, _f)))
                    {
                        yield return _obj;
                    }
                }
                else
                {
                    yield return _obj;
                }
            }
            yield break;
        }
        #endregion

        #region IAnchorage Members
        public virtual bool CanAcceptAnchor(IAdjunctable newAnchor)
            => (newAnchor is ICoreObject _core)
            && !_Connected.Contains(_core) && !_COCtrl.WillAbortChange(_core, @"Add");

        public virtual bool CanEjectAnchor(IAdjunctable existingAnchor)
            => (existingAnchor is ICoreObject _core)
            && _Connected.Contains(_core) && !_COCtrl.WillAbortChange(_core, @"Remove");

        public virtual void AcceptAnchor(IAdjunctable newAnchor)
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

        public virtual void EjectAnchor(IAdjunctable existingAnchor)
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

        #region Actiony Stuff
        // IActionSource Members
        public IVolatileValue ActionClassLevel
            => new Deltable(1);

        // ITacticalActionProvider Members
        public virtual IEnumerable<CoreAction> GetTacticalActions(CoreActionBudget budget)
        {
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
                        {
                            yield return _act;
                        }
                    }
                }
            }
            yield break;
        }

        public bool IsContextMenuOnly => true;

        // IActionProvider Members
        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
            => this.AccessibleActions(budget as LocalActionBudget).Union(GetTacticalActions(budget));

        public Guid PresenterID => ID;
        #endregion

        #region ITacticalInquiry Members

        protected virtual bool SegmentIntersection(in TacticalInfo tacticalInfo)
        {
            foreach (var _pp in Orientation.GetPlanarPoints(GetPlanarFaces()))
            {
                if (_pp.SegmentIntersection(tacticalInfo.SourcePoint, tacticalInfo.TargetPoint).HasValue)
                {
                    return true;
                }
            }
            return false;
        }

        public CoverLevel SuppliesCover(in TacticalInfo tacticalInfo)
            // TODO: property to control this???
            => SegmentIntersection(in tacticalInfo)
            ? CoverLevel.Hard
            : CoverLevel.None;

        public bool SuppliesConcealment(in TacticalInfo tacticalInfo)
            => ProvidesConcealment && SegmentIntersection(in tacticalInfo);

        public bool SuppliesTotalConcealment(in TacticalInfo tacticalInfo)
            => ProvidesTotalConcealment && SegmentIntersection(in tacticalInfo);

        public bool BlocksLineOfEffect(in TacticalInfo tacticalInfo)
            => BlocksEffect && SegmentIntersection(in tacticalInfo);

        public bool BlocksLineOfDetect(in TacticalInfo tacticalInfo)
            => BlocksDetect && SegmentIntersection(in tacticalInfo);

        public abstract bool BlocksSpread(AnchorFace transitFace, ICellLocation sourceCell, ICellLocation targetCell);

        public abstract bool CanBlockSpread { get; }

        #endregion

        #region IMoveAlterer Members

        // outward/inward blockage
        public virtual bool BlocksTransit(MovementTacticalInfo moveTactical)
        {
            // small surfaces can be ignored
            if (Length < 2 || Width < 2)
            {
                return false;
            }

            return !moveTactical.Movement.CanMoveThrough(ObjectMaterial)
                && Orientation.GetPlanarPoints(GetPlanarFaces())
                .Any(_f => _f.SegmentIntersection(
                    moveTactical.SourceCell.GetPoint(), moveTactical.TargetCell.GetPoint()).HasValue);
        }

        protected bool ApplyOpening(ICellLocation occupyCell, ICoreObject testObj, Axis axis)
            => (testObj is Furnishing _furnish)
            ? Orientation.GetHedralCoverage(occupyCell, axis)
                .Intersect(_furnish.Orientation.GetHedralCoverage(occupyCell, axis)).HasAny
            : true;

        #region public virtual IEnumerable<MovementOpening> OpensTowards(MovementBase movement, ICellLocation occupyCell, AnchorFace baseFace, ICoreObject testObj)
        public virtual IEnumerable<MovementOpening> OpensTowards(MovementBase movement, ICellLocation occupyCell, AnchorFace baseFace, ICoreObject testObj)
        {
            // NOTE: supported surface *might* provide more than one opens-towards...?
            if (!movement.CanMoveThrough(ObjectMaterial))
            {
                var _loc = this.GetLocated();
                var _rgn = _loc?.Locator?.GeometricRegion;
                if (_rgn?.ContainsCell(occupyCell) ?? false)
                {
                    var _ext = Orientation.CoverageExtents;
                    foreach (var _mvOpen in from _af in GetCoveredFaces().ToAnchorFaces()
                                            where !Orientation.IsFaceSnapped(_af, _ext)
                                            && ApplyOpening(occupyCell, testObj, _af.GetAxis())
                                            from _mo in AnchorFaceOpenings(_af, occupyCell, _rgn, false)
                                            select _mo)
                    {
                        yield return _mvOpen;
                    }
                }
            }
            yield break;
        }
        #endregion

        #region public virtual bool HindersTransit(MovementBase movement, IGeometricRegion region)
        public virtual bool HindersTransit(MovementBase movement, IGeometricRegion region)
        {
            if (!movement.CanMoveThrough(ObjectMaterial))
            {
                var _srcPt = region.GetPoint3D();
                var _corners = region.AllCorners().ToList();
                var _sides = Orientation.GetPlanarPoints(GetPlanarFaces());

                // if source is in a cell that is cut by a plane...
                return (from _s in _sides
                        from _c in _corners
                        let _i = _s.SegmentIntersection(_srcPt, _c).HasValue
                        where _i
                        select _i).Any();
            }
            return false;
        }
        #endregion

        #region public virtual double HedralTransitBlocking(MovementTacticalInfo moveTactical)
        /// <summary>Used to determine support against gravity and starting elevations</summary>
        public virtual double HedralTransitBlocking(MovementTacticalInfo moveTactical)
        {
            if (!moveTactical.Movement.CanMoveThrough(ObjectMaterial))
            {
                // reverse transit faces must be snapped (should only be one face)
                var _ext = Orientation.CoverageExtents;
                if (moveTactical.TransitFaces
                    .Any(_f => !Orientation.IsFaceSnapped(_f, _ext)))
                {
                    return 0;
                }

                return GetCoverage(moveTactical);
            }
            return 0d;
        }
        #endregion

        #region public virtual bool CanOccupy(MovementBase movement, IGeometricRegion region, ICoreObject testObj)
        // cell-wide can occupy for movement
        public virtual bool CanOccupy(MovementBase movement, IGeometricRegion region, ICoreObject testObj)
        {
            if (!movement.CanMoveThrough(ObjectMaterial))
            {
                // TODO: depends on dimensions...bulkiness...or on top...
                var _surfaces = Orientation.GetPlanarPoints(GetPlanarFaces());
                var _cells = region.AllCellLocations().ToList();
                while (_cells.Any())
                {
                    // start with any cell in region
                    var _source = _cells.First();

                    // compare to other cells
                    foreach (var _target in _cells.Skip(1))
                    {
                        // connecting line go through a surface?
                        foreach (var _corners in _surfaces)
                        {
                            if (_corners.SegmentIntersection(_source.GetPoint(), _target.GetPoint()).HasValue)
                            {
                                return false;
                            }
                        }
                    }

                    // draw down the cell
                    _cells.Remove(_source);
                }
            }
            return true;
        }
        #endregion

        // cell-wide move costliness
        public abstract bool IsCostlyMovement(MovementBase movement, IGeometricRegion region);

        #endregion

        public ObjectSizer ObjectSizer => _Sizer;
        public Sizer Sizer => _Sizer;
        public Deltable ExtraSoundDifficulty => _SoundDiff;

        /// <summary>Some furnishings cannot stand upright under certain conditions</summary>
        public abstract bool IsUprightAllowed { get; }
        // TODO: consider some non-flexible flat objects (panels/lids) can be upright if against a solid surface

        public virtual int ArmorRating
            => this.GetArmorRating(Sizer);

        public virtual bool IsLocatable
            => true;

        public virtual int Hardness
            => ObjectMaterial.Hardness;

        public int GetHardness()
            => Hardness;

        public virtual void AttendeeAdjustments(IAttackSource source, AttackData attack) { this.DoAttendeeAdjustments(source, attack); }

        #region public virtual Material ObjectMaterial { get; set; }
        public virtual Material ObjectMaterial
        {
            get => _Material;
            set
            {
                _Material = value;
                DoPropertyChanged(nameof(ObjectMaterial));
            }
        }
        #endregion

        #region public bool Masterwork { get; set; }
        public bool Masterwork
        {
            get => _MWork;
            set
            {
                _MWork = value;
                DoPropertyChanged(nameof(Masterwork));
            }
        }
        #endregion

        #region public virtual int StructurePoints {get; set;}
        public virtual int StructurePoints
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

        #region public virtual int MaxStructurePoints {get; set;}
        public virtual int MaxStructurePoints
        {
            get => _MaxStrucPts;
            set
            {
                var _diff = value - _MaxStrucPts;
                if (value > 0)
                {
                    _MaxStrucPts = value;
                    DoPropertyChanged(nameof(MaxStructurePoints));
                    StructurePoints += _diff;
                }
            }
        }
        #endregion

        public FurnishingOrientation Orientation => _Orient;

        public ObjectPresenter ObjectPresenter
            => (ObjectPresenter)this.GetLocated()?.Locator;

        public override CoreSetting Setting
            => ObjectPresenter?.Context.ContextSet.Setting;

        #region Info Providing
        public override Info GetInfo(CoreActor actor, bool baseValues)
            => ObjectInfoFactory.CreateInfo<ObjectInfo>(actor, this);

        public override Info MergeConnectedInfos(Info fetchedInfo, CoreActor actor)
            => fetchedInfo;

        /// <summary>Used when the ObjectBase is acting as an IActionProvider</summary>
        public virtual Info GetProviderInfo(CoreActionBudget budget)
            => GetInfoData.GetInfoFeedback(this, budget.Actor);
        #endregion

        // dimensional

        /// <summary>True if the face is covered by a furnishing face</summary>
        protected virtual bool IsCoveredFace(AnchorFace face)
            => GetCoveredFaces().Contains(face);

        #region public System.Windows.Media.Media3D.Point3D[] GetDimensionalCorners(AnchorFace face)
        /// <summary>Get point cloud for base face, untransformed (allows ApplicableTransform to be used separately)</summary>
        public System.Windows.Media.Media3D.Point3D[] GetDimensionalCorners(AnchorFace face)
        {
            var _z = Height / 2;
            var _y = Width / 2;
            var _x = Length / 2;
            return face switch
            {
                AnchorFace.ZHigh =>
                    new System.Windows.Media.Media3D.Point3D[]
                    {
                        new System.Windows.Media.Media3D.Point3D(0 - _x, 0 - _y, _z),
                        new System.Windows.Media.Media3D.Point3D(    _x, 0 - _y, _z),
                        new System.Windows.Media.Media3D.Point3D(    _x,     _y, _z),
                        new System.Windows.Media.Media3D.Point3D(0 - _x,     _y, _z)
                    },
                AnchorFace.ZLow => new System.Windows.Media.Media3D.Point3D[]
                    {
                        new System.Windows.Media.Media3D.Point3D(0 - _x, 0 - _y, 0 - _z),
                        new System.Windows.Media.Media3D.Point3D(    _x, 0 - _y, 0 - _z),
                        new System.Windows.Media.Media3D.Point3D(    _x,     _y, 0 - _z),
                        new System.Windows.Media.Media3D.Point3D(0 - _x,     _y, 0 - _z)
                    },
                AnchorFace.YHigh => new System.Windows.Media.Media3D.Point3D[]
                    {
                        new System.Windows.Media.Media3D.Point3D(0 - _x, _y, 0 - _z),
                        new System.Windows.Media.Media3D.Point3D(    _x, _y, 0 - _z),
                        new System.Windows.Media.Media3D.Point3D(    _x, _y,     _z),
                        new System.Windows.Media.Media3D.Point3D(0 - _x, _y,     _z)
                    },
                AnchorFace.YLow => new System.Windows.Media.Media3D.Point3D[]
                    {
                        new System.Windows.Media.Media3D.Point3D(0 - _x, 0 - _y, 0 - _z),
                        new System.Windows.Media.Media3D.Point3D(    _x, 0 - _y, 0 - _z),
                        new System.Windows.Media.Media3D.Point3D(    _x, 0 - _y,     _z),
                        new System.Windows.Media.Media3D.Point3D(0 - _x, 0 - _y,     _z)
                    },
                AnchorFace.XHigh => new System.Windows.Media.Media3D.Point3D[]
                    {
                        new System.Windows.Media.Media3D.Point3D(_x, 0 - _y, 0 - _z),
                        new System.Windows.Media.Media3D.Point3D(_x,     _y, 0 - _z),
                        new System.Windows.Media.Media3D.Point3D(_x,     _y,     _z),
                        new System.Windows.Media.Media3D.Point3D(_x, 0 - _y,     _z)
                    },
                _ => new System.Windows.Media.Media3D.Point3D[]
                    {
                        new System.Windows.Media.Media3D.Point3D(0 - _x, 0 - _y, 0 - _z),
                        new System.Windows.Media.Media3D.Point3D(0 - _x,     _y, 0 - _z),
                        new System.Windows.Media.Media3D.Point3D(0 - _x,     _y,     _z),
                        new System.Windows.Media.Media3D.Point3D(0 - _x, 0 - _y,     _z)
                    }
            };
        }
        #endregion

        /// <summary>Indicates whether face snapping is considered hard for displacement purposes or whether margin can be maintained.</summary>
        public abstract bool IsHardSnap(AnchorFace face);

        protected virtual AnchorFaceList GetCoveredFaces()
            => AnchorFaceList.All;

        /// <summary>Defines standard reference planar coverage faces for furniture orientation point cloud</summary>
        protected virtual AnchorFaceList GetPlanarFaces()
            => AnchorFaceList.All;

        /// <summary>Provides volumetric coverage across an axis (0..1)</summary>
        protected double GetCoverage(ICellLocation cell, Axis axis)
            => Orientation.GetHedralCoverage(cell, axis).GripCount() / 64d;

        #region protected double GetCoverage(LocalLink link)
        protected double GetCoverage(LocalLink link)
        {
            var _region = ObjectPresenter?.GeometricRegion;
            var _roomA = link.GroupA.ContainsGeometricRegion(_region);
            var _face = _roomA
                ? link.AnchorFaceInA
                : link.AnchorFaceInA.ReverseFace();

            if (Orientation.IsFaceSnapped(_face) && IsCoveredFace(_face))
            {
                return GetCoverage(
                    _roomA ? link.InteractionCell(link.GroupA) : link.InteractionCell(link.GroupB),
                    _face.GetAxis());
            }
            return 0d;
        }
        #endregion

        #region protected double GetCoverage(MovementTacticalInfo moveTactical)
        /// <summary>Always uses TransitFace and SourceCell to probe from inside to outside</summary>
        protected double GetCoverage(MovementTacticalInfo moveTactical)
        {
            var _region = ObjectPresenter?.GeometricRegion;
            var _face = moveTactical.TransitFaces.FirstOrDefault();
            if (Orientation.IsFaceSnapped(_face) && IsCoveredFace(_face))
            {
                return GetCoverage(moveTactical.SourceCell, _face.GetAxis());
            }

            return 0d;
        }
        #endregion

        protected bool IsCellFaceAtRegionEdge(IGeometricRegion region, ICellLocation sourceCell, AnchorFace face)
            => ((face == AnchorFace.ZHigh) && (sourceCell.Z == region.UpperZ))
                || ((face == AnchorFace.YHigh) && (sourceCell.Y == region.UpperY))
                || ((face == AnchorFace.XHigh) && (sourceCell.X == region.UpperX))
                || ((face == AnchorFace.ZLow) && (sourceCell.Z == region.LowerZ))
                || ((face == AnchorFace.YLow) && (sourceCell.Y == region.LowerY))
                || ((face == AnchorFace.XLow) && (sourceCell.X == region.LowerX));

        #region protected IEnumerable<MovementOpening> AnchorFaceOpenings(...)
        protected IEnumerable<MovementOpening> AnchorFaceOpenings(AnchorFace face,
            ICellLocation sourceCell, IGeometricRegion region, bool useReverse)
        {
            // since we are at the edge, and haven't been face-snapped (from callee), it is safe to use GetCoverageExtent % 5
            if (IsCellFaceAtRegionEdge(region, sourceCell, face))
            {
                var _cellExtent = Orientation.GetCoverageExtent(face.GetAxis()) % 5;
                var _coverage = GetCoverage(sourceCell, face.GetAxis());
                yield return new MovementOpening(face, 5d - _cellExtent, _coverage);
                if (useReverse)
                {
                    yield return new MovementOpening(face.ReverseFace(), _cellExtent, _coverage);
                }
            }
            yield break;
        }
        #endregion

        // tactical

        #region public bool BlocksEffect { get; set; }
        public bool BlocksEffect
        {
            get => _Effect;
            set
            {
                _Effect = value;
                DoPropertyChanged(nameof(BlocksEffect));
            }
        }
        #endregion

        #region public bool BlocksDetect { get; set; }
        public bool BlocksDetect
        {
            get => _Detect;
            set
            {
                _Detect = value;
                DoPropertyChanged(nameof(BlocksDetect));
            }
        }
        #endregion

        #region public bool ProvidesConcealment { get; set; }
        public bool ProvidesConcealment
        {
            get => _Conceal;
            set
            {
                _Conceal = value;
                DoPropertyChanged(nameof(ProvidesConcealment));
            }
        }
        #endregion

        #region public bool ProvidesTotalConcealment { get; set; }
        public bool ProvidesTotalConcealment
        {
            get => _Total;
            set
            {
                _Total = value;
                DoPropertyChanged(nameof(ProvidesTotalConcealment));
            }
        }
        #endregion

        // IProvideSaves

        #region public virtual BestSoftQualifiedDelta GetBestSoftSave(SavingThrowData saveData)
        public virtual BestSoftQualifiedDelta GetBestSoftSave(SavingThrowData saveData)
        {
            if (AlwaysFailsSave)
            {
                return null;
            }

            // potential save values
            var _deltables = new List<ConstDeltable>();

            // strongest magic-source aura
            var _casterLevel = (from _msaa in Adjuncts.OfType<MagicSourceAuraAdjunct>()
                                where _msaa.IsActive
                                orderby _msaa.CasterLevel descending
                                select new ConstDeltable(Math.Max(_msaa.CasterLevel / 2, 1))).FirstOrDefault();
            if (_casterLevel != null)
            {
                _deltables.Add(_casterLevel);
            }

            // may be multiple attendees?
            ConstDeltable _save(Creature critter)
                => saveData.SaveMode.SaveType switch
                {
                    SaveType.Fortitude => critter.FortitudeSave,
                    SaveType.Reflex => critter.ReflexSave,
                    SaveType.Will => critter.WillSave,
                    _ => null,
                };

            foreach (var _attendee in (from _a in Adjuncts.OfType<Attended>()
                                       where _a.IsActive
                                       select _save(_a.Creature)))
            {
                _deltables.Add(_attendee);
            }

            if (_deltables.Any())
            {
                return new BestSoftQualifiedDelta(_deltables.ToArray());
            }

            return null;
        }
        #endregion

        public virtual bool AlwaysFailsSave
            => !this.HasActiveAdjunct<Attended>();
    }
}