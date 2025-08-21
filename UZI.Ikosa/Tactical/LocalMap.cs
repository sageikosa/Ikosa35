using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Media.Media3D;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Universal;
using Uzi.Visualize;
using System.Threading;
using Uzi.Ikosa.Movement;
using System.Threading.Tasks;
using Uzi.Packaging;
using Uzi.Visualize.Packaging;
using Uzi.Visualize.Geometry3D;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using Uzi.Ikosa.Guildsmanship;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class LocalMap : CoreSetting, IModuleNode, IBasePart, ICorePartNameManager, INotifyPropertyChanged,
        ICreatureProvider, IInteractProvider, ITacticalMap
    {
        #region state
        // items expressed as related parts
        [NonSerialized, JsonIgnore]
        private PackagePart _MapPart;
        [NonSerialized, JsonIgnore]
        private ShadingZoneSet _Shadings = null;
        [NonSerialized, JsonIgnore]
        private ICorePartNameManager _NameManager = null;
        [NonSerialized, JsonIgnore]
        private VisualResources _Resources = null;

        // configuration control
        private Dictionary<string, CellMaterial> _CellMaterials = [];
        private Dictionary<Guid, string> _NamedKeys = [];
        private CellSpaceSet _Spaces = null;
        private PanelPalette _Panels = null;

        // semi-morphable geometry
        private Dictionary<string, AmbientLight> _AmbientLights = [];
        private RoomTracker _Tracker = null;
        private BackgroundCellGroupSet _Backgrounds;
        private RoomSet _Rooms;

        // items defined in map
        private Cubic _Viewport = new Cubic(0, 0, 0, 10, 10, 10);

        private double _CurrentTime;
        private bool _Initialized = false;

        [NonSerialized, JsonIgnore]
        private ReaderWriterLockSlim _SyncLock = null;
        #endregion

        /// <summary>Relationship type to identify a local map object part (http://pack.guildsmanship.com/ikosa/localmap)</summary>
        public const string IkosaMapRelation = @"http://pack.guildsmanship.com/ikosa/localmap";

        #region Constructors
        public LocalMap()
            : base(string.Empty)
        {
            _Backgrounds = new BackgroundCellGroupSet(this);
            _Rooms = new RoomSet(this);
            _Shadings = new ShadingZoneSet(this);
            _Spaces = [];
            _Panels = new PanelPalette();
            _Resources = new VisualResources(this, @"Resources");
            ContextSet.Add(new MapContext(@"Prime", this));
        }

        /// <summary>Load a map package part from a package part</summary>
        public static LocalMap GetLocalMap(PackagePart part)
        {
            if (part != null)
            {
                using var _ctxStream = part.GetStream(FileMode.Open, FileAccess.Read);
                IFormatter _fmt = new BinaryFormatter();
                var _map = (LocalMap)_fmt.Deserialize(_ctxStream);
                _map.SetPackagePart(part);

                // shouldn't be necessary (ultimately)
                _map.MapContext.LocatorZones.Remove(_map.MapContext.LocatorZones.AllCaptures().FirstOrDefault());
                _map.ContextSet.RebuildCoreIndex();
                _map.IkosaProcessManager.CleanupTrackers(_map.MapContext);
                return _map;
            }

            return null;
        }

        protected void SetPackagePart(PackagePart part)
        {
            _SyncLock = null;
            _MapPart = part;
            _Resources = part.GetRelationships().RelatedBaseParts(this).OfType<VisualResources>().FirstOrDefault();
            _Resources ??= new VisualResources(this, @"Resources");

            BasePartHelper.LoadMessage?.Invoke($@"LocalMap[{Name}] --> ShadingZoneSet");
            _Shadings = new ShadingZoneSet(this);

            // reconnect/cleanup adjunct groups
            BasePartHelper.LoadMessage?.Invoke($@"LocalMap[{Name}] --> Adjunct Groups");
            ContextSet.RebindAllAdjuncts();

            BasePartHelper.LoadMessage?.Invoke($@"LocalMap[{Name}] --> RecacheShadingZones");
            ShadingZones.RecacheShadingZones();

            BasePartHelper.LoadMessage?.Invoke($@"LocalMap[{Name}] --> ReshadeBackground");
            ShadingZones.ReshadeBackground();

            BasePartHelper.LoadMessage?.Invoke($@"LocalMap[{Name}] --> RefreshAwarenesses");
            MapContext.RefreshAwarenesses();
        }
        #endregion

        public Dictionary<string, AmbientLight> AmbientLights => _AmbientLights;
        public Dictionary<string, CellMaterial> CellMaterials => _CellMaterials;
        public RoomSet Rooms => _Rooms;
        public BackgroundCellGroupSet Backgrounds => _Backgrounds;
        public CellSpaceSet CellSpaces => _Spaces;

        public Description Description => new Description(Name);

        public RoomTracker RoomIndex
            => _Tracker ??= new RoomTracker(this);

        public VisualResources Resources => _Resources;

        /// <summary>Used by Workshop to DataBind CellSpaces for Room Editor</summary>
        public IEnumerable<CellSpace> AllCellSpaces
            => _Spaces.Select(_s => _s);

        public IEnumerable<CellMaterial> AllCellMaterials
            => _CellMaterials.Select(_kvp => _kvp.Value).OrderBy(_v => _v.Name);

        public PanelPalette Panels
            => _Panels ??= new PanelPalette();

        public IEnumerable<LocalCellGroup> AllLocalCellGroups
            => Rooms.Select(_r => _r as LocalCellGroup).Union(Backgrounds.All());

        public LocalCellGroup GetLocalCellGroup(Guid id)
            => (Rooms[id] as LocalCellGroup) ?? Backgrounds.FirstOrDefault(_b => _b.ID == id);

        #region public void RelightMap()
        /// <summary>Recalculates all links and re-lights everything</summary>
        public void RelightMap()
        {
            // clear all old
            var _groups = _Rooms.AsEnumerable<LocalCellGroup>().Union(_Backgrounds.All()).ToList();
            Parallel.ForEach(_groups, (_group) =>
                {
                    _group.Links.Clear();
                });

            // add all new
            foreach (var _room in _groups.OfType<Room>())
            {
                _room.AddLinks();
            }

            // let every local cell group propogate before finalizing locator effects and cell shadings
            // NOTE: notifyLighting = false allows the following three blocks to run from here for all groups
            var _notifiers = _groups
                .SelectMany(_g => _g.NotifyLighting(false))
                .Distinct()
                .ToList();

            // update all locators
            Parallel.ForEach(MapContext.AllTokensOf<Locator>(), (_loc) =>
            {
                _loc.DetermineIllumination();
            });

            Parallel.ForEach(_groups.OfType<Room>(), (_room) =>
            {
                // update each room shading
                _room.RefreshTerrainShading();
            });

            // update background shading
            var _bgGroup = _groups.OfType<BackgroundCellGroup>().FirstOrDefault();
            if (_bgGroup != null)
            {
                _bgGroup.RefreshTerrainShading();
            }

            // TODO: should probably just do for all sensors...
            AwarenessSet.RecalculateAllSensors(this, _notifiers, false);
        }
        #endregion

        public ShadingZoneSet ShadingZones => _Shadings;

        /// <summary>ReaderWriterLock used to synchronize access</summary>
        public ReaderWriterLockSlim Synchronizer { get => _SyncLock; set => _SyncLock = value; }

        /// <summary>Key guids defined for this local map</summary>
        public Dictionary<Guid, string> NamedKeyGuids => _NamedKeys;

        /// <summary>All background groups (theoretically shared with the sharedSource)</summary>
        public IEnumerable<LocalCellGroup> GetSharedGroups(LocalCellGroup sharedSource)
            => Backgrounds.All().AsEnumerable<LocalCellGroup>().Union(Rooms.Where(_r => _r.IsPartOfBackground));

        /// <summary>Gets the first (canonical) local cell group (room or background) that contains the cell.</summary>
        public LocalCellGroup GetLocalCellGroup(ICellLocation location)
            => (RoomIndex.GetRoom(location) as LocalCellGroup)
            ?? Backgrounds.GetBackgroundCellGroup(location);

        // Map Contents

        /// <summary>Object locators, effects, reactors, and interaction transit alterers</summary>
        public MapContext MapContext
            => ContextSet[0] as MapContext;

        public IkosaProcessManager IkosaProcessManager
            => ContextSet.ProcessManager as IkosaProcessManager;

        // TODO: initialize this somewhow...
        public double CurrentTime => _CurrentTime;

        /// <summary>Gets time marking start of current day</summary>
        public double StartOfDay => Day.StartOfDay(CurrentTime);

        /// <summary>Gets time marking end of current day (or start of next day)</summary>
        public double EndOfDay => Day.EndOfDay(CurrentTime);

        #region public void TimeTick(double time)
        /// <summary>Signal all time tracking objects and adjuncts that time has changed</summary>
        public void TimeTick(double time)
        {
            void _doTick(TimeValTransition direction)
            {
                // notify all time tracking objects and adjuncts
                foreach (var _core in from _ctx in ContextSet
                                      from _c in GetAllICore<ICore>().ToList()
                                      select _c)
                {
                    if (_core is ITrackTime _tTrack)
                    {
                        _tTrack.TrackTime(CurrentTime, direction);
                    }

                    if (_core is IAdjunctable _iAdj)
                    {
                        foreach (var _adj in _iAdj.Adjuncts.OfType<ITrackTime>().ToList())
                        {
                            _adj.TrackTime(CurrentTime, direction);
                        }
                    }
                }
            };

            // tick leaving current time
            _doTick(TimeValTransition.Leaving);

            // change current time
            _CurrentTime = time;

            // tick entering new time
            _doTick(TimeValTransition.Entering);

            ClearTransientVisualizers();
            DoPropertyChanged(nameof(CurrentTime));
        }
        #endregion

        #region public void ClearTransientVisualizers()
        public void ClearTransientVisualizers()
        {
            // clear transient visualizers
            foreach (var _ctx in ContextSet.OfType<MapContext>())
            {
                _ctx.TransientVisualizers.Clear();
            }
        }
        #endregion

        // Spatial Queries

        #region public ref readonly CellStructure this[int z, int y, int x, IGeometricRegion currentRegion]
        public ref readonly CellStructure this[int z, int y, int x, IGeometricRegion currentRegion]
        {
            get
            {
                var _grp = (currentRegion as LocalCellGroup);
                if (_grp != null)
                {
                    ref readonly var _struct = ref _grp.GetCellSpace(z, y, x);
                    if (_struct.CellSpace != null)
                    {
                        return ref _struct;
                    }

                    // check linked rooms
                    foreach (var _room in _grp.Links.TouchingRooms)
                    {
                        ref readonly var _struc = ref _room.GetCellSpace(z, y, x);
                        if (_struc.CellSpace != null)
                        {
                            return ref _struc;
                        }
                    }
                }

                // check all rooms
                if (_grp?.IsPartOfBackground ?? false)
                {
                    var _room = RoomIndex.GetRoom(new CellPosition(z, y, x));
                    if (_room != null)
                    {
                        ref readonly var _struc = ref _room.GetCellSpace(z, y, x);
                        if (_struc.CellSpace != null)
                        {
                            return ref _struc;
                        }
                    }
                }

                // background cell-spaces
                foreach (var _bgSet in Backgrounds.All())
                {
                    if (_bgSet.ContainsCell(z, y, x))
                    {
                        return ref _bgSet.TemplateCell;
                    }
                }

                // no base cell-space anymore (it is in the backgrounds)
                return ref CellStructure.Default;
            }
        }
        #endregion

        #region public ref readonly CellStructure this[int z, int y, int x, AnchorFace adjacentTo] { get; }
        public ref readonly CellStructure this[int z, int y, int x, AnchorFace adjacentTo, IGeometricRegion currentRegion]
        {
            get
            {
                switch (adjacentTo)
                {
                    case AnchorFace.XHigh:
                        return ref this[z, y, x + 1, currentRegion];
                    case AnchorFace.XLow:
                        return ref this[z, y, x - 1, currentRegion];
                    case AnchorFace.YHigh:
                        return ref this[z, y + 1, x, currentRegion];
                    case AnchorFace.YLow:
                        return ref this[z, y - 1, x, currentRegion];
                    case AnchorFace.ZHigh:
                        return ref this[z + 1, y, x, currentRegion];
                    default:
                        return ref this[z - 1, y, x, currentRegion];
                }
            }
        }
        #endregion

        #region public ref readonly CellStructure this[ICellLocation location] { get; }
        public ref readonly CellStructure this[in ICellLocation location]
        {
            get
            {
                var _room = RoomIndex.GetRoom(location);
                if (_room != null)
                {
                    ref readonly var _cell = ref _room.GetCellSpace(location);
                    if (_cell.CellSpace != null)
                    {
                        return ref _cell;
                    }
                }

                // background cell-spaces
                foreach (var _bgSet in Backgrounds.All())
                {
                    if (_bgSet.ContainsCell(location))
                    {
                        return ref _bgSet.TemplateCell;
                    }
                }

                // no base cell-space anymore (it is in the backgrounds)
                return ref CellStructure.Default;
            }
        }
        #endregion

        #region public SegmentRef GetSegmentRef(int z, int y, int x, List<Room> rooms)
        public SegmentRef GetSegmentRef(int z, int y, int x, in DistantPoint3D entry, in DistantPoint3D exit)
        {
            var _room = RoomIndex.GetRoom(new CellPosition(z, y, x));
            if (_room != null)
            {
                var _struc = _room.GetCellSpace(z, y, x);
                if (_struc.CellSpace != null)
                {
                    return new SegmentRef(z, y, x, entry.Distance, _struc, entry.Point3D, exit.Point3D, _room);
                }
            }

            // background cell-spaces
            foreach (var _bgSet in Backgrounds.All())
            {
                if (_bgSet.ContainsCell(z, y, x))
                {
                    return new SegmentRef(z, y, x, entry.Distance, _bgSet.TemplateCell, entry.Point3D, exit.Point3D, null);
                }
            }

            // no base cell-space anymore (it is in the backgrounds)
            return default;
        }
        #endregion

        /// <summary>Gets cell space adjacent to location across the specified face</summary>
        public ref readonly CellStructure this[ICellLocation location, AnchorFace adjacentTo, LocalCellGroup currentGroup]
            => ref this[location.Z, location.Y, location.X, adjacentTo, currentGroup];

        #region private List<SegmentRef> SegmentCells(DistantPoint3D pt1, DistantPoint3D pt2, List<Room> rooms)
        private void _addCRef(List<SegmentRef> cRef, int z, int y, int x, in DistantPoint3D pt1, in DistantPoint3D pt2)
        {
            var _segRef = GetSegmentRef(z, y, x, pt1, pt2);
            if (_segRef.Contains(pt2.Point3D))
            {
                cRef.Add(_segRef);
            }
        }

        /// <summary>
        /// Returns the cells for the point.
        /// </summary>
        /// <param name="pt1">point to examine for contained or touching cells</param>
        /// <returns>array of CellRefs for that point</returns>
        /// <remarks>
        /// PointCells can return multiple CellRefs if the point falls on a cell face, edge, or vertex.  
        /// For a face, 2 CellRefs can be returned; for an edge, 4 CellRefs; and for a vertex, 8 CellRefs.
        /// </remarks>
        private List<SegmentRef> SegmentCellsX(in DistantPoint3D pt1, in DistantPoint3D pt2)
        {
            var _cRef = new List<SegmentRef>(8);
            var _dz = pt1.Point3D.Z / 5.0d;
            var _dy = pt1.Point3D.Y / 5.0d;
            var _dx = pt1.Point3D.X / 5.0d;
            var _iz = (int)Math.Floor(_dz);
            var _iy = (int)Math.Floor(_dy);
            var _ix = (int)Math.Floor(_dx);

            // converted back to double
            var _cz = (double)_iz;
            var _cy = (double)_iy;
            var _cx = (double)_ix;

            // the easy one
            _addCRef(_cRef, _iz, _iy, _ix, pt1, pt2);

            // check the distance from the original estimate to the back converted one, this tells us if it was on an edge
            if (_dz == _cz)
            {
                // on Z edge
                _addCRef(_cRef, _iz - 1, _iy, _ix, pt1, pt2);
                if (_dy == _cy)
                {
                    // on Z and Y edge
                    _addCRef(_cRef, _iz - 1, _iy - 1, _ix, pt1, pt2);
                    if (_dx == _cx)
                    {
                        // on Z, Y and X edge
                        _addCRef(_cRef, _iz - 1, _iy - 1, _ix - 1, pt1, pt2);
                    }
                }
                if (_dx == _cx)
                {
                    // on Z and X edge
                    _addCRef(_cRef, _iz - 1, _iy, _ix - 1, pt1, pt2);
                }
            }
            if (_dy == _cy)
            {
                // on Y edge
                _addCRef(_cRef, _iz, _iy - 1, _ix, pt1, pt2);
                if (_dx == _cx)
                {
                    // on Y and X edge
                    _addCRef(_cRef, _iz, _iy - 1, _ix - 1, pt1, pt2);
                }
            }
            if (_dx == _cx)
            {
                // on X Edge
                _addCRef(_cRef, _iz, _iy, _ix - 1, pt1, pt2);
            }
            return _cRef;
        }

        private void AddSegment(SegmentSet lineSet, in DistantPoint3D pt1, in DistantPoint3D pt2)
        {
            // TODO: handle multiple aligned non-segmented blockers (???)
            var _segCells = SegmentCellsX(pt1, pt2);
            if (_segCells.Count == 1)
            {
                var _sc = _segCells.First();
                lineSet.Add(in _sc);
            }
            else if (_segCells.Count == 0)
            {
                lineSet.Block();
            }
            else
            {
                // hugging a face or edge
                var _clear = false;

                // add unblocked cells contributing to travel path
                foreach (var _c in _segCells.Where(_lcr => !_lcr.IsPoint && !_lcr.BlocksPath))
                {
                    lineSet.Add(in _c);
                    _clear = true;
                }

                if (!_clear)
                {
                    // didn't find any, add what we've got...
                    foreach (var _c in _segCells)
                    {
                        lineSet.Add(in _c);
                    }
                }
                else
                {
                    // found some, so add non-distance cells in case they have tactical blockers
                    foreach (var _c in _segCells.Where(_lcr => _lcr.IsPoint))
                    {
                        lineSet.Add(in _c);
                    }
                }
            }
        }
        #endregion

        #region private double IncrementToNext(double d1, bool climbing)
        /// <summary>
        /// Defined to help arbitrary point to point line crossings
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="climbing"></param>
        /// <returns></returns>
        private double IncrementToNext(double d1, bool climbing)
        {
            var _mod = (d1 % 5d);
            if (climbing)
            {
                if (_mod == 0d)
                {
                    return 5d;
                }
                else if (d1 < 0d)
                {
                    return 0d - _mod;
                }
                else
                {
                    return (5d - _mod) % 5d;
                }
            }
            else
            {
                if (_mod == 0d)
                {
                    return -5d;
                }
                else if (d1 < 0d)
                {
                    return (-5d - _mod) % 5d;
                }
                else
                {
                    return 0d - _mod;
                }
            }
        }
        #endregion

        #region private IEnumerable<double, Point3D> GetLineIntersects(Point3D pt1, Point3D pt2)
        private IEnumerable<DistantPoint3D> GetLineIntersects(Point3D pt1, Point3D pt2)
        {
            Vector3D _v1 = pt2 - pt1;

            // always the intial
            yield return new DistantPoint3D(pt1, pt1);

            if (pt1 != pt2)
            {
                // z generator
                var _zClimb = pt1.Z < pt2.Z;
                var _zPt = new Point3D(pt1.X, pt1.Y, pt1.Z);
                var _rYZ = (_v1.Z != 0) ? _v1.Y / _v1.Z : 0;
                var _rXZ = (_v1.Z != 0) ? _v1.X / _v1.Z : 0;
                Func<DistantPoint3D> _zGen =
                    _v1.Z != 0 ?
                    (Func<DistantPoint3D>)(() =>
                    {
                        var _dz = IncrementToNext(_zPt.Z, _zClimb);
                        var _py = _dz * _rYZ + _zPt.Y;
                        var _px = _dz * _rXZ + _zPt.X;
                        _zPt = new Point3D(_px, _py, _dz + _zPt.Z);

                        // return value if not done
                        if (_zClimb ? _zPt.Z < pt2.Z : _zPt.Z > pt2.Z)
                        {
                            return new DistantPoint3D(_zPt, pt1);
                        }

                        // otherwise nul
                        return new DistantPoint3D(false);
                    })
                    : (Func<DistantPoint3D>)(() => new DistantPoint3D(false));

                // y generator
                var _yClimb = pt1.Y < pt2.Y;
                var _yPt = new Point3D(pt1.X, pt1.Y, pt1.Z);
                var _rZY = (_v1.Y != 0) ? _v1.Z / _v1.Y : 0;
                var _rXY = (_v1.Y != 0) ? _v1.X / _v1.Y : 0;
                Func<DistantPoint3D> _yGen =
                    _v1.Y != 0 ?
                    (Func<DistantPoint3D>)(() =>
                    {
                        var _dy = IncrementToNext(_yPt.Y, _yClimb);
                        var _pz = _dy * _rZY + _yPt.Z;
                        var _px = _dy * _rXY + _yPt.X;
                        _yPt = new Point3D(_px, _dy + _yPt.Y, _pz);

                        // return value if not done
                        if (_yClimb ? _yPt.Y < pt2.Y : _yPt.Y > pt2.Y)
                        {
                            return new DistantPoint3D(_yPt, pt1);
                        }

                        // otherwise null
                        return new DistantPoint3D(false);
                    })
                    : (Func<DistantPoint3D>)(() => new DistantPoint3D(false));

                // x generator
                var _xClimb = pt1.X < pt2.X;
                var _xPt = new Point3D(pt1.X, pt1.Y, pt1.Z);
                var _rZX = (_v1.X != 0) ? _v1.Z / _v1.X : 0;
                var _rYX = (_v1.X != 0) ? _v1.Y / _v1.X : 0;
                Func<DistantPoint3D> _xGen =
                    _v1.X != 0 ?
                    (Func<DistantPoint3D>)(() =>
                    {
                        var _dx = IncrementToNext(_xPt.X, _xClimb);
                        var _pz = _dx * _rZX + _xPt.Z;
                        var _py = _dx * _rYX + _xPt.Y;
                        _xPt = new Point3D(_dx + _xPt.X, _py, _pz);

                        // return value if not done
                        if (_xClimb ? _xPt.X < pt2.X : _xPt.X > pt2.X)
                        {
                            return new DistantPoint3D(_xPt, pt1);
                        }

                        // otherwise null
                        return new DistantPoint3D(false);
                    })
                    : (Func<DistantPoint3D>)(() => new DistantPoint3D(false));

                // load up generators ...
                DistantPoint3D _gen(int refer)
                    => refer switch
                    {
                        0 => _zGen(),
                        1 => _yGen(),
                        _ => _xGen(),
                    };

                // ... and seed values
                var _dPts = new DistantPoint3D[] { _zGen(), _yGen(), _xGen() };

                // keep going while there is something
                while (_dPts[0].IsValid || _dPts[1].IsValid || _dPts[2].IsValid)
                {
                    // get lowest value
                    var _m1 = (_dPts[0].Distance < _dPts[1].Distance ? _dPts[0] : _dPts[1]);
                    var _mn = (_m1.Distance < _dPts[2].Distance ? _m1 : _dPts[2]);

                    // figure out which ones match
                    var _added = false;
                    for (var _ref = 0; _ref <= 2; _ref++)
                    {
                        if (_dPts[_ref].Distance == _mn.Distance)
                        {
                            if (!_added)
                            {
                                // add the point once
                                _added = true;
                                yield return _dPts[_ref];
                            }

                            // cycle any generator that matches
                            _dPts[_ref] = _gen(_ref);
                        }
                    }
                }
            }

            // always the final
            yield return new DistantPoint3D(pt2, pt1);
            yield break;
        }
        #endregion

        [ThreadStatic]
        private static StringBuilder _DebugSegments;

        internal static void DoDebugAddLine(string adder)
            => _DebugSegments?.AppendLine(adder);

        internal static void DoDebugAdd(string adder)
            => _DebugSegments?.Append(adder);

        #region public SegmentSet SegmentCells(Point3D pt1, Point3D pt2, SegmentSetFactory factory)
        /// <summary>Returns LineSet between arbitrary points.</summary>
        public SegmentSet SegmentCells(Point3D pt1, Point3D pt2, SegmentSetFactory factory, PlanarPresence planar)
        {
            var _pt1 = pt1;
            var _pt2 = pt2;

            // NOTE: LengthSquared is more economical than Length for Vectors, and comparatively congruent
            var _points = GetLineIntersects(_pt1, _pt2).GetEnumerator();

            // build a set of cells ordered by length from _pt1, also catch the entry/exit points
            // TODO: the entry/exit point detection process may not work quite right for arbitrary points
            var _cells = factory.CreateSegmentSet(pt1, pt2, planar);

            // initial
            var _aPt = _points.MoveNext() ? _points.Current : new DistantPoint3D(false);
            AddSegment(_cells, _aPt, _aPt);

            var _px = 0;
            var _bPt = _points.MoveNext() ? _points.Current : new DistantPoint3D(false);
            while (_bPt.IsValid)
            {
                AddSegment(_cells, _aPt, _bPt);

                // added short-circuit when line cannot add any more
                if (!factory.CanAddMore(_cells))
                {
                    return _cells;
                }
                _aPt = _bPt;
                _bPt = _points.MoveNext() ? _points.Current : new DistantPoint3D(false);

                // count at least one extra point
                _px++;
            }

            // terminal (if counted at least one extra point)
            if (_px > 0)
            {
                AddSegment(_cells, _aPt, _aPt);
            }

            return _cells;
        }
        #endregion

        /// <summary>Examines lines from source to all corners of target cell until a non-blocking line is found.</summary>
        /// <param name="source">point from which to determine line of effect</param>
        /// <param name="target">single cell location</param>
        /// <returns>true if a line to at least one corner is not blocked</returns>
        public bool HasLineOfEffect(Point3D source, IGeometricRegion sourceRegion, ICellLocation target, PlanarPresence planar)
            => EffectLinesToTarget(source, sourceRegion, target, planar, ITacticalInquiryHelper.EmptyArray).Any();

        #region public bool CanOccupy(ICoreObject coreObj, IGeometricRegion test, MovementBase movement, ICore exclude = null)
        /// <summary>true if the cells in the region do not prevent the cubic from occupying the space</summary>
        public bool CanOccupy(ICoreObject coreObj, IGeometricRegion region, MovementBase movement, Dictionary<Guid, ICore> exclude,
            PlanarPresence planar)
        {
            AxisSnap _makeFlag(int part)
                => part < 0 ? AxisSnap.Low : (part > 0 ? AxisSnap.High : AxisSnap.None);

            var _exclusions = (exclude ?? [])
                .Where(_x => _x.Value is IMoveAlterer)
                .ToDictionary(_ima => _ima.Key, _ima => _ima.Value);

            // loop through all cells
            // TODO: parallel
            var _affecters = new List<IMoveAlterer>();
            foreach (var _cellLoc in region.AllCellLocations())
            {
                // cell to test
                var _cell = this[_cellLoc];

                // TODO: less loopy, more linear, handle single cell (account for non-squeezables)

                // loop through interior components
                for (var _zp = (!region.IsCellAtSurface(_cellLoc, AnchorFace.ZLow) ? -1 : 0);
                    _zp <= (!region.IsCellAtSurface(_cellLoc, AnchorFace.ZHigh) ? 1 : 0);
                    _zp++)
                {
                    var _zflag = _makeFlag(_zp);
                    for (var _yp = (!region.IsCellAtSurface(_cellLoc, AnchorFace.YLow) ? -1 : 0);
                        _yp <= (!region.IsCellAtSurface(_cellLoc, AnchorFace.YHigh) ? 1 : 0);
                        _yp++)
                    {
                        var _yflag = _makeFlag(_yp);
                        for (var _xp = (!region.IsCellAtSurface(_cellLoc, AnchorFace.XLow) ? -1 : 0);
                            _xp <= (!region.IsCellAtSurface(_cellLoc, AnchorFace.XHigh) ? 1 : 0);
                            _xp++)
                        {
                            var _xflag = _makeFlag(_xp);

                            // any blocked, the cubic cannot occupy
                            if (_cell.BlockedAt(movement, new CellSnap(_zflag, _yflag, _xflag)))
                            {
                                // TODO:  account for clearance...???
                                return false;
                            }
                        }
                    }
                }

                // find affecters in the cell
                _affecters.AddRange(MapContext
                    .AllInCell<IMoveAlterer>(_cellLoc, planar)
                    .Where(_aff => !_exclusions.ContainsKey(_aff.ID) && !_affecters.Contains(_aff)));
            }

            // if had any affecters, make sure they allow occupation
            if (_affecters.Any())
            {
                return _affecters.All(_ma => _ma.CanOccupy(movement, region, coreObj));
            }

            return true;
        }
        #endregion

        #region public List<SegmentSet> GetLinesSets(IList<Point3D> sourcePoints, IList<Point3D> targetPoints, ICoreObject targetObject, params ICore[] exclude)
        /// <summary>Gets all linesets between two sets of points</summary>
        public List<SegmentSet> GetLinesSets(IList<Point3D> sourcePoints, IList<Point3D> targetPoints,
            IGeometricRegion sourceRegion, IGeometricRegion targetRegion, PlanarPresence planar, params ICore[] exclude)
        {
            // prepare to test lines
            var _factory = new SegmentSetFactory(this, sourceRegion, targetRegion,
                ITacticalInquiryHelper.GetITacticals(exclude).ToArray(), SegmentSetProcess.Effect);

            // build lines in parallel and snapshot to list (will need them all anyway)
            return (from _srcPt in sourcePoints
                    from _trgPt in targetPoints
                    select new { Source = _srcPt, Target = _trgPt })
                    .AsParallel()
                    .Select(_pts => SegmentCells(_pts.Source, _pts.Target, _factory, planar))
                    .ToList();
        }
        #endregion

        #region public int CoverValue(IList<Point3D> sourcePoints, IList<Point3D> targetPoints, ICoreObject targetObject, bool allowSoft, Interaction workSet, params ICore[] exclude)
        /// <summary>Returns Cover Bonus (0, 2, 4, 8 or Int32.MaxValue)</summary>
        public int CoverValue(IList<Point3D> sourcePoints, IList<Point3D> targetPoints,
            IGeometricRegion sourceRegion, IGeometricRegion targetRegion, bool allowSoft,
            Interaction workSet, PlanarPresence planar, params ICore[] exclude)
        {
            // all lines
            var _lines = GetLinesSets(sourcePoints, targetPoints, sourceRegion, targetRegion, planar, exclude);
            var _max = _lines.Count;

            // good lines are effect lines that can carry the interaction
            var _goodLines = _lines.AsParallel()
                .Where(_l => _l.IsLineOfEffect && _l.CarryInteraction(workSet))
                .ToList();
            return CoverValue(_goodLines, _max, allowSoft, workSet);
        }
        #endregion

        #region public int CoverValue(List<LineSet> goodLines, int maxLines, bool allowSoft, Interaction workSet)
        /// <summary>Returns Cover Bonus (0, 2, 4, 8 or Int32.MaxValue)</summary>
        public int CoverValue(List<SegmentSet> goodLines, int maxLines, bool allowSoft, Interaction workSet)
        {
            if (goodLines.Count == 0)
            {
                return Int32.MaxValue;
            }

            // get a count of those lines with no effect, or which cannot pass the interaction
            var _badCount = maxLines - goodLines.Count;

            // percent
            var _percent = (1d * _badCount) / (1d * maxLines);
            if (_percent > 0.75d)
            {
                return 8;
            }

            // now, for the lines that do have effect, start maxxing cover percent
            var _max = goodLines.AsParallel().Max(
                (_ln) => (_ln.SuppliesCover()) switch
                {
                    CoverLevel.Soft => allowSoft ? 0.5d : 0d,
                    CoverLevel.Hard => 0.5d,
                    CoverLevel.Improved => 0.9d,
                    _ => 0d,
                }
            );

            // new max (lines and lines that supply cover)
            _max = Math.Max(_max, _percent);
            if (_max > 0.75d)
            {
                // improved
                return 8;
            }
            if (_max > 0.25d)
            {
                // regular
                return 4;
            }
            if (_max > 0)
            {
                // partial (leftover from line count)
                return 2;
            }

            // no cover
            return 0;
        }
        #endregion

        #region public IEnumerable<(ICoreObject CoreObj, CellPosition Location)> MeleeBlockers(ICellLocation source, ICellLocation target, ICoreObject targetObject, params ICore[] exclude)
        /// <summary>Gets Melee Blockers (object and cell) between source and target</summary>
        public IEnumerable<(ICoreObject CoreObj, CellPosition Location)> MeleeBlockers(ICellLocation source,
            ICellLocation target, PlanarPresence planar, params ICore[] exclude)
        {
            var _factory = new SegmentSetFactory(this, source.ToCellPosition(), target.ToCellPosition(),
                ITacticalInquiryHelper.GetITacticals(exclude).ToArray(), SegmentSetProcess.Effect);
            return (from _sISect in source.AllCorners()
                    from _tISect in target.AllCorners()
                    let _lSet = SegmentCells(_sISect, _tISect, _factory, planar)
                    where (_lSet?.BlockedObject is ICoreObject)
                    select (_lSet.BlockedObject as ICoreObject, new CellPosition(_lSet.BlockedCell))).Distinct();
        }
        #endregion

        #region public IEnumerable<LineSet> EffectLinesToTarget(Intersection source, ICellLocation target)
        /// <summary>
        /// Provides all non-blocking lines from intersection to all corners of target cell.
        /// </summary>
        /// <param name="source">point from which to determine line of effect</param>
        /// <param name="target">single cell location</param>
        /// <returns>lines of effect, if any can be found</returns>
        public IEnumerable<SegmentSet> EffectLinesToTarget(Point3D source, IGeometricRegion sourceRegion, ICellLocation target,
            PlanarPresence planar, ITacticalInquiry[] exclusions)
        {
            var _factory = new SegmentSetFactory(this, sourceRegion, target.ToCellPosition(),
                exclusions, SegmentSetProcess.Effect);
            foreach (var _corner in target.AllCorners())
            {
                var _lSet = SegmentCells(source, _corner, _factory, planar);
                if (_lSet.IsLineOfEffect)
                {
                    yield return _lSet;
                }
            }
            yield break;
        }
        #endregion

        public static Model3D CellGlow(ICellLocation cLoc)
            => DrawingTools.CellGlow(cLoc.Z, cLoc.Y, cLoc.X);

        public static Model3D CellGlow(ICellLocation cLoc, System.Windows.Media.Media3D.Material brushMaterial)
            => DrawingTools.CellGlow(cLoc.Z, cLoc.Y, cLoc.X, brushMaterial);

        // TODO: animate instantaneous effects
        // TODO: affect on specific interactions...

        #region public IEnumerable<AwarenessResult> GetAwarenessResults(Creature creature)
        /// <summary>Enumerates all objects of which the creature should currently have awareness.</summary>
        public IEnumerable<AwarenessResult> GetAwarenessResults(Creature creature, ISensorHost sensors)
        {
            // find creature
            var _sensorLocator = sensors.GetLocated().Locator;

            // find stuff in the localMap
            var _sweep = sensors?.RoomAwarenesses?.GetSweptRooms() ?? new Dictionary<Guid, Guid>();
            var _render = sensors?.RoomAwarenesses?.GetRenderedRooms() ?? new Dictionary<Guid, Guid>();
            var _cx = 0;
            foreach (var _targetLocator in (from _rid in _sweep
                                            join _room in Rooms
                                            on _rid.Key equals _room.ID
                                            from _loc in _room.Locators
                                            where _render.ContainsKey(_room.ID) || _loc.Links.Any(_out => _render.ContainsKey(_out.ID))
                                            select _loc)
                                            .Union(MapContext.LocatorsInBackground()))
            {
                // distance from sensor to target
                var _distance = _targetLocator.GeometricRegion.NearDistance(_sensorLocator.GeometricRegion);
                foreach (var _obj in _targetLocator.ICoreAs<CoreObject>())
                {
                    _cx++;
                    // observation from creature to target object
                    var _obs = new Observe(creature, sensors, _targetLocator, _sensorLocator, _distance);
                    var _obsInteract = new Interaction(null, sensors, _obj, _obs);
                    _obj.HandleInteraction(_obsInteract);
                    foreach (var _oFeed in _obsInteract.Feedback.OfType<ObserveFeedback>())
                    {
                        // default (or only) level
                        if ((_oFeed is ObserveSpotFeedback _osFeed)
                            && creature.Skills.Skill<SpotSkill>().AutoCheck(_osFeed.Difficulty, _obj))
                        {
                            // iterate it back
                            foreach (var _kvp in _osFeed.SpotSuccesses)
                            {
                                yield return new AwarenessResult
                                {
                                    ID = _kvp.Key,
                                    AwarenessLevel = _kvp.Value,
                                    Locator = _targetLocator
                                };
                                foreach (var _cnKvp in ObserveFeedback.YieldConnectedResults(_kvp.Value, _obj, _osFeed,
                                    creature, sensors, _targetLocator, _sensorLocator, _distance))
                                {
                                    yield return new AwarenessResult
                                    {
                                        ID = _cnKvp.Key,
                                        AwarenessLevel = _cnKvp.Value,
                                        Locator = _targetLocator
                                    };
                                }
                            }
                        }
                        else
                        {
                            // iterate it back
                            foreach (var _kvp in _oFeed.Levels)
                            {
                                yield return new AwarenessResult
                                {
                                    ID = _kvp.Key,
                                    AwarenessLevel = _kvp.Value,
                                    Locator = _targetLocator
                                };
                                foreach (var _cnKvp in ObserveFeedback.YieldConnectedResults(_kvp.Value, _obj, _oFeed,
                                    creature, sensors, _targetLocator, _sensorLocator, _distance))
                                {
                                    yield return new AwarenessResult
                                    {
                                        ID = _cnKvp.Key,
                                        AwarenessLevel = _cnKvp.Value,
                                        Locator = _targetLocator
                                    };
                                }
                            }
                        }
                    }
                }
            }
            yield break;
        }

        #endregion

        #region ICorePart Members

        public IEnumerable<ICorePart> Relationships
        {
            get
            {
                yield return Resources;
                yield return new PartsFolder(this, @"Cell Templating", typeof(PartsFolder),
                    new PartsFolder(this, @"Cell Materials", CellMaterials.Select(_cm => _cm.Value as ICorePart).OrderBy(_ip => _ip.Name), typeof(CellMaterial)),
                    new PartsFolder(this, @"Cell Spaces", CellSpaces.Select(_s => _s as ICorePart).OrderBy(_ip => _ip.Name), typeof(CellSpace)),
                    new PartsFolder(this, @"Cell Panels", Panels.All().Select(_p => _p as ICorePart).OrderBy(_ip => _ip.Name), typeof(BasePanel)));
                yield return new PartsFolder(this, @"Cell Groups", typeof(PartsFolder),
                    new PartsFolder(this, @"Ambient Lights", AmbientLights.Select(_kvp => _kvp.Value as ICorePart).OrderBy(_ip => _ip.Name), typeof(AmbientLight)),
                    Backgrounds, Rooms);
                //yield return MapContext;
                //yield return new PartsFolder(this, @"Room Locators", Rooms.SelectMany(_r => _r.Locators), typeof(Locator));
                yield return new PartsFolder(this, @"Processes", ContextSet.ProcessManager.AllProcesses.Select(_p => new CoreProcessPart(_p)), typeof(CoreProcessPart));
                yield break;
            }
        }
        public string TypeName => GetType().FullName;

        #endregion

        #region IBasePart Members

        #region public string BindableName { get; set; }
        public string BindableName
        {
            get => Name;
            set
            {
                if (value == null)
                {
                    return;
                }

                if (!(_NameManager?.CanUseName(value, typeof(LocalMap)) ?? false))
                {
                    return;
                }
                _Name = value.ToSafeString();
                DoPropertyChanged(nameof(Name));
                DoPropertyChanged(nameof(BindableName));
            }
        }
        #endregion

        #region public ICorePartNameManager NameManager { get; set; }
        public ICorePartNameManager NameManager
        {
            get => _NameManager;
            set => _NameManager ??= value;
        }
        #endregion

        public PackagePart Part
            => _MapPart;

        public void Save(Package parent)
        {
            // create new part
            var _folder = UriHelper.ConcatRelative(new Uri(@"/", UriKind.Relative), $@"{Name}.map");
            var _content = UriHelper.ConcatRelative(_folder, @"localmap.ikosa");
            _MapPart = parent.CreatePart(_content, @"ikosa/localmap", CompressionOption.Normal);
            parent.CreateRelationship(_content, TargetMode.Internal, IkosaMapRelation);

            DoSave(_folder);

            //JsonSave();
        }

        public void Save(PackagePart parent, Uri baseUri)
        {
            // create new part
            var _folder = UriHelper.ConcatRelative(baseUri, $@"{Name}.map");
            var _content = UriHelper.ConcatRelative(_folder, @"localmap.ikosa");
            _MapPart = parent.Package.CreatePart(_content, @"ikosa/localmap", CompressionOption.Normal);
            parent.CreateRelationship(_content, TargetMode.Internal, IkosaMapRelation);

            DoSave(_folder);

            //JsonSave();
        }

        private void JsonSave()
        {
            var _serializer = new JsonSerializer
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };
            using var _file = File.Create(@"d:\ikosa\map.json");
            using var _writer = new StreamWriter(_file);
            _serializer.Serialize(_writer, this);
        }

        #region private void DoSave(Uri _base)
        private void DoSave(Uri _base)
        {
            // map
            using (var _mapStream = _MapPart.GetStream(FileMode.Create, FileAccess.ReadWrite))
            {
                IFormatter _fmt = new BinaryFormatter();
                _fmt.Serialize(_mapStream, this);
            }

            // save resources
            _Resources.Save(_MapPart, _base);
        }
        #endregion

        #region public void RefreshPart(PackagePart part)
        public void RefreshPart(PackagePart part)
        {
            _MapPart = part;

            // if this wasn't cleared, update and add
            if (_MapPart != null)
            {
                var _rscPart = _MapPart.GetRelationships().RelatedPackageParts()
                    .FirstOrDefault(_p => _p.RelationshipType == VisualResources.VisualResourcesRelation);
                if (_rscPart != null)
                {
                    _Resources.RefreshPart(_rscPart.Part);
                }
            }
        }
        #endregion

        #endregion

        #region ICorePartNameManager Members

        public bool CanUseName(string name, Type partType)
        {
            var _name = name.ToSafeString();
            if (partType.Equals(typeof(CellSpace)))
            {
                return !_Spaces.Any(_s => _s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
            else if (partType.Equals(typeof(BasePanel)))
            {
                return !Panels.ContainsKey(name);
            }
            return false;
        }

        public void Rename(string oldName, string newName, Type partType)
        {
            // NOTE: nothing
        }

        #endregion

        #region INotifyPropertyChanged Members

        protected void DoPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region public void SignalMapChanged(object sender)
        [field: NonSerialized, JsonIgnore]
        public event EventHandler MapChanged;

        /// <summary>
        /// This should only be called when the entire map reference is replaced.  
        /// It indicates to clients to flush and recreate the entire data cache for local mapping.
        /// </summary>
        public void SignalMapChanged(object sender)
        {
            MapChanged?.Invoke(sender, new EventArgs());
        }
        #endregion

        #region public Cubic BackgroundViewport { get; set; }
        public Cubic BackgroundViewport
        {
            get { return _Viewport; }
            set
            {
                _Viewport = value;
                if (_Initialized)
                {
                    ShadingZones.RecacheShadingZones();
                    ShadingZones.ReshadeBackground();
                }
            }
        }
        #endregion

        /// <summary>Reports whether the specified cell is in magical darkness</summary>
        public bool IsInMagicDarkness(ICellLocation location)
            => RoomIndex.GetRoom(location)?.IsInMagicDarkness(location)
            ?? ShadingZones.IsMagicDark(location);

        /// <summary>Reports the light level for the specified cell</summary>
        public LightRange GetLightLevel(ICellLocation location)
            => RoomIndex.GetRoom(location)?.GetLightLevel(location)
            ?? ShadingZones.GetLightLevel(location, LightRange.OutOfRange);

        public VisualEffect GetVisualEffect(IGeometricRegion source, ICellLocation target, TerrainVisualizer visualizer)
            => RoomIndex.GetRoom(target)?.CellEffect(source, target.Z, target.Y, target.X, visualizer)
            ?? ShadingZones.CellEffect(source, target.Z, target.Y, target.X, visualizer);

        /// <summary>Gravity in effect (6-directional) at cell location</summary>
        public AnchorFace GetGravityFace(ICellLocation location)
        {
            // TODO: get this from setting, and zonal effects...
            return AnchorFace.ZLow;
        }

        #region public Vector3D GetGravityDropVector3D(ICellLocation location)
        /// <summary>Gravity drop vector at cell location (fixed directional and linear)</summary>
        public Vector3D GetGravityDropVector3D(ICellLocation location)
            => (GetGravityFace(location)) switch
            {
                AnchorFace.ZLow => new Vector3D { Z = -200 },
                AnchorFace.ZHigh => new Vector3D { Z = 200 },
                AnchorFace.YLow => new Vector3D { Y = -200 },
                AnchorFace.YHigh => new Vector3D { Y = 200 },
                AnchorFace.XLow => new Vector3D { X = -200 },
                _ => new Vector3D { X = 200 },
            };
        #endregion

        #region public IEnumerable<SlopeSegment> CellSlopes(ICoreObject coreObj, CellLocation cell, MovementBase movement, AnchorFace baseFace, List<ICore> exclusions)
        /// <summary>Slopes (suitable for relative elevation checks with same baseFace) of a cell using the movement</summary>
        public IEnumerable<SlopeSegment> CellSlopes(ICoreObject coreObj, CellLocation cell, MovementBase movement, AnchorFace baseFace,
            Dictionary<Guid, ICore> exclusions, PlanarPresence planar)
        {
            Func<IMoveAlterer, bool> _ignore =
                (exclusions?.Count ?? 0) > 0
                ? ((ma) => exclusions.ContainsKey(ma.ID))
                : (Func<IMoveAlterer, bool>)((ma) => false);

            var _revBase = baseFace.ReverseFace();
            var _gOffset = baseFace.GetAnchorOffset();

            // intra-cell opening
            var _mti = new MovementTacticalInfo
            {
                Movement = movement,
                SourceCell = cell,
                TargetCell = cell,
                TransitFaces = new AnchorFace[] { _revBase }
            };

            // get openings both for cell structure and any move affecters
            var _elev = cell.CellElevation(baseFace);
            var _cs = this[cell];
            var _openings = _cs.InnerSlopes(movement, _revBase, _elev)
                    .Union(from _ma in MapContext.AllInCell<IMoveAlterer>(_mti.SourceCell, planar)
                           where !_ignore(_ma)
                           from _mo in _ma.OpensTowards(movement, cell, baseFace, coreObj)
                           where _mo.Face == _revBase
                           select new SlopeSegment
                           {
                               Low = (5 - _mo.Amount) + _elev,
                               High = (5 - _mo.Amount) + _elev,
                               Run = 5 * _mo.Blockage,
                               Source = _ma
                           }).ToList();

            // completely underneath check
            if (_openings.Count == 0)
            {
                // look to see if "undercell" can support...
                var _cMask = _cs.HedralGripping(movement, baseFace).Invert();
                var _holdCell = cell.Move(_gOffset) as CellLocation;
                var _hc = this[_holdCell];
                var _holdBlock = _hc.HedralGripping(movement, _revBase).Intersect(_cMask).GripCount();
                if (_holdBlock < 32)
                {
                    _mti = new MovementTacticalInfo
                    {
                        Movement = movement,
                        SourceCell = _holdCell,
                        TransitFaces = new AnchorFace[] { _revBase }
                    };

                    foreach (var _ima in from _a in MapContext.AllInCell<IMoveAlterer>(_holdCell, planar)
                                         where !_ignore(_a)
                                         let _htb = _a.HedralTransitBlocking(_mti)
                                         where _htb >= 0.2
                                         select new { Block = _htb, Source = _a })
                    {
                        _openings.Add(new SlopeSegment
                        {
                            Low = _elev,
                            High = _elev,
                            Run = _ima.Block * 5,
                            Source = _ima.Source
                        });
                    }
                }
                else
                {
                    // cell holding this up has support
                    _openings.Add(new SlopeSegment
                    {
                        Low = _elev,
                        High = _elev,
                        Run = (_holdBlock / 64d) * 5
                    });
                }
            }
            else if ((_openings.Count == 1) && _openings.All(_o => _o.Run < 5))
            {
                var _under = 5 - _openings.Max(_o => _o.Run);

                // look to see if "undercell" can support...
                var _cMask = _cs.HedralGripping(movement, baseFace).Invert();
                var _holdCell = cell.Move(_gOffset) as CellLocation;
                var _hc = this[_holdCell];
                var _holdBlock = _hc.HedralGripping(movement, _revBase).Intersect(_cMask).GripCount();
                if (_holdBlock > 56)
                {
                    // cell holding this up has support
                    _openings.Add(new SlopeSegment
                    {
                        Low = _elev,
                        High = _elev,
                        Run = _under
                    });
                }
            }

            // everything gathered
            return _openings;
        }
        #endregion

        #region protected int? CellGrip(IGeometricRegion region, MovementBase movement, ICellLocation cell, AnchorFace baseFace)
        /// <summary>CellGrip for a cell within a region</summary>
        protected CellGripResult CellGrip(IGeometricRegion region, MovementBase movement, ICellLocation cell, AnchorFace baseFace)
        {
            // TODO: make this some kind of average?
            var _gravity = GetGravityFace(cell);

            // inner grip
            var _struc = this[cell];
            var _grip = _struc.InnerGripResult(_gravity, movement);

            // if inner grip matches the existing baseFace, don't try and change
            // look at surfaces of cell that are on surface of region
            foreach (var _rslt in (from _f in AnchorFaceHelper.GetAllFaces()
                                   where region.IsCellAtSurface(cell, _f) // only outward grippers
                                   let _rf = _f.ReverseFace()
                                   let _oCell = this[cell.Add(_f)]
                                   where _oCell.HedralGripping(movement, _rf)
                                        .Intersect(_struc.HedralGripping(movement, _f).Invert()).GripCount() > 3
                                   let _outer = _oCell.OuterGripDifficulty(_rf, _gravity, movement, _struc)
                                   where _outer != null
                                   select new CellGripResult
                                   {
                                       Difficulty = _outer,
                                       Faces = _f.ToAnchorFaceList()
                                   }))
            {
                // minimum difficulty and all faces
                if (_rslt.Difficulty <= (_grip.Difficulty ?? 1000))
                {
                    _grip.Difficulty = _rslt.Difficulty;
                    _grip.Faces = _grip.Faces.Union(_rslt.Faces);
                }
            }

            // outer proximal grip, ledges at edges...
            // any of the nearby edges as a last ditch dangling grip
            if (_grip.Difficulty == null)
            {
                foreach (var _rslt in (from _eco in AnchorFaceListHelper.EdgeCellOffsets().AsParallel()
                                       let _cell = cell.Add(_eco)
                                       where !region.ContainsCell(_cell) // only cells outside the region
                                       let _eStruc = this[_cell]
                                       let _eGrip = _eStruc.OuterCornerGripDifficulty(_eco.Invert(), _gravity, movement, _struc)
                                       select new CellGripResult
                                       {
                                           Difficulty = _eGrip,
                                           Faces = _eco
                                       }))
                {
                    // minimum difficuty and all faces
                    if (_rslt.Difficulty <= (_grip.Difficulty ?? 1000))
                    {
                        _grip.Difficulty = _rslt.Difficulty;
                        _grip.Faces = _grip.Faces.Union(_rslt.Faces);
                    }
                    // TODO: since we had to use an edge, increase difficulty...???
                    // TODO: unless the edge is a "ledge" (support against gravity), in which case it decreases difficulty
                }
            }

            return _grip;
        }
        #endregion

        #region public int? GripDifficulty(IGeometricRegion region, MovementBase movement, AnchorFace baseFace)
        /// <summary>Grip difficulty for a locator</summary>
        public CellGripResult GripDifficulty(IGeometricRegion region, MovementBase movement, AnchorFace baseFace)
        {
            var _grip = new CellGripResult
            {
                Difficulty = null,
                Faces = AnchorFaceList.None,
                InnerFaces = AnchorFaceList.None
            };
            foreach (var _rslt in from _cLoc in region.AllCellLocations()
                                  let _cg = CellGrip(region, movement, _cLoc, baseFace)
                                  where _cg.Difficulty != null
                                  select _cg)
            {
                // minimum difficuty and all faces
                if (_rslt.Difficulty <= (_grip.Difficulty ?? 1000))
                {
                    _grip.Difficulty = _rslt.Difficulty;
                    _grip.Faces = _grip.Faces.Union(_rslt.Faces);

                    // collect isinner faces (should pretty much just be one
                    _grip.InnerFaces = _grip.InnerFaces.Union(_rslt.InnerFaces);
                }
            }

            // summation
            return _grip;
        }
        #endregion

        #region protected int? SmallClimbOutDifficulty(ICoreObject coreObj, IGeometricRegion startRegion, MovementBase movement, AnchorFaceList moveTowards, AnchorFaceList lastCrossings)
        /// <summary>Difficulty for a creature small or smaller to climb</summary>
        protected int? SmallClimbOutDifficulty(ICoreObject coreObj, IGeometricRegion startRegion, MovementBase movement,
            AnchorFaceList moveTowards, AnchorFaceList lastCrossings, List<AnchorFace> grippers,
            AnchorFace gravity, AnchorFace baseFace, PlanarPresence planar)
        {
            // creature moving
            var _critter = movement.CoreObject as Creature;
            var _reach = _critter.Body.Height;
            var _snap = _critter.Adjuncts.OfType<SmallClimbing>().FirstOrDefault();

            // must be able to grip somewhere in the start region to climb
            var _start = GripDifficulty(startRegion, movement, baseFace);
            if (_start.Difficulty != null)
            {
                var _lastInnerGrip = new CellSnap(lastCrossings.Invert());

                // get all cells and whether we had some kind of implicit inner grip from our last movement
                var _startCells = (from _cellLoc in startRegion.AllCellLocations().AsParallel()
                                   let _innerStruc = this[_cellLoc]
                                   select new
                                   {
                                       Structure = _innerStruc,
                                       Location = _cellLoc,
                                       StartGrip = _innerStruc.BlockedAt(movement, _lastInnerGrip)
                                   }).ToList();

                // each cell in start, get difficulty to *exit*
                var _exit = (from _cell in _startCells.AsParallel()
                             select _cell.Structure.InnerGripMoveOutDelta(_reach, moveTowards, GetGravityFace(_cell.Location), movement))
                             .Union(from _gripFace in grippers.AsParallel()         // grip capable faces
                                    let _revGrip = _gripFace.ReverseFace()          // outer cell grip mirror
                                                                                    // TODO: must be possible to have made this grip...
                                    let _outerExitStart = lastCrossings.Remove(_gripFace).Add(_revGrip)
                                    let _outerExitEnd = moveTowards.Add(_revGrip)   // faces outside cell(s)
                                                                                    // cells at boundary
                                    from _cell in _startCells.AsParallel()
                                    where startRegion.IsCellAtSurface(_cell.Location, _gripFace)
                                    // facing inwards
                                    let _outerCell = _cell.Location.Add(_gripFace)
                                    let _outerStruc = this[_outerCell]
                                    // coverage% across face, and specifically at last movement spot
                                    let _blockage = DirectionalBlockage(coreObj, new CellPosition(_cell.Location), movement, _gripFace, gravity, null, planar)
                                    let _blocksStart = _outerStruc.BlockedAt(movement, new CellSnap(_outerExitStart))
                                    let _blocksEnd = _outerStruc.BlockedAt(movement, new CellSnap(_outerExitEnd))
                                    // TODO: delta based on how we entered this cell versus how we are trying to exit
                                    select (int?)null)
                             .Min();

                // total starting difficulty
                return _start.Difficulty + (_exit ?? 0);
            }

            // unable to calculate difficulty, therefore, cannot climb
            return (int?)null;
        }
        #endregion

        /// <summary>delta based on how we entered this cell versus how we are trying to exit</summary>
        private int? OuterMoveOut(double maxReach, bool originGripped, bool finalGripped, double faceBlockage)
        {
            return null;
        }

        #region public int? ClimbOutDifficulty(IGeometricRegion startRegion, MovementBase movement, AnchorFaceList moveTowards, AnchorFaceList lastCrossings)
        public int? ClimbOutDifficulty(ICoreObject coreObj, IGeometricRegion startRegion, MovementBase movement,
            AnchorFaceList moveTowards, AnchorFaceList lastCrossings, List<AnchorFace> grippers,
            AnchorFace gravity, AnchorFace baseFace, PlanarPresence planar)
        {
            // creature moving
            var _critter = movement.CoreObject as Creature;

            // small creature climbing works with surface adhesion
            //if (_critter.Body.Sizer.Size.Order < Size.Medium.Order)
            //    return SmallClimbOutDifficulty(startRegion, movement, moveTowards, lastCrossings, grippers, gravity, baseFace);

            var _reach = _critter.Body.Height;

            // must be able to grip somewhere in the start region to climb
            var _start = GripDifficulty(startRegion, movement, baseFace);
            if (_start.Difficulty != null)
            {
                var _lastInnerGrip = new CellSnap(lastCrossings.Invert());

                // get all cells and whether we had some kind of implicit inner grip from our last movement
                var _startCells = (from _cellLoc in startRegion.AllCellLocations().AsParallel()
                                   let _innerStruc = this[_cellLoc]
                                   select new
                                   {
                                       Structure = _innerStruc,
                                       Location = _cellLoc,
                                       StartGrip = _innerStruc.BlockedAt(movement, _lastInnerGrip)
                                   }).ToList();

                // each cell in start, get difficulty to *exit*
                var _exit = (from _cell in _startCells.AsParallel()
                             select _cell.Structure.InnerGripMoveOutDelta(_reach, moveTowards, GetGravityFace(_cell.Location), movement))
                             .Union(from _gripFace in grippers.AsParallel()             // grip capable faces
                                    let _innerExitFaces = moveTowards.Add(_gripFace)    // faces inside cell(s)
                                    let _revGrip = _gripFace.ReverseFace()              // outer cell grip mirror
                                    let _outerExitFaces = moveTowards.Add(_revGrip)     // faces outside cell(s)
                                                                                        // cells at boundary
                                    from _cell in _startCells.AsParallel()
                                    where startRegion.IsCellAtSurface(_cell.Location, _gripFace)
                                    // facing inwards
                                    let _outerCell = _cell.Location.Add(_gripFace)
                                    let _outerStruc = this[_outerCell]
                                    // coverage% across face, and specifically at last movement spot
                                    let _block = DirectionalBlockage(coreObj, new CellPosition(_cell.Location), movement, _gripFace, gravity, null, planar)
                                    let _doesBlock = _outerStruc.BlockedAt(movement, new CellSnap(_outerExitFaces))
                                    select OuterMoveOut(_reach, _cell.StartGrip, _doesBlock, _block))
                             .Min();

                // starting difficulty
                return _start.Difficulty + (_exit ?? 0);
            }

            // unable to calculate difficulty, therefore, cannot climb
            return (int?)null;
        }
        #endregion

        #region protected CellGripResult SmallClimbInDifficulty(ICoreObject coreObj, IGeometricRegion startRegion, IGeometricRegion endRegion, MovementBase movement, AnchorFaceList moveTowards, AnchorFaceList lastCrossings)
        /// <summary>Difficulty for a creature small or smaller to climb</summary>
        protected CellGripResult SmallClimbInDifficulty(ICoreObject coreObj, IGeometricRegion startRegion, IGeometricRegion _endRegion, MovementBase movement,
            AnchorFaceList moveTowards, AnchorFaceList lastCrossings, List<AnchorFace> grippers,
            AnchorFace gravity, AnchorFace baseFace, PlanarPresence planar)
        {
            // creature moving
            var _critter = movement.CoreObject as Creature;
            var _reach = _critter.Body.Height;
            var _snap = _critter.Adjuncts.OfType<SmallClimbing>().FirstOrDefault();

            // must be able to grip somewhere in the end region to climb
            var _end = GripDifficulty(_endRegion, movement, baseFace);
            if (_end.Difficulty != null)
            {
                // "move-from" information
                var _moveFrom = moveTowards.Invert();

                // each cell in end, get difficulty to *enter*
                var _entranceCells = (from _cellLoc in _endRegion.AllCellLocations().AsParallel()
                                      let _innerStruc = this[_cellLoc]
                                      select new
                                      {
                                          Structure = _innerStruc,
                                          Location = _cellLoc,
                                          StartGrip = _innerStruc.BlockedAt(movement, new CellSnap(_moveFrom))
                                      }).ToList();

                var _enter = (from _cell in _entranceCells.AsParallel()
                              select _cell.Structure.InnerGripMoveInDelta(_reach, _moveFrom, GetGravityFace(_cell.Location), movement))
                              .Union(from _gripFace in grippers.AsParallel()           // grip capable faces
                                     let _innerEntryFaces = _moveFrom.Add(_gripFace)    // faces inside cell(s)
                                     let _revGrip = _gripFace.ReverseFace()             // outer cell grip mirror
                                     let _outerEntryFaces = _moveFrom.Add(_revGrip)     // faces outside cell(s)
                                                                                        // cells at boundary
                                     from _cell in _entranceCells.AsParallel()
                                     where startRegion.IsCellAtSurface(_cell.Location, _gripFace)
                                     // facing inwards
                                     let _outerCell = _cell.Location.Add(_gripFace)
                                     let _outerStruc = this[_outerCell]
                                     // coverage% across face, and specifically at first movement spot
                                     let _block = DirectionalBlockage(coreObj, new CellPosition(_cell.Location), movement, _gripFace, gravity, null, planar)
                                     let _doesBlock = _outerStruc.BlockedAt(movement, new CellSnap(_outerEntryFaces))
                                     select (int?)null)
                             .Min();
                _end.Difficulty += _enter ?? 0;

                // max difficulty
                return _end;
            }

            // unable to calculate difficulty, therefore, cannot climb
            return new CellGripResult
            {
                Difficulty = null,
                Faces = AnchorFaceList.None,
                InnerFaces = AnchorFaceList.None
            };
        }
        #endregion

        #region public CellGripResult ClimbInDifficulty(ICoreObject coreObj, IGeometricRegion startRegion, IGeometricRegion startCell, IGeometricRegion endRegion, MovementBase movement, AnchorFaceList moveTowards, AnchorFaceList lastCrossings)
        public CellGripResult ClimbInDifficulty(ICoreObject coreObj, IGeometricRegion startRegion, IGeometricRegion endRegion, MovementBase movement,
            AnchorFaceList moveTowards, AnchorFaceList lastCrossings, List<AnchorFace> grippers,
            AnchorFace gravity, AnchorFace baseFace, PlanarPresence planar)
        {
            // creature moving
            var _critter = movement.CoreObject as Creature;

            // small creature climbing works with surface adhesion
            if (_critter.Body.Sizer.Size.Order < Size.Medium.Order)
            {
                return SmallClimbInDifficulty(coreObj, startRegion, endRegion, movement, moveTowards, lastCrossings, grippers, gravity, baseFace, planar);
            }

            var _reach = _critter.Body.Height;

            // must be able to grip somewhere in the end region to climb
            var _end = GripDifficulty(endRegion, movement, baseFace);
            if (_end.Difficulty != null)
            {
                // entry region needs "move-from" information
                var _moveFrom = moveTowards.Invert();

                // each cell in end, get difficulty to *enter*
                var _entranceCells = (from _cellLoc in endRegion.AllCellLocations().AsParallel()
                                      let _innerStruc = this[_cellLoc]
                                      select new
                                      {
                                          Structure = _innerStruc,
                                          Location = _cellLoc,
                                          StartGrip = _innerStruc.BlockedAt(movement, new CellSnap(_moveFrom))
                                      }).ToList();

                var _enter = (from _cell in _entranceCells.AsParallel()
                              select _cell.Structure.InnerGripMoveInDelta(_reach, _moveFrom, GetGravityFace(_cell.Location), movement))
                              .Union(from _gripFace in grippers.AsParallel()            // grip capable faces
                                     let _innerEntryFaces = _moveFrom.Add(_gripFace)    // faces inside cell(s)
                                     let _revGrip = _gripFace.ReverseFace()             // outer cell grip mirror
                                     let _outerEntryFaces = _moveFrom.Add(_revGrip)     // faces outside cell(s)
                                                                                        // cells at boundary
                                     from _cell in _entranceCells.AsParallel()
                                     where startRegion.IsCellAtSurface(_cell.Location, _gripFace)
                                     // facing inwards
                                     let _outerCell = _cell.Location.Add(_gripFace)
                                     let _outerStruc = this[_outerCell]
                                     // coverage% across face, and specifically at first movement spot
                                     let _block = DirectionalBlockage(coreObj, new CellPosition(_cell.Location), movement, _gripFace, gravity, null, planar)
                                     let _doesBlock = _outerStruc.BlockedAt(movement, new CellSnap(_outerEntryFaces))
                                     select (int?)null)
                             .Min();
                _end.Difficulty += _enter ?? 0;

                // max difficulty
                return _end;
            }

            // unable to calculate difficulty, therefore, cannot climb
            return new CellGripResult
            {
                Difficulty = null,
                Faces = AnchorFaceList.None,
                InnerFaces = AnchorFaceList.None
            };
        }
        #endregion

        #region public double DirectionalBlockage(IGeometricRegion region, MovementBase movement, AnchorFace blockDirection)
        /// <summary>
        /// Indicates how much (fractional) of the region's directional boundary is blocked by material.
        /// Includes external cells structure and contents facing backwards towards the boundary cells.
        /// </summary>
        public double DirectionalBlockage(ICoreObject coreObj, IGeometricRegion region, MovementBase movement,
            AnchorFace blockDirection, AnchorFace gravity, Dictionary<Guid, ICore> exclusions, PlanarPresence planar)
        {
            Func<IMoveAlterer, bool> _ignore =
                (exclusions?.Count ?? 0) > 0
                ? ((ma) => exclusions.ContainsKey(ma.ID))
                : (Func<IMoveAlterer, bool>)((ma) => false);

            var _revBlockage = blockDirection.ReverseFace();
            var _bOffset = blockDirection.GetAnchorOffset();
            var _boundCells = region.AllCellLocations().Where(_cl => region.IsCellAtSurface(_cl, blockDirection));
            var _foot = 0d;
            var _coverage = 0d;
            foreach (var _cell in _boundCells.Select(_sc => new CellLocation(_sc)))
            {
                _foot += 1d;

                // intra-cell opening
                var _mti = new MovementTacticalInfo
                {
                    Movement = movement,
                    SourceCell = _cell
                };

                // cell structure unioned with move affecters
                ref readonly var _cs = ref this[_cell];
                var _openings = _cs.OpensTowards(movement, gravity).Where(_o => _o.Face == _revBlockage)
                    .Union(from _ma in MapContext.AllInCell<IMoveAlterer>(_mti.SourceCell, planar)
                           where !_ignore(_ma)
                           from _mo in _ma.OpensTowards(movement, _cell, gravity, coreObj)
                           where _mo.Face == _revBlockage
                           select _mo).ToList();
                var _max = 0d;
                if (_openings.Count > 0)
                {
                    _max = _openings.Max(_o => _o.Blockage);
                }

                if ((_openings.Count == 0) || (_max < 1d))
                {
                    // direct support from cell "underneath"?
                    var _holdCell = _cell.Move(_bOffset) as CellLocation;
                    ref readonly var _hs = ref this[_holdCell];
                    var _cMask = _cs.HedralGripping(movement, blockDirection).Invert();
                    var _holdBlock = _hs.HedralGripping(movement, _revBlockage).Intersect(_cMask).GripCount() / 64d;
                    if (_holdBlock > _max)
                    {
                        _max = _holdBlock;
                    }

                    if (_max < 1d)
                    {
                        // support from IMoveAffecters "underneath"
                        _mti = new MovementTacticalInfo
                        {
                            Movement = movement,
                            SourceCell = _holdCell,
                            TransitFaces = new AnchorFace[] { _revBlockage }
                        };
                        var _ima = MapContext.AllInCell<IMoveAlterer>(_holdCell, planar)
                            .Where(_ma => !_ignore(_ma))
                            .ToList();
                        if (_ima.Count > 0)
                        {
                            var _imaBlock = _ima.Max(_i => _i.HedralTransitBlocking(_mti));
                            if (_imaBlock > _max)
                            {
                                _max = _imaBlock;
                            }
                        }
                    }
                }

                // main cell has blockage
                _coverage += _max;
            }

            // total coverage values over footer cells
            return _coverage / _foot;
        }
        #endregion

        /// <summary>Get implied plummet deflection faces (if possible)</summary>
        public AnchorFace[] PlummetDeflection(ICellLocation cell, MovementBase movement, AnchorFace gravity)
            => this[cell].PlummetDeflection(movement, gravity).ToArray();

        #region public bool CanPlummet(ICellLocation cell, MovementBase lastMove, MovementBase movement, AnchorFace plummetFace)
        /// <summary>True if this cell can be entered during a plummet (including being blown or swept away)</summary>
        public bool CanPlummet(ICellLocation start, ICellLocation end, MovementBase lastMove, MovementBase movement,
            AnchorFace plummetFace, PlanarPresence planar)
        {
            var _revPlummet = plummetFace.ReverseFace();
            var _currCtx = new MovementTacticalInfo
            {
                Movement = lastMove,
                TransitFaces = plummetFace.ToEnumerable().ToArray(),
                SourceCell = start,
                TargetCell = end
            };
            var _nextCtx = new MovementTacticalInfo
            {
                Movement = movement,
                TransitFaces = plummetFace.ReverseFace().ToEnumerable().ToArray(),
                SourceCell = end,
                TargetCell = start
            };
            return this[start].CanPlummetAcross(lastMove, plummetFace)
                && this[end].CanPlummetAcross(movement, _revPlummet)
                && !MapContext.AllInCell<IMoveAlterer>(start, planar).Any(_iti => _iti.BlocksTransit(_currCtx))
                && !MapContext.AllInCell<IMoveAlterer>(end, planar).Any(_iti => _iti.BlocksTransit(_nextCtx));
        }
        #endregion

        public int? SwimDifficulty(IGeometricRegion startRegion, IGeometricRegion endRegion, MovementBase movement)
        {
            // calm: 10
            // rough: 15
            // stormy: 20 (no taking 10)
            int _difficulty(IGeometricRegion region)
                => (int)Math.Ceiling((from _cLoc in region.AllCellLocations().AsParallel()
                                      let _cell = this[_cLoc]
                                      where _cell != null
                                      select (decimal?)_cell.InnerSwimDifficulty())
                                      .Average() ?? 0);
            // calculate both
            var _start = _difficulty(startRegion);
            var _end = _difficulty(endRegion);

            // get maximum difficulty
            return (new[] { _start > 0 ? (int?)_start : null, _end > 0 ? (int?)_end : null }).Max();
        }

        // TODO: ... edit in map preview window, save/load with XML

        public override CoreProcessManager GenerateProcessManager()
            => new IkosaProcessManager();

        public void Close()
        {
            _Resources?.Close();
            _Resources = null;
        }

        // ICreatureProvider Members
        public Creature GetCreature(Guid creatureID)
            => GetICore<Creature>(creatureID);

        // IInteractProvider Members
        public IInteract GetIInteract(Guid id)
            => GetICore<IInteract>(id);
    }
}