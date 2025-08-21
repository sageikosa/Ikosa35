using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Senses;
using System.Linq;
using Uzi.Ikosa.Movement;
using System.Diagnostics;
using Uzi.Visualize;
using Uzi.Packaging;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Universal;
using Newtonsoft.Json;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Tactical
{
    /// <summary>Used to locate an object or set of objects in a local map setting</summary>
    [Serializable]
    public class Locator : CoreToken, ILightVisibleTarget,
        IGeometryAnchorSupplier, IControlChange<IGeometricRegion>,
        IControlChange<IGeometricSize>, ICorePart
    {
        #region Constructor
        public Locator(ICoreObject root, MapContext context, IGeometricSize normalSize, IGeometricRegion region)
            : this(root, context, normalSize, region, true)
        {
        }

        protected Locator(ICoreObject root, MapContext context, IGeometricSize normalSize, IGeometricRegion region, bool addToContext)
            : base(root, context)
        {
            _NormSize = new GeometricSize(normalSize);
            _ApparentScale = 1;
            _Region = region;
            _RegionCtrl = new ChangeController<IGeometricRegion>(this, region);
            _LocationCtrl = new ChangeController<ICellLocation>(this, Location);
            _CurrentSizeCtrl = new ChangeController<IGeometricSize>(this, new GeometricSize(region));
            _CellGroups = [];
            LightLevel = LightRange.OutOfRange;
            ActiveMovement = null;
            if (addToContext)
            {
                context.Add(this);
            }

            _Ethereal = false;
        }
        #endregion

        #region data
        private double _ApparentScale;
        private bool _MustSqueeze = false;
        private IGeometricRegion _Region;
        private GeometricSize _NormSize;
        private ChangeController<IGeometricRegion> _RegionCtrl;
        private ChangeController<ICellLocation> _LocationCtrl;
        private ChangeController<IGeometricSize> _CurrentSizeCtrl;
        private List<LocalCellGroup> _CellGroups;
        private MovementBase _Movement = null;
        private AnchorFaceList _BaseFace = AnchorFaceList.None;
        private AnchorFaceList _MoveCrossings = AnchorFaceList.None;
        private Vector3D _IntraModelOffset = new Vector3D();
        private IIllumination _BestLight = null;
        private Vector3D _MoveFrom = new Vector3D();
        private IGeometricRegion _LastLegal = null;
        private Vector3D _LastOffset = new Vector3D();
        private List<LocalCellGroup> _Links;
        private bool _Ethereal;
        [NonSerialized, JsonIgnore]
        private ulong _SerialState = 0;
        #endregion

        public IkosaProcessManager IkosaProcessManager
            => Context.ContextSet.ProcessManager as IkosaProcessManager;

        public LocalMap Map
            => Context.ContextSet.Setting as LocalMap;

        public MapContext MapContext
            => Context as MapContext;

        public PlanarPresence PlanarPresence => _Ethereal ? PlanarPresence.Ethereal : PlanarPresence.Material;

        #region public bool HasMeleeCover(Locator source)
        /// <summary>Determines whether this locator has the specified melee cover level from the source</summary>
        public bool HasMeleeCover(Locator source)
        {
            var _map = Map;

            var _planar = PlanarPresence;
            foreach (var _tLoc in GeometricRegion.AllCellLocations()
                .Select(_cl => _cl is CellLocation ? _cl as CellLocation : new CellLocation(_cl)))
            {
                foreach (var _sLoc in source.GeometricRegion.AllCellLocations()
                    .Select(_cl => _cl is CellLocation ? _cl as CellLocation : new CellLocation(_cl)))
                {
                    // build an attack
                    var _mAtk = new MeleeAttackData(source.Chief, null, source, AttackImpact.Penetrating, null,
                        null, true, _sLoc, _tLoc, 1, 1);
                    var _srcPts = GetMeleeSourcePoints.GetPoints(_mAtk);
                    var _trgPts = GetMeleeTargetPoints.GetPoints(_mAtk, ICore as IInteract, _srcPts.DownWard, _srcPts.DownFace);

                    // anything less than +4 cover means no melee cover
                    if (_map.CoverValue(_srcPts.Value, _trgPts.Value, source.GeometricRegion, _mAtk.TargetCell.ToCellPosition(),
                        false, new Interaction(_mAtk.Attacker, this, ICore as IInteract, _mAtk), _planar, ICore, source.ICore) < 4)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        #endregion

        /// <summary>Finds the first locator for the target in its environment</summary>
        public static Locator FindFirstLocator(ICore target)
            => (target as IAdjunctSet)?.GetLocated()?.Locator;

        public Point3D CenterPoint
            => _Region.GetPoint3D();

        #region IntraModelOffset functions
        /// <summary>Used to offset the locator due to terrain hugging</summary>
        public Vector3D IntraModelOffset { get { return _IntraModelOffset; } set { _IntraModelOffset = value; } }

        /// <summary>Elevation of base from Gravity and IntraModelOffset</summary>
        public double Elevation
            => (GetGravityFace()) switch
            {
                AnchorFace.ZLow => (GeometricRegion.LowerZ * 5) + IntraModelOffset.Z,
                AnchorFace.ZHigh => ((GeometricRegion.UpperZ + 1) * 5) + IntraModelOffset.Z,
                AnchorFace.YLow => (GeometricRegion.LowerY * 5) + IntraModelOffset.Y,
                AnchorFace.YHigh => ((GeometricRegion.UpperY + 1) * 5) + IntraModelOffset.Y,
                AnchorFace.XLow => (GeometricRegion.LowerX * 5) + IntraModelOffset.X,
                AnchorFace.XHigh => ((GeometricRegion.UpperX + 1) * 5) + IntraModelOffset.X,
                _ => (GeometricRegion.LowerZ * 5) + IntraModelOffset.Z,
            };

        public double GetAxialOffset(Axis axis)
            => axis switch
            {
                Axis.X => _IntraModelOffset.X,
                Axis.Y => _IntraModelOffset.Y,
                Axis.Z => _IntraModelOffset.Z,
                _ => _IntraModelOffset.Z,
            };
        #endregion

        /// <summary>Provides the scale vector needed to get the original model to fit in the normal size</summary>
        public Vector3D CubeFitScale
            => new Vector3D(_NormSize.XExtent, _NormSize.YExtent, _NormSize.ZExtent);

        #region public virtual double ApparentScale { get; set; }
        /// <summary>Used to scale the appearance of the locator</summary>
        public virtual double ApparentScale
        {
            get { return _ApparentScale; }
            set { _ApparentScale = value; }
        }
        #endregion

        public IGeometricRegion GeometricRegion => _Region;

        // reversion tracking if movement needs to be undone
        public IGeometricRegion LastLegalRegion { get { return _LastLegal; } set { _LastLegal = value; } }
        public Vector3D LastModelOffset { get { return _LastOffset; } set { _LastOffset = value; } }

        public void RefreshAppearance(PlanarPresence prevPresence, IGeometricRegion previous = null)
        {
            // explicitly let the map context know we're moving
            MapContext.SetLocatorRegion(this, previous, prevPresence);
            this.DetermineIllumination();

            // resynce sensors on the locator
            ResyncAllSensorHosts();
        }

        #region public void Relocate(IGeometricRegion region, PlanarPresence planarPresence)
        public void Relocate(IGeometricRegion region, PlanarPresence presence)
        {
            if (region != null)
            {
                // try alternates
                if (ICore is IAlternateRelocate _altReloc)
                {
                    var (_cube, _offset) = _altReloc.GetRelocation(region, this);
                    if (_cube == null)
                    {
                        // nothing, so cannot relocate
                        return;
                    }

                    // set from alternates
                    region = _cube;
                    IntraModelOffset = _offset;
                }

                // changing!, capture old
                var _old = _Region;
                var _oldPresence = PlanarPresence;

                // set new
                _Region = region;
                _Ethereal = presence.HasEtherealPresence();
                _MoveFrom = _old.GetPoint3D() - _Region.GetPoint3D();
                _SerialState = MapContext.SerialState + 1;

                // TODO: NOTE: clearance in neighboring cells can negate forced squeezing?

                // explicitly let the map context know we're moving
                RefreshAppearance(_oldPresence, _old);

                // change controllers monitoring location and region
                _LocationCtrl.DoValueChanged(Location);
                _RegionCtrl.DoValueChanged(region);
                DoPropertyChanged(nameof(PlanarPresence));

                // when the current cube changes, inform listeners...
                var _size = new GeometricSize(region);
                if (!_size.SameSize(_CurrentSizeCtrl.LastValue))
                {
                    _CurrentSizeCtrl.DoValueChanged(_size);
                }

                // each object in old and new regions gets to know we have moved, including our contained objects
                // and we get to know that locators in old/new region have virtually moved
                foreach (var _otherLoc in MapContext.LocatorsInRegion(_old, PlanarPresence.Both)
                    .Union(MapContext.LocatorsInRegion(region, PlanarPresence.Both))
                    .Where(_l => _l != this).ToList())
                {
                    // locator is passing by each other locator
                    if (ICore is ICoreObject _thisObj)
                    {
                        _thisObj.HandleInteraction(new Interaction(null, this, _thisObj,
                            new LocatorMove(Chief, _otherLoc, LocatorMoveState.PassingBy)));
                    }

                    // each other locator is passed by this locator
                    if (_otherLoc.ICore is ICoreObject _otherObj)
                    {
                        _otherObj.HandleInteraction(new Interaction(null, this, _otherObj,
                            new LocatorMove(_otherLoc.Chief, this, LocatorMoveState.TargetPassedBy)));
                    }
                }
            }
            else
            {
                Debug.WriteLine($@"{DateTime.Now:o}:Locator.Relocate.region == null");
            }

            EnsureSqueeze();
        }
        #endregion

        #region internal void Locate()
        internal void Locate()
        {
            // make sure planar presence is correct for ICore
            _Ethereal = ICoreAs<IAdjunctSet>().Any(_as => _as.HasActiveAdjunct<EtherealState>());

            // bootstrap our groups before adjuncting
            var _groups = RecalcLocalCellGroups().groups;
            foreach (var _new in _groups)
            {
                _new.AddLocator(this);
            }

            foreach (var _item in ICoreAs<IAdjunctable>())
            {
                _item.AddAdjunct(new Located(this));
            }

            DoPropertyChanged(nameof(PlanarPresence));
            RefreshAppearance(PlanarPresence);

            // each object in new region gets to know we have moved, including our contained objects
            // and we get to know that locators in new region have virtually moved
            foreach (var _loc in MapContext.LocatorsInRegion(GeometricRegion, PlanarPresence.Both)
                .Where(_l => _l != this).ToList())
            {
                // each locator is virtually moving with respect to this one
                if (ICore is ICoreObject _obj)
                {
                    _obj.HandleInteraction(new Interaction(null, this, _obj,
                        new LocatorMove(Chief, _loc, LocatorMoveState.ArrivingTo)));
                }

                // this locator is moving with respect to each locator
                if (_loc.ICore is ICoreObject _locObj)
                {
                    _locObj.HandleInteraction(new Interaction(null, this, _locObj,
                        new LocatorMove(_loc.Chief, this, LocatorMoveState.TargetArrival)));
                }
            }
        }
        #endregion

        #region internal void Delocate()
        internal void Delocate()
        {
            foreach (var _item in ICoreAs<IAdjunctable>())
            {
                var _located = _item.Adjuncts.OfType<Located>().FirstOrDefault();
                if (_located != null)
                {
                    _located.Eject();
                }
            }

            // since room awarenesses won't be updated, the sensors on the locator should update directly
            foreach (var _sensor in ICoreAs<ISensorHost>())
            {
                _sensor.RoomAwarenesses?.ClearAwarenesses(_sensor, Map);
            }

            // each object in new region gets to know we have moved, including our contained objects
            // and we get to know that locators in new region have virtually moved
            foreach (var _loc in MapContext.LocatorsInRegion(GeometricRegion, PlanarPresence.Both)
                .Where(_l => _l != this).ToList())
            {
                // each locator is virtually moving with respect to this one
                if (ICore is ICoreObject _obj)
                {
                    _obj.HandleInteraction(new Interaction(null, this, _obj,
                        new LocatorMove(Chief, _loc, LocatorMoveState.DepartingFrom)));
                }

                // this locator is moving with respect to each locator
                if (_loc.ICore is ICoreObject _locObj)
                {
                    _locObj.HandleInteraction(new Interaction(null, this, _locObj,
                        new LocatorMove(_loc.Chief, this, LocatorMoveState.TargetDeparture)));
                }
            }
        }
        #endregion

        public void SetLightLevel(LightRange range)
        {
            LightLevel = range;
            foreach (var _c in AllConnectedOf<IInteract>())
            {
                _c.HandleInteraction(new Interaction(null, this, _c, new ApplyLight(null, range)));
            }
        }

        /// <summary>Last calculated light level on the locator</summary>
        public LightRange LightLevel { get; private set; }

        #region public bool InMagicalDarkness
        private bool _InMagicalDarkness = false;
        /// <summary>
        /// Determines whether the locator is in magical darkness after determining illumination (DarknessShrouded versus LightBathed effects).
        /// If so, then NearShadows are treated as FarShadows by LowLightVision when Hide checks are needed.
        /// </summary>
        /// <returns></returns>
        public bool InMagicalDarkness { get => _InMagicalDarkness; set => _InMagicalDarkness = value; }
        #endregion

        /// <summary>True if all local cell groups use the deep shadows illumination model</summary>
        public ShadowModel ShadowModel
            => GetLocalCellGroups().All(_g => _g.DeepShadows) ? ShadowModel.Deep
            : GetLocalCellGroups().Any(_g => _g.DeepShadows) ? ShadowModel.Mixed
            : ShadowModel.Normal;

        #region private void GetObscurementFlags(SegmentSet lSet, SensoryBase sense)
        /// <summary>Determines cover, concealment and total concealment along a line for a sense</summary>
        private (bool cover, bool concealment) GetObscurementFlags(SegmentSet lSet, SensoryBase sense)
        {
            var _cover = false;
            var _concealment = false;
            if (sense.UsesLineOfEffect && (lSet.SuppliesCover() > CoverLevel.Soft))
            {
                _cover = true;
            }
            if (!sense.IgnoresConcealment)
            {
                // check for concealment and total concealment
                switch (lSet.SuppliesConcealment()) // exclusion=targets
                {
                    case CoverConcealmentResult.Total:
                    case CoverConcealmentResult.Partial:
                        _concealment = true;
                        break;
                }

                if (lSet.IsLineOfEffect)
                {
                    // regen a fresh sense transit (in case of alterations)
                    var _sTrans = new SenseTransit(sense);
                    var _workSet = new Interaction(null, sense, null, _sTrans);

                    // pass each line-of-effect through the environment, looking for alterations
                    // TODO: allow interactionTransitZones to be presented as a re-usable context
                    if (lSet.CarryInteraction(_workSet))
                    {
                        // found a non-blocked line, so total concealment is impossible
                        if (_sTrans.IsConcealed)
                        {
                            // if any sense line is partially blocked...
                            _concealment = true;
                        }
                    }
                    else
                    {
                        // any sense was totally blocked...
                        _concealment = true;
                    }
                }
            }
            return (_cover, _concealment);
        }
        #endregion

        #region private void TargetPointObscurement(ICore observer, Point3D source, SensoryBase sense, int z, int y, int x, int[, ,] corner, bool[, ,] cornerConceal)
        private void TargetPointObscurement(ISensorHost observer, Point3D source, Intersection iSect,
            SensoryBase sense, int z, int y, int x, int[][][] corner, bool[][][] cornerConceal)
        {
            // haven't visited this point yet, and we haven't detected total concealment for this cell
            if (corner[z][y][x] == 0)
            {
                // only examine surface points
                var _set = ICoreAs<ITacticalInquiry>().Union((observer as ICore).ToEnumerable()).ToArray();
                var _lSet = Map.SegmentCells(source, iSect.Point3D(),
                    new SegmentSetFactory(Map, GeometricRegion, null, ITacticalInquiryHelper.GetITacticals(_set).ToArray(),
                    SegmentSetProcess.Observation), sense.PlanarPresence);
                // NOTE: an ethereal observer sees ethereal and material locators, otherwise, just material

                // line does not intersect observer or target, so look for things that might produce cover or concealment
                var (_cover, _concealment) = GetObscurementFlags(_lSet, sense);
                corner[z][y][x] = (_cover ? 2 : 1);
                cornerConceal[z][y][x] = _concealment;
            }
        }
        #endregion

        private T[][][] CubicArray<T>(long z, long y, long x)
            where T : struct
        {
            var _array = new T[z][][];
            for (var _z = 0; _z < z; _z++)
            {
                var _level2 = new T[y][];
                for (var _y = 0; _y < y; _y++)
                {
                    _level2[_y] = new T[x];
                }
                _array[_z] = _level2;
            }
            return _array;
        }

        #region public bool IsObscuredFromObserverPoint(ICore observer, Point3D source, SensoryBase sense)
        public bool IsObscuredFromObserverPoint(ISensorHost observer, Point3D source, SensoryBase sense)
        {

            // -1=bad-line (passes through locator), 0=not visited, 1=clear, 2=cover
            var _size = _CurrentSizeCtrl.LastValue;
            var _corner = CubicArray<int>(_size.ZHeight + 1, _size.YLength + 1, _size.XLength + 1);
            var _cornerConceal = CubicArray<bool>(_size.ZHeight + 1, _size.YLength + 1, _size.XLength + 1);
            var _location = Location;
            foreach (var _iSect in GeometricRegion.AllIntersections())
            {
                // calculate array indexed from intersections
                var _z = _iSect.Z - _location.Z;
                var _y = _iSect.Y - _location.Y;
                var _x = _iSect.X - _location.X;

                // fill up arrays
                TargetPointObscurement(observer, source, _iSect, sense, _z, _y, _x, _corner, _cornerConceal);
                TargetPointObscurement(observer, source, _iSect, sense, _z, _y, _x + 1, _corner, _cornerConceal);
                TargetPointObscurement(observer, source, _iSect, sense, _z, _y + 1, _x, _corner, _cornerConceal);
                TargetPointObscurement(observer, source, _iSect, sense, _z, _y + 1, _x + 1, _corner, _cornerConceal);
                TargetPointObscurement(observer, source, _iSect, sense, _z + 1, _y, _x, _corner, _cornerConceal);
                TargetPointObscurement(observer, source, _iSect, sense, _z + 1, _y, _x + 1, _corner, _cornerConceal);
                TargetPointObscurement(observer, source, _iSect, sense, _z + 1, _y + 1, _x, _corner, _cornerConceal);
                TargetPointObscurement(observer, source, _iSect, sense, _z + 1, _y + 1, _x + 1, _corner, _cornerConceal);

                // if all from source to cell's corners are clear, no cover
                // NOTE: cannot have any blocked, and at least one must be clear
                var _cover = ((_corner[_z][_y][_x] < 2) && (_corner[_z][_y][_x + 1] < 2)
                    && (_corner[_z][_y + 1][_x] < 2) && (_corner[_z][_y + 1][_x + 1] < 2)
                    && (_corner[_z + 1][_y][_x] < 2) && (_corner[_z + 1][_y][_x + 1] < 2)
                    && (_corner[_z + 1][_y + 1][_x] < 2) && (_corner[_z + 1][_y + 1][_x + 1] < 2)
                    && ((_corner[_z][_y][_x] == 1) || (_corner[_z][_y][_x + 1] == 1)
                        || (_corner[_z][_y + 1][_x] == 1) || (_corner[_z][_y + 1][_x + 1] == 1)
                        || (_corner[_z + 1][_y][_x] == 1) || (_corner[_z + 1][_y][_x + 1] == 1)
                        || (_corner[_z + 1][_y + 1][_x] == 1) || (_corner[_z + 1][_y + 1][_x + 1] == 1))
                    )
                    ? false
                    : true;

                // any concealed corner conceals the cell
                var _concealment = _cornerConceal[_z][_y][_x] || _cornerConceal[_z][_y][_x + 1]
                            || _cornerConceal[_z][_y + 1][_x] || _cornerConceal[_z][_y + 1][_x + 1]
                            || _cornerConceal[_z + 1][_y][_x] || _cornerConceal[_z + 1][_y][_x + 1]
                            || _cornerConceal[_z + 1][_y + 1][_x] || _cornerConceal[_z + 1][_y + 1][_x + 1];
                if (!_cover && !_concealment)
                {
                    // found a cell lacking cover and concealment
                    return false;
                }
            }

            // didn't find an unencumbered cell, therefore isobscured
            return true;
        }
        #endregion

        #region public void VisibilityForSense(SensoryBase sense, out bool visible, out bool shadowShroud)
        /// <summary>Determines visibility for sense, regardless of range or interference</summary>
        public (bool visible, bool shadowShroud) VisibilityForSense(SensoryBase sense)
        {
            // determine visual light and shadow
            if (sense.UsesSight)
            {
                if (!sense.IgnoresVisualEffects && InMagicalDarkness)
                {
                    // Shadow Shrouded is applied to this sense (and magical darkness is lit up)
                    return (true, true);
                }
                else if (sense.UsesLight)
                {
                    // Standard Light-Level check
                    if (sense.LowLight)
                    {
                        // determine how much light is detectable
                        switch (LightLevel)
                        {
                            case LightRange.ExtentBoost:
                            case LightRange.FarShadow:
                                return (true, true);

                            case LightRange.OutOfRange:
                                // assume not lit and shrouded in normal shadows or magical darkness
                                return (false, true);
                        }
                        return (true, false);
                    }

                    switch (LightLevel)
                    {
                        case LightRange.FarBoost:
                        case LightRange.NearShadow:
                            return (true, true);

                        case LightRange.OutOfRange:
                        case LightRange.ExtentBoost:
                        case LightRange.FarShadow:
                            // assume not lit and shrouded in normal shadows or magical darkness
                            return (false, true);
                    }
                    return (true, false);
                }

                // Darkvision cannot be shrouded in non-magical darkness
                return (true, false);
            }

            // non sight-based senses cannot be shrouded in shadows
            return (true, false);
        }
        #endregion

        #region public MovementBase ActiveMovement { get; set; }
        /// <summary>Last locomoted movement mode on the locator (only set in Relocation Step)</summary>
        public MovementBase ActiveMovement
        {
            // NOTE: this can be attached from another object
            //       once removed from context, the object being located will disconnect from the locator
            get
            {
                if (_Movement == null)
                {
                    _Movement = (Chief as Creature)?.Movements.AllMovements.FirstOrDefault(_m => _m.IsUsable);
                    _Movement ??= new LandMovement(ICore as CoreObject, this);
                }
                return _Movement;
            }
            set
            {
                var _last = _Movement;
                _Movement = value;
                if ((_last != _Movement) && (_last != null))
                {
                    _last.OnEndActiveMovement();
                }
            }
        }
        #endregion

        #region public AnchorFaceList MovementCrossings { get; set; }
        /// <summary>Last locomoted movement direction made on the locator (only set in Relocation Step)</summary>
        public AnchorFaceList MovementCrossings
        {
            get { return _MoveCrossings; }
            set { _MoveCrossings = value; }
        }
        #endregion

        /// <summary>Last movement vector (in reverse)</summary>
        public Vector3D MoveFrom => _MoveFrom;

        /// <summary>SerialState for last relocation</summary>
        public ulong SerialState => _SerialState;

        private List<LocalCellGroup> CellGroups
            => _CellGroups ?? RecalcLocalCellGroups().groups;

        /// <summary>groups connected via links</summary>
        public List<LocalCellGroup> Links
            => _Links ?? RecalcLocalCellGroups().links;

        protected (List<LocalCellGroup> groups, List<LocalCellGroup> links) RecalcLocalCellGroups()
        {
            _CellGroups = (from _cell in GeometricRegion.AllCellLocations()
                           let _group = Map.GetLocalCellGroup(_cell)
                           where _group != null
                           select _group).Distinct().ToList();
            _Links = (from _g in _CellGroups
                      from _l in _g.Links.All
                      where GeometricRegion.ContainsGeometricRegion(_l.LinkCube)
                      select _l.OutsideGroup(_g))
                      .Distinct()
                      .ToList();
            return (_CellGroups, _Links);
        }

        /// <summary>Gets canonical local cell groups in which the Locator has a presence.</summary>
        public IEnumerable<LocalCellGroup> CalculateLocalCellGroups()
            => RecalcLocalCellGroups().groups.Select(_lcg => _lcg);

        /// <summary>Gets cached local cell groups (stored with locator)</summary>
        public IEnumerable<LocalCellGroup> GetLocalCellGroups()
            => CellGroups.Select(_lcg => _lcg);

        /// <summary>True if any cell group is part of the background</summary>
        public bool HasBackgroundPresence()
            => (from _grp in GetLocalCellGroups()
                where _grp.IsPartOfBackground
                select _grp).FirstOrDefault() != null;

        #region IControlChange<IGeometricRegion> Members
        public virtual void AddChangeMonitor(IMonitorChange<IGeometricRegion> monitor)
        {
            _RegionCtrl.AddChangeMonitor(monitor);
        }

        public virtual void RemoveChangeMonitor(IMonitorChange<IGeometricRegion> monitor)
        {
            _RegionCtrl.RemoveChangeMonitor(monitor);
        }
        #endregion

        #region IControlChange<ICellLocation> Members
        public virtual void AddChangeMonitor(IMonitorChange<ICellLocation> monitor)
        {
            _LocationCtrl.AddChangeMonitor(monitor);
        }

        public virtual void RemoveChangeMonitor(IMonitorChange<ICellLocation> monitor)
        {
            _LocationCtrl.RemoveChangeMonitor(monitor);
        }
        #endregion

        #region IControlChange<IGeometricSize> Members
        public void AddChangeMonitor(IMonitorChange<IGeometricSize> monitor)
        {
            _CurrentSizeCtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<IGeometricSize> monitor)
        {
            _CurrentSizeCtrl.RemoveChangeMonitor(monitor);
        }
        #endregion

        public IEnumerable<CoreObject> IlluminableObjects
            => ICoreAs<CoreObject>();

        public IIllumination BestIlluminator { get => _BestLight; set => _BestLight = value; }

        #region anchor faces
        /// <summary>
        /// for locator orientation and sensor host alignment
        /// </summary>
        public AnchorFace BaseFace
        {
            get
            {
                return _BaseFace == AnchorFaceList.None
                    ? GetGravityFace()
                    : _BaseFace.ToAnchorFaces().First();
            }
            set
            {
                _BaseFace = value.ToAnchorFaceList();
            }
        }

        /// <summary>Gravity won't work on ethereal locators, even though it is "presnet"</summary>
        public bool IsGravityEffective
            => !_Ethereal;

        #region public AnchorFace GetGravityFace()
        public AnchorFace GetGravityFace()
        {
            // TODO: figure effect of all gravity vectors for all cells
            return (from _cell in GeometricRegion.AllCellLocations()
                    let _face = Map.GetGravityFace(_cell)
                    group _face by _face)
                    .OrderByDescending(_f => _f.Count())
                    .FirstOrDefault()
                    ?.Key ?? AnchorFace.ZLow;
        }
        #endregion
        #endregion

        #region private void ResyncAllSensorHosts()
        private void ResyncAllSensorHosts()
        {
            foreach (var _sensors in ICoreAs<ISensorHost>())
            {
                _sensors.AimPoint = ResyncTacticalPoint(_sensors, _sensors.AimPointRelativeLongitude, _sensors.AimPointLatitude, _sensors.AimPointDistance);
                _sensors.ThirdCameraPoint = ResyncTacticalPoint(_sensors, _sensors.ThirdCameraRelativeHeading * 45d,
                    _sensors.ThirdCameraIncline * 45d, _sensors.ThirdCameraDistance);

                // sound awarenesses of locator should be refreshed in new location
                RefreshSoundAwareness((id) => true);
            }
        }
        #endregion

        #region public void AdjustAimPoint(ISensorHost sensorHost, short zOff, short yOff, short xOff)
        /// <summary>Creature has attempted to adjust aim point by offset</summary>
        public void AdjustAimPoint(ISensorHost sensorHost, short zOff, short yOff, short xOff)
        {
            var _aimPoint = sensorHost.AimPoint;

            // offset from original (by small amount)
            var _zO = (zOff > 0) ? double.Epsilon : ((zOff < 0) ? -double.Epsilon : 0d);
            var _yO = (yOff > 0) ? double.Epsilon : ((yOff < 0) ? -double.Epsilon : 0d);
            var _xO = (xOff > 0) ? double.Epsilon : ((xOff < 0) ? -double.Epsilon : 0d);

            _aimPoint.Offset(_xO, _yO, _zO);

            #region point test (no backtracking)
            bool _goodOrdinate(double testVal, double baseVal, short comparison)
                => (comparison == 0)
                    || ((comparison > 0) && (testVal > baseVal))
                    || ((comparison < 0) && (testVal < baseVal));

            bool _goodPoint(Point3D testPoint)
                => _goodOrdinate(testPoint.Z, _aimPoint.Z, zOff)
                    && _goodOrdinate(testPoint.Y, _aimPoint.Y, yOff)
                    && _goodOrdinate(testPoint.X, _aimPoint.X, xOff);
            #endregion

            if ((ActiveMovement != null) && (Map != null))
            {
                // get nearest good point
                var _points = (from _cLoc in GeometricRegion.AllCellLocations().AsParallel()
                               let _space = Map[_cLoc]
                               let _clp = _cLoc.Point3D()
                               let _clv = new Vector3D(_clp.X, _clp.Y, _clp.Z)
                               from _tp in _space.TacticalPoints(ActiveMovement)
                               let _pt = _tp + _clv
                               where _goodPoint(_pt)
                               orderby (_pt - _aimPoint).LengthSquared
                               select _pt).Take(1).ToList();

                if (_points.Any())
                {
                    // set aim point if a new one was found
                    sensorHost.AimPoint = _points.First();

                    // sensor-host base point
                    var _basePt = new Point3D(
                        (GeometricRegion.LowerX * 5) + sensorHost.XOffset,
                        (GeometricRegion.LowerY * 5) + sensorHost.YOffset,
                        (GeometricRegion.LowerZ * 5) + sensorHost.ZOffset);

                    // aim vector (relative to base point) and distance
                    var _aimVector = sensorHost.AimPoint - _basePt;
                    sensorHost.AimPointDistance = _aimVector.Length;

                    // calculate relative angles
                    var _aimHeading = new Vector3D();
                    var _zeroHeading = new Vector3D();
                    var _baseLongitude = (sensorHost.Heading * 45d);
                    var _gravity = BaseFace;
                    var _axis = new Vector3D();
                    switch (_gravity)
                    {
                        case AnchorFace.ZLow:
                        case AnchorFace.ZHigh:
                            // aim heading is projection of aimPoint onto conceptual heading plane
                            _aimHeading = (new Point3D(sensorHost.AimPoint.X, sensorHost.AimPoint.Y, _basePt.Z)) - _basePt;

                            // zero heading is projection of "heading 0" onto conceptual heading plane
                            _zeroHeading = new Vector3D(sensorHost.AimPointDistance, 0, 0);

                            // axis
                            _axis = new Vector3D(0, 0, 1);
                            break;

                        case AnchorFace.YLow:
                        case AnchorFace.YHigh:
                            // aim heading is projection of aimPoint onto conceptual heading plane
                            _aimHeading = (new Point3D(sensorHost.AimPoint.X, _basePt.Y, sensorHost.AimPoint.Z)) - _basePt;

                            // zero heading is projection of "heading 0" onto conceptual heading plane
                            _zeroHeading = new Vector3D(0, 0, sensorHost.AimPointDistance);

                            // axis
                            _axis = new Vector3D(0, 1, 0);
                            break;

                        case AnchorFace.XLow:
                        case AnchorFace.XHigh:
                            // aim heading is projection of aimPoint onto conceptual heading plane
                            _aimHeading = (new Point3D(_basePt.X, sensorHost.AimPoint.Y, sensorHost.AimPoint.Z)) - _basePt;

                            // zero heading is projection of "heading 0" onto conceptual heading plane
                            _zeroHeading = new Vector3D(0, sensorHost.AimPointDistance, 0);

                            // axis
                            _axis = new Vector3D(1, 0, 0);
                            break;
                    }

                    // latAxis is aim heading crossed to gravity reference axis
                    var _latAxis = Vector3D.CrossProduct(_aimHeading, _axis);
                    sensorHost.AimPointLatitude = _latAxis.AxisAngleBetween(_aimHeading, _aimVector);

                    // relative longitude (from heading)
                    sensorHost.AimPointRelativeLongitude = _axis.AxisAngleBetween(_zeroHeading, _aimHeading) - _baseLongitude;
                }
            }
        }
        #endregion

        #region public Point3D MiddlePoint { get; }
        /// <summary>Middle point using gravity face and intraModelOffset</summary>
        public Point3D MiddlePoint
        {
            get
            {
                var _pt = CenterPoint + IntraModelOffset;
                switch (BaseFace)
                {
                    case AnchorFace.XLow:
                        _pt.X = _Region.LowerX * 5d + (XFit / 2) + IntraModelOffset.X;
                        break;
                    case AnchorFace.XHigh:
                        _pt.X = ((_Region.UpperX + 1) * 5d) - (XFit / 2) + IntraModelOffset.X;
                        break;
                    case AnchorFace.YLow:
                        _pt.Y = _Region.LowerY * 5d + (YFit / 2) + IntraModelOffset.Y;
                        break;
                    case AnchorFace.YHigh:
                        _pt.Y = ((_Region.UpperY + 1) * 5d) - (YFit / 2) + IntraModelOffset.Y;
                        break;
                    case AnchorFace.ZLow:
                        _pt.Z = _Region.LowerZ * 5d + (ZFit / 2) + IntraModelOffset.Z;
                        break;
                    case AnchorFace.ZHigh:
                    default:
                        _pt.Z = ((_Region.UpperZ + 1) * 5d) - (ZFit / 2) + IntraModelOffset.Z;
                        break;
                }
                return _pt;
            }
        }
        #endregion

        #region public Point3D ResyncTacticalPoint(ISensorHost sensorHost)
        /// <summary>ISensorHost has rotated or moved, and needs new tactical point</summary>
        public Point3D ResyncTacticalPoint(ISensorHost sensorHost, double relativeLongitude, double latitude, double distance)
        {
            // indicates the sensor-host start point
            var _start = MiddlePoint;

            // distance length heading directed vector
            // ... rotated "up-down" (ref gravity) by latitude
            // ... and around by relative longitude
            var _aimVector = new Vector3D();
            RotateTransform3D _rotate = null;
            var _longitude = (sensorHost.Heading * 45d) + relativeLongitude;
            var _baseFace = BaseFace;
            var _lowFace = _baseFace.IsLowFace();
            switch (_baseFace)
            {
                case AnchorFace.ZLow:
                case AnchorFace.ZHigh:
                    _aimVector = new Vector3D(distance, 0, 0);
                    _rotate = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, _lowFace ? -1 : 1, 0), latitude));
                    _aimVector = _rotate.Transform(_aimVector);
                    _rotate = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, _lowFace ? 1 : -1), _longitude));
                    _aimVector = _rotate.Transform(_aimVector);
                    break;

                case AnchorFace.YLow:
                case AnchorFace.YHigh:
                    _aimVector = new Vector3D(distance, 0, 0);
                    _rotate = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, _lowFace ? 1 : -1), latitude));
                    _aimVector = _rotate.Transform(_aimVector);
                    _rotate = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, _lowFace ? 1 : -1, 0), _longitude));
                    _aimVector = _rotate.Transform(_aimVector);
                    break;

                case AnchorFace.XLow:
                case AnchorFace.XHigh:
                    _aimVector = new Vector3D(0, distance, 0);
                    _rotate = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, _lowFace ? -1 : 1), latitude));
                    _aimVector = _rotate.Transform(_aimVector);
                    _rotate = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(_lowFace ? 1 : -1, 0, 0), _longitude - 90));
                    _aimVector = _rotate.Transform(_aimVector);
                    break;
            }

            // ending point of aiming vector from start point
            var _end = _start + _aimVector;
            if ((ActiveMovement != null) && (Map != null))
            {
                // sort points by length to endpoint (points for each cell location can be done in parallel)
                var _points = (from _cLoc in GeometricRegion.AllCellLocations().AsParallel()
                               let _space = Map[_cLoc]
                               let _clp = _cLoc.Point3D()
                               let _clv = new Vector3D(_clp.X, _clp.Y, _clp.Z)
                               from _tp in _space.TacticalPoints(ActiveMovement)
                               let _pt = _tp + _clv
                               orderby (_pt - _end).LengthSquared
                               select _pt).Take(1).ToList();

                // first found (nearest) point
                if (_points.Any())
                {
                    return _points.First();
                }
                else
                {
                    return _start;
                }
            }
            else
            {
                return _start;
            }
        }
        #endregion

        // TODO: implement willContinueFall...
        // TODO: implement canClimb and climbDifficulty (also used for catching self while falling)

        #region public void RefreshSoundAwareness(Func<(SoundRef sound, IGeometricRegion region, double magnitude), bool> soundFilter)
        public void RefreshSoundAwareness(Func<Guid, bool> soundFilter)
        {
            /* update sensor host info with sounds */
            if (ICore is ISensorHost _host
                && _host.IsSensorHostActive)
            {
                // sound references, grouped by sources and ordered by relative magnitude
                _host.SoundAwarenesses.ResolveSounds(
                    (from _gsr in (from _isolated in GetLocalCellGroups()
                                   where !_isolated.IsPartOfBackground
                                   from _isr in _isolated.GetSoundRefs(this, soundFilter)
                                   select _isr)
                                   .Union(from _background in GetLocalCellGroups().Where(_lg => _lg.IsPartOfBackground).Take(1)
                                          from _bsr in _background.GetSoundRefs(this, soundFilter)
                                          select _bsr)
                     group _gsr by _gsr.sound.Audible.SoundGroupID)
                    .ToDictionary(
                        _sGrp => _sGrp.Key,
                        _sGrp => _sGrp.OrderByDescending(_sr3 => _sr3.magnitude).ToList()), soundFilter);

                // cleanup anything out of time
                _host.SoundAwarenesses.Cleanup();
            }
        }
        #endregion

        /// <summary>Center anchoring location</summary>
        public ICellLocation Location
            => LocationAimMode == LocationAimMode.Cell
            ? new CellLocation((GeometricRegion.LowerZ + GeometricRegion.UpperZ) / 2,
                    (GeometricRegion.LowerY + GeometricRegion.UpperY) / 2,
                    (GeometricRegion.LowerX + GeometricRegion.UpperX) / 2)
            : new CellLocation((GeometricRegion.LowerZ + GeometricRegion.UpperZ + 1) / 2,
                    (GeometricRegion.LowerY + GeometricRegion.UpperY + 1) / 2,
                    (GeometricRegion.LowerX + GeometricRegion.UpperX + 1) / 2);

        public LocationAimMode LocationAimMode
        => ((NormalSize?.XLength ?? 1) % 2) == 1
            ? LocationAimMode.Cell
            : LocationAimMode.Intersection;

        // ICorePart Members
        public IEnumerable<ICorePart> Relationships
            => ICoreAs<ICore>().OfType<ICorePart>();

        public string TypeName
            => GetType().FullName;

        #region public double ZFit { get; }
        public double ZFit
        {
            get
            {
                if (ICoreAs<ICorePhysical>().Any())
                {
                    if (GetGravityFace().GetAxis() == Axis.Z)
                    {
                        return ICoreAs<ICorePhysical>().Max(_icp => _icp.Height);
                    }
                    else
                    {
                        return ICoreAs<ICorePhysical>().Max(_icp => _icp.Width);
                    }
                }

                return 0d;
            }
        }
        #endregion

        #region public double YFit { get; }
        public double YFit
        {
            get
            {
                if (ICoreAs<ICorePhysical>().Any())
                {
                    if (GetGravityFace().GetAxis() == Axis.Y)
                    {
                        return ICoreAs<ICorePhysical>().Max(_icp => _icp.Height);
                    }
                    else
                    {
                        return ICoreAs<ICorePhysical>().Max(_icp => _icp.Width);
                    }
                }

                return 0d;
            }
        }
        #endregion

        #region public double XFit { get; }
        public double XFit
        {
            get
            {
                if (ICoreAs<ICorePhysical>().Any())
                {
                    if (GetGravityFace().GetAxis() == Axis.X)
                    {
                        return ICoreAs<ICorePhysical>().Max(_icp => _icp.Height);
                    }
                    else
                    {
                        return ICoreAs<ICorePhysical>().Max(_icp => _icp.Width);
                    }
                }

                return 0d;
            }
        }
        #endregion

        /// <summary>Size the locator is supposed to be without squeezing or extending</summary>
        public IGeometricSize NormalSize
            => (ICore as ISizable)?.GeometricSize;

        /// <summary>Indicates that the current cube isn't big enough for the current size</summary>
        public bool IsSqueezing => _MustSqueeze;

        #region public bool SqueezesIntoGeometry(IGeometricRegion region)
        /// <summary>True if the geometry is narrower than the Locator's requirements</summary>
        public bool SqueezesIntoGeometry(IGeometricRegion region)
        {
            if (region is CellList _list)
            {
                return (_list.XNarrowest < Math.Min(XFit, NormalSize.XExtent * 5))
                    || (_list.YNarrowest < Math.Min(YFit, NormalSize.YExtent * 5))
                    || (_list.ZNarrowest < Math.Min(ZFit, NormalSize.ZExtent * 5));
            }
            return false;
        }
        #endregion

        #region private void EnsureSqueeze()
        private void EnsureSqueeze()
        {
            if (SqueezesIntoGeometry(GeometricRegion))
            {
                if (!_MustSqueeze)
                {
                    _MustSqueeze = true;
                    // squeeze
                    foreach (var _actor in ICoreAs<Creature>().Where(_a => !_a.HasActiveAdjunct<Squeezing>())
                        .ToList())
                    {
                        // add squeezings
                        _actor.AddAdjunct(new Squeezing());
                    }
                }
            }
            else
            {
                if (_MustSqueeze)
                {
                    _MustSqueeze = false;
                    // do not squeeze
                    foreach (var _actor in ICoreAs<Creature>().Where(_a => _a.HasActiveAdjunct<Squeezing>())
                        .ToList())
                    {
                        // remove any squeezing
                        _actor.RemoveAdjunct(_actor.GetAllConnectedAdjuncts<Squeezing>().FirstOrDefault());
                    }
                }
            }
        }
        #endregion

        #region public IEnumerable<Presentation> GetPresentations(IGeometricRegion location, Creature critter, ISensorHost sensors)
        public IEnumerable<Presentation> GetPresentations(IGeometricRegion location, Creature critter, ISensorHost sensors,
            IList<SensoryBase> filteredSenses)
        {
            if (((Chief != null) && (sensors?.Awarenesses.ShouldDraw(Chief.ID) ?? true))
                || ((Chief == null) && ICoreAs<ICore>().Any(_ic => (sensors?.Awarenesses.ShouldDraw(_ic.ID) ?? true))))
            {
                // must have something to interact with
                IInteract _iTarget = Chief ?? ICoreAs<IInteract>().FirstOrDefault();
                if (_iTarget == null)
                {
                    yield break;
                }

                var _planar = critter.GetPlanarPresence();

                // get model data
                var _visModelData = new VisualPresentationData(critter, location, sensors, filteredSenses, this);
                var _vmdInteract = new Interaction(critter, critter, _iTarget, _visModelData);
                _iTarget.HandleInteraction(_vmdInteract);

                // process each feedback
                foreach (var _vpBack in _vmdInteract.Feedback.OfType<VisualPresentationFeedback>())
                {
                    if (_vpBack.IsPresentable)
                    {
                        // get visual effect(s)
                        var _visEffectData = new VisualEffectData(critter, location, this, filteredSenses, _vpBack.Presentation, _planar);
                        var _vedInteract = new Interaction(critter, critter, _iTarget, _visEffectData);
                        _iTarget.HandleInteraction(_vedInteract);

                        // store visual effects
                        var _skip = false;
                        var _vedBack = _vedInteract.Feedback.OfType<VisualEffectFeedback>().FirstOrDefault();
                        if (_vedBack != null)
                        {
                            // copy feedback effects to presentation
                            foreach (var _eff in _vedBack.VisualEffects)
                            {
                                if (_eff.Value == VisualEffect.Skip)
                                {
                                    _skip = true;
                                    break;
                                }
                                else if ((_eff.Value == VisualEffect.Unseen)
                                    && ICoreAs<ICore>().Any(_ic => (sensors?.Awarenesses.GetAwarenessLevel(_ic.ID) ?? AwarenessLevel.Aware) == AwarenessLevel.Aware))
                                {
                                    _vpBack.Presentation.VisualEffects.Add(new VisualEffectValue(_eff.Key, VisualEffect.MonochromeDim));
                                }
                                else
                                {
                                    _vpBack.Presentation.VisualEffects.Add(new VisualEffectValue(_eff.Key, _eff.Value));
                                }
                            }
                        }

                        // return
                        if (!_skip)
                        {
                            yield return _vpBack.Presentation;
                        }
                    }
                }
            }
            yield break;
        }
        #endregion

        public IEnumerable<Locator> OverlappedLocators(PlanarPresence locPlanes)
            => MapContext?.LocatorsInRegion(GeometricRegion, locPlanes).Where(_loc => _loc != this);

        public IEnumerable<TType> GetCapturable<TType>() where TType : ICore
            => (from _c in ICoreAs<TType>()
                where !(_c is ICapturePassthrough)
                select _c).Union(from _c in ICoreAs<ICapturePassthrough>()
                                 from _pi in _c.Contents.OfType<TType>()
                                 select _pi);

        public (LocalMap map, List<Guid> notifiers) GetDeferredRefreshObservers()
            => (Map, GetLocalCellGroups().ToList()
                .SelectMany(_g => _g.GetSensorKeys())
                .Distinct()
                .ToList());

        public void RefreshObservers()
        {
            var (_map, _notifiers) = GetDeferredRefreshObservers();
            AwarenessSet.RecalculateAllSensors(_map, _notifiers, false);
        }
    }
}
