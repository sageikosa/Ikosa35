using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Senses;
using Uzi.Core;
using Uzi.Visualize;
using System.Threading.Tasks;
using Uzi.Ikosa.Movement;
using Uzi.Packaging;
using Uzi.Visualize.Contracts.Tactical;
using System.Diagnostics;
using System.Threading;
using System.Runtime.Serialization;
using Uzi.Visualize.Contracts;
using System.Collections.Concurrent;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class Room : LocalCellGroup, ICorePart, ISerializable
    {
        #region construction
        public Room(string name, ICellLocation location, IGeometricSize size, LocalMap map, bool deepShadows, bool enclosed)
            : base(location, size, map, name, deepShadows)
        {
            var _zh = Convert.ToInt32(ZHeight);
            var _yl = Convert.ToInt32(YLength);
            var _xl = Convert.ToInt32(XLength);
            _Strucs = GetCellStructureArray(_zh, _yl, _xl);
            _Shadings = GetLightShadeLevelArray(_zh, _yl, _xl);
            _IsEnclosed = enclosed;
        }

        protected Room(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            try
            {
                _Strucs = (CellStructure[][][])info.GetValue(nameof(_Strucs), typeof(CellStructure[][][]));
                _Shadings = (LightShadeLevel[][][])info.GetValue(nameof(_Shadings), typeof(LightShadeLevel[][][]));
            }
            catch
            {
            }

            if ((_Strucs == null) || (_Shadings == null))
            {
                // migrate old structure if defined and current not
                try
                {
                    var _cells = (CellStructure[,,])info.GetValue(@"_Cells", typeof(CellStructure[,,]));
                    var _levels = (LightShadeLevel[,,])info.GetValue(@"_Levels", typeof(LightShadeLevel[,,]));
                    if (_cells != null)
                    {
                        var _mz = _cells.GetLength(0);
                        var _my = _cells.GetLength(1);
                        var _mx = _cells.GetLength(2);

                        // copy from old rectangular arrays
                        _Strucs = new CellStructure[_mz][][];
                        _Shadings = new LightShadeLevel[_mz][][];
                        for (var _cz = 0; _cz < _mz; _cz++)
                        {
                            _Strucs[_cz] = new CellStructure[_my][];
                            _Shadings[_cz] = new LightShadeLevel[_my][];
                            for (var _cy = 0; _cy < _my; _cy++)
                            {
                                _Strucs[_cz][_cy] = new CellStructure[_mx];
                                _Shadings[_cz][_cy] = new LightShadeLevel[_mx];
                                for (var _cx = 0; _cx < _mx; _cx++)
                                {
                                    _Strucs[_cz][_cy][_cx] = _cells[_cz, _cy, _cx];
                                    _Shadings[_cz][_cy][_cx] = _levels[_cz, _cy, _cx];
                                }
                            }
                        }
                    }
                }
                catch
                {
                }
            }

            _IsEnclosed = info.GetBoolean(nameof(_IsEnclosed));
        }
        #endregion

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(_Strucs), _Strucs);
            info.AddValue(nameof(_Shadings), _Shadings);
            info.AddValue(nameof(_IsEnclosed), _IsEnclosed);
        }

        #region private CellStructure[][][] GetCellStructureArray(int z, int y, int x)
        private CellStructure[][][] GetCellStructureArray(int z, int y, int x)
        {
            var _strucs = new CellStructure[z][][];
            for (var _cz = 0; _cz < z; _cz++)
            {
                _strucs[_cz] = new CellStructure[y][];
                for (var _cy = 0; _cy < y; _cy++)
                {
                    _strucs[_cz][_cy] = new CellStructure[x];
                    for (var _cx = 0; _cx < x; _cx++)
                    {
                        _strucs[_cz][_cy][_cx] = new CellStructure();
                    }
                }
            }
            return _strucs;
        }
        #endregion

        #region private LightShadeLevel[][][] GetLightShadeLevelArray(int z, int y, int x)
        private LightShadeLevel[][][] GetLightShadeLevelArray(int z, int y, int x)
        {
            var _shades = new LightShadeLevel[z][][];
            for (var _cz = 0; _cz < z; _cz++)
            {
                _shades[_cz] = new LightShadeLevel[y][];
                for (var _cy = 0; _cy < y; _cy++)
                {
                    _shades[_cz][_cy] = new LightShadeLevel[x];
                    for (var _cx = 0; _cx < x; _cx++)
                    {
                        _shades[_cz][_cy][_cx] = LightShadeLevel.None;
                    }
                }
            }
            return _shades;
        }
        #endregion

        #region private data
        private bool _IsEnclosed = true;

        private CellStructure[][][] _Strucs;
        private LightShadeLevel[][][] _Shadings;

        private static Vector3D _ZDrop;
        private static Vector3D _ZLift;
        private static Vector3D _YDrop;
        private static Vector3D _YLift;
        private static Vector3D _XDrop;
        private static Vector3D _XLift;
        #endregion

        #region static construction
        static Room()
        {
            _ZDrop = new Vector3D(0, 0, -5);
            _ZLift = new Vector3D(0, 0, 5);
            _YDrop = new Vector3D(0, -5, 0);
            _YLift = new Vector3D(0, 5, 0);
            _XDrop = new Vector3D(-5, 0, 0);
            _XLift = new Vector3D(5, 0, 0);
        }
        #endregion

        public override bool IsPartOfBackground
            => !_IsEnclosed;

        #region public CellStructure this[int z, int y, int x] { get; set; }
        /// <summary>Gets or sets a IBaseCellSpace using relative room coordinates.  Automatically Relinks room</summary>
        public CellStructure this[int z, int y, int x]
        {
            get
            {
                try
                {
                    return _Strucs[z][y][x];
                }
                catch
                {
                    return default;
                }
            }
            set
            {
                if (value.Template != null)
                {
                    _Strucs[z][y][x] = value;
                    ReLink(true);
                }
            }
        }
        #endregion

        private void DoSetAxis(string axis)
        {
            DoPropertyChanged(axis);
            DoPropertyChanged($@"Lower{axis}");
            DoPropertyChanged($@"Upper{axis}");
        }

        private void DoSetAxisExtent(string axis)
        {
            DoPropertyChanged($@"Upper{axis}");
            DoPropertyChanged($@"{axis}Extent");
            if (axis == @"Z")
            {
                DoPropertyChanged(@"BindableZHeight");
            }
            else
            {
                DoPropertyChanged($@"Bindable{axis}Length");
            }
        }

        #region public void SetCellStructure(int z, int y, int x, in CellStructure structure)
        /// <summary>Sets a IBaseCellSpace using relative room coordinates.  Does not Relink room (used by Fill operations)</summary>
        public void SetCellStructure(int z, int y, int x, in CellStructure structure)
        {
            try
            {
                if (structure.Template != null)
                {
                    _Strucs[z][y][x] = structure;
                }
            }
            catch
            {
            }
        }
        #endregion

        #region public override CellStructure GetCellSpace(int z, int y, int x)
        /// <summary>Gets a GeoCell using map coordinates</summary>
        public override ref readonly CellStructure GetCellSpace(int z, int y, int x)
        {
            if ((z >= _Z) && (z <= _HiZ) && (y >= _Y) && (y <= _HiY) && (x >= _X) && (x <= _HiX))
            {
                return ref _Strucs[z - _Z][y - _Y][x - _X];
            }

            return ref CellStructure.Default;
        }
        #endregion

        /// <summary>Gets a GeoCell using map coordinates</summary>
        public override ref readonly CellStructure GetCellSpace(ICellLocation location)
            => ref GetCellSpace(location.Z, location.Y, location.X);

        //public override CellStructure? GetContainedCellSpace(int z, int y, int x)
        //    => (z >= _Z) && (z <= _HiZ) && (y >= _Y) && (y <= _HiY) && (x >= _X) && (x <= _HiX)
        //    ? _Strucs[z - _Z][y - _Y][x - _X]
        //    : (CellStructure?)null;

        /// <summary>Indicates the position falls within the extents of the drawing array</summary>
        public bool InBounds(int z, int y, int x)
            => (z >= 0) && (z < _ZExt)
            && (y >= 0) && (y < _YExt)
            && (x >= 0) && (x < _XExt);

        #region private void TryCreateLink(CellLocation inRoom, CellLocation outRoom, Axis axis, bool plusFlag)
        private void TryCreateLink(CellLocation inRoom, CellLocation outRoom, Axis axis, bool plusFlag)
        {
            var _externalSet = Map.GetLocalCellGroup(outRoom);
            var _link = PotentialLink(inRoom, outRoom, _externalSet, axis, plusFlag);
            if (_link != null)
            {
                Links.Add(_link);
            }
            else
            {
                Links.AddRoom(_externalSet);
            }
        }
        #endregion

        #region private LocalLink PotentialLink(ICellLocation inRoom, ICellLocation outRoom, Axis axis, bool plusFlag)
        /// <summary>Returns the potential link that would be created, or null, if linking is not possible</summary>
        private LocalLink PotentialLink(ICellLocation inRoom, ICellLocation outRoom, LocalCellGroup externalSet, Axis axis, bool plusFlag)
        {
            // convert axis and flag to AnchorFace
            AnchorFace _face = (axis == Axis.Z ? (plusFlag ? AnchorFace.ZHigh : AnchorFace.ZLow) :
                (axis == Axis.Y ? (plusFlag ? AnchorFace.YHigh : AnchorFace.YLow) :
                (plusFlag ? AnchorFace.XHigh : AnchorFace.XLow)));

            // check whether transitting can occur
            ref readonly var _inCell = ref _Map[inRoom.Z, inRoom.Y, inRoom.X, this];
            var _inMask = _inCell.HedralGripping(LinkMovement.Static, _face).Invert();
            if (_inMask.HasAny)
            {
                // check the opposite plus flag of the outer cell
                ref readonly var _outCell = ref _Map[outRoom];
                var _outFlow = _outCell.HedralGripping(LinkMovement.Static, _face.ReverseFace()).Invert();
                if ((_outFlow.Intersect(_inMask)).HasAny)
                {
                    // find whether there is a cellset involved with the outer cellspace
                    var _externalSet = externalSet ?? Map.GetLocalCellGroup(outRoom);
                    return new LocalLink(_face, this, _externalSet, inRoom, outRoom);
                }
            }
            return null;
        }
        #endregion

        #region public void ReLink(bool reNotify = false)
        /// <summary>Removes old links and creates new ones as needed, then notifies all effected groups to alter lighting</summary>
        public void ReLink(bool reNotify = false)
        {
            var _removedGroups = RemoveDeadLinks().ToList();
            AddLinks();

            var _allGroups = _removedGroups
                .Union(Links.All.SelectMany(_l => _l.Groups).ToList())
                .ToList();

            var _notifiers = _allGroups
                .SelectMany(_g => _g.NotifyLighting())
                .Distinct()
                .ToList();

            AwarenessSet.RecalculateAllSensors(Map, _notifiers, true);

            // gather sounds in all groups
            var _sounds = (from _g in _allGroups
                           from _sg in _g.GetSoundGroups((ID) => true)
                           select _sg).Distinct()
                           .ToDictionary(_sg => _sg.SoundPresence.Audible.SoundGroupID);
            if (_sounds.Any())
            {
                Map.MapContext.SerialState++;
                var _serial = Map.MapContext.SerialState;
                var _updateGroups = new ConcurrentDictionary<Guid, LocalCellGroup>();
                foreach (var _sg in _sounds.Values)
                {
                    // make sounds refresh themselves
                    _sg.SetSoundRef(_sg.SoundPresence.GetRefresh(_serial), _updateGroups);
                }

                // allow locators in groups to handle apparent sound changes
                if (_updateGroups.Any())
                {
                    // all potential listeners
                    _updateGroups.UpdateListenersInGroups((id) => _sounds.ContainsKey(id));
                }
            }
            if (reNotify)
            {
                AwarenessSet.RecalculateAllSensors(Map, NotifyLighting(), true);
            }

            // notify
            Freshness.UpdateFreshness(Map.CurrentTime);
            Redrawn?.Invoke(this, new EventArgs());
        }
        #endregion

        #region public bool AddLinks()
        /// <summary>Called by LocalMap after recreation of map, or when its Rooms collection is altered.</summary>
        public bool AddLinks()
        {
            if (_IsEnclosed)
            {
                var _count = Links.Count;

                // ZMinus and ZPlus Faces
                for (var _y = Y; _y <= UpperY; _y++)
                {
                    for (var _x = X; _x <= UpperX; _x++)
                    {
                        // Z
                        var _inRoom = new CellLocation(Z, _y, _x);
                        var _outRoom = new CellLocation(Z - 1, _y, _x);
                        TryCreateLink(_inRoom, _outRoom, Axis.Z, false);
                        // Upper Z
                        _inRoom = new CellLocation(UpperZ, _y, _x);
                        _outRoom = new CellLocation(UpperZ + 1, _y, _x);
                        TryCreateLink(_inRoom, _outRoom, Axis.Z, true);
                    }
                }

                // YMinus and YPlus Faces
                for (var _z = Z; _z <= UpperZ; _z++)
                {
                    for (var _x = X; _x <= UpperX; _x++)
                    {
                        // Y
                        var _inRoom = new CellLocation(_z, Y, _x);
                        var _outRoom = new CellLocation(_z, Y - 1, _x);
                        TryCreateLink(_inRoom, _outRoom, Axis.Y, false);
                        // UpperY
                        _inRoom = new CellLocation(_z, UpperY, _x);
                        _outRoom = new CellLocation(_z, UpperY + 1, _x);
                        TryCreateLink(_inRoom, _outRoom, Axis.Y, true);
                    }
                }

                // XMinus and XPlus Faces
                for (var _z = Z; _z <= UpperZ; _z++)
                {
                    for (var _y = Y; _y <= UpperY; _y++)
                    {
                        // X
                        var _inRoom = new CellLocation(_z, _y, X);
                        var _outRoom = new CellLocation(_z, _y, X - 1);
                        // UpperX
                        TryCreateLink(_inRoom, _outRoom, Axis.X, false);
                        _inRoom = new CellLocation(_z, _y, UpperX);
                        _outRoom = new CellLocation(_z, _y, UpperX + 1);
                        TryCreateLink(_inRoom, _outRoom, Axis.X, true);
                    }
                }

                return _count != Links.Count;
            }
            return false;
        }
        #endregion

        #region public IEnumerable<LocalCellGroup> RemoveAllLinks()
        /// <summary>Removes links returning all previously linked local cell groups</summary>
        public IEnumerable<LocalCellGroup> RemoveAllLinks()
        {
            if (_IsEnclosed)
            {
                var _clean = Links.All.ToList();
                foreach (var _link in _clean)
                {
                    var _deRoom = _link.Groups.ToList();
                    foreach (var _rm in _deRoom)
                    {
                        _rm.Links.Remove(_link);
                    }
                }

                foreach (var _grp in _clean.SelectMany(_l => _l.Groups).Distinct())
                {
                    yield return _grp;
                }
            }
            yield break;
        }
        #endregion

        #region public IEnumerable<LocalCellGroup> RemoveDeadLinks()
        /// <summary>Removes links returning all previously linked local cell groups</summary>
        public IEnumerable<LocalCellGroup> RemoveDeadLinks()
        {
            if (_IsEnclosed)
            {
                var _clean = new List<LocalLink>();
                foreach (var _link in Links.All.ToList())
                {
                    _clean.Add(_link);
                    var _deRoom = _link.Groups.ToList();
                    foreach (var _rm in _deRoom)
                    {
                        _rm.Links.Remove(_link);
                    }
                }

                foreach (var _grp in _clean.SelectMany(_l => _l.Groups).Distinct())
                {
                    yield return _grp;
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
                // TODO: yield cell spaces? etc...
                yield break;
            }
        }

        public string TypeName
            => GetType().FullName;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        [field: NonSerialized, JsonIgnore]
        public event EventHandler Redrawn;

        // ---------- editing ----------

        #region EDITOR USE DIMENSIONS
        /// <summary>Intended for editor use</summary>
        public int BindableZ { get { return Z; } set { if (value != Z) { Move(value, Y, X); } } }

        /// <summary>Intended for editor use</summary>
        public int BindableY { get { return Y; } set { if (value != Y) { Move(Z, value, X); } } }

        /// <summary>Intended for editor use</summary>
        public int BindableX { get { return X; } set { if (value != X) { Move(Z, Y, value); } } }

        /// <summary>Intended for editor use</summary>
        public long BindableZHeight { get { return ZHeight; } set { if (value != ZHeight) { Resize(value, YLength, XLength); } } }

        /// <summary>Intended for editor use</summary>
        public long BindableYLength { get { return YLength; } set { if (value != YLength) { Resize(ZHeight, value, XLength); } } }

        /// <summary>Intended for editor use</summary>
        public long BindableXLength { get { return XLength; } set { if (value != XLength) { Resize(ZHeight, YLength, value); } } }
        #endregion

        #region public void Move(int z, int y, int x)
        /// <summary>
        /// Moves the room and all locators contained within
        /// </summary>
        /// <param name="z">new Z</param>
        /// <param name="y">new Y</param>
        /// <param name="x">new X</param>
        public void Move(int z, int y, int x)
        {
            // get locators in room
            var _locList = (from _ctx in Map.ContextSet.All()
                            let _mapCtx = _ctx as MapContext
                            where _mapCtx != null
                            from _l in _mapCtx.LocatorsInRegion(this, PlanarPresence.Both)
                            select _l).ToList();

            // locator offsets (could be larger than an Int32)
            var _zD = z - _Z;
            var _yD = y - _Y;
            var _xD = x - _X;
            var _offSet = new CellLocation(_zD, _yD, _xD);

            // move room
            Z = z;
            Y = y;
            X = x;
            Map.RoomIndex.ReIndex(this);

            // update locator positions
            foreach (var _loc in _locList)
            {
                _loc.Relocate(_loc.GeometricRegion.Move(_offSet), _loc.PlanarPresence);
            }

            // lots of stuff changed
            if (_zD != 0)
            {
                DoSetAxis(nameof(Z));
            }

            if (_yD != 0)
            {
                DoSetAxis(nameof(Y));
            }

            if (_xD != 0)
            {
                DoSetAxis(nameof(X));
            }

            DoPropertyChanged(nameof(CenterPoint));

            // relink room to map
            ReLink();
        }
        #endregion

        #region public void Resize(long zHeight, long yLength, long xLength)
        public void Resize(long zHeight, long yLength, long xLength)
        {
            if (zHeight > 0 && yLength > 0 && xLength > 0)
            {
                var _zTop = (1d * Z) + zHeight;
                var _yTop = (1d * Y) + yLength;
                var _xTop = (1d * X) + xLength;

                // must fit inside Int32 range
                if ((_zTop <= Convert.ToDouble(int.MaxValue))
                    && (_yTop <= Convert.ToDouble(int.MaxValue))
                    && (_xTop <= Convert.ToDouble(int.MaxValue)))
                {
                    var _zD = zHeight != ZHeight;
                    var _yD = yLength != YLength;
                    var _xD = xLength != XLength;

                    ZExtent = Convert.ToDouble(zHeight);
                    YExtent = Convert.ToDouble(yLength);
                    XExtent = Convert.ToDouble(xLength);
                    Map.RoomIndex.ReIndex(this);

                    // new arrays
                    var _newCells = GetCellStructureArray((int)zHeight, (int)yLength, (int)xLength);
                    var _newLevels = GetLightShadeLevelArray((int)zHeight, (int)yLength, (int)xLength);

                    // copy
                    for (var _z = 0; _z < Math.Min(zHeight, _Strucs.GetLength(0)); _z++)
                    {
                        for (var _y = 0; _y < Math.Min(yLength, _Strucs[0].GetLength(0)); _y++)
                        {
                            for (var _x = 0; _x < Math.Min(xLength, _Strucs[0][0].GetLength(0)); _x++)
                            {
                                _newCells[_z][_y][_x] = _Strucs[_z][_y][_x];
                                _newLevels[_z][_y][_x] = _Shadings[_z][_y][_x];
                            }
                        }
                    }

                    // replace
                    _Strucs = _newCells;
                    _Shadings = _newLevels;

                    _Map.MapContext.CacheLocatorGroups();

                    // stuff changed
                    if (_zD)
                    {
                        DoSetAxisExtent(nameof(Z));
                    }

                    if (_yD)
                    {
                        DoSetAxisExtent(nameof(Y));
                    }

                    if (_xD)
                    {
                        DoSetAxisExtent(nameof(X));
                    }

                    DoPropertyChanged(nameof(CenterPoint));

                    ReLink();
                }
            }
        }
        #endregion

        #region public void ResizeUp(long zHeight, long yLength, long xLength, int? dupZ, int? dupY, int? dupX)
        public void ResizeUp(long zHeight, long yLength, long xLength, int? dupZ, int? dupY, int? dupX)
        {
            var _zTop = (1d * Z) + zHeight;
            var _yTop = (1d * Y) + yLength;
            var _xTop = (1d * X) + xLength;

            // must fit inside Int32 range
            if ((_zTop <= Convert.ToDouble(int.MaxValue))
                && (_yTop <= Convert.ToDouble(int.MaxValue))
                && (_xTop <= Convert.ToDouble(int.MaxValue)))
            {
                var _zD = zHeight != ZHeight;
                var _yD = yLength != YLength;
                var _xD = xLength != XLength;

                ZExtent = Convert.ToDouble(zHeight);
                YExtent = Convert.ToDouble(yLength);
                XExtent = Convert.ToDouble(xLength);
                Map.RoomIndex.ReIndex(this);

                // new arrays
                var _newCells = GetCellStructureArray((int)zHeight, (int)yLength, (int)xLength);
                var _newLevels = GetLightShadeLevelArray((int)zHeight, (int)yLength, (int)xLength);

                // copy
                for (var _z = 0; _z < Math.Max(zHeight, _Strucs.GetLength(0)); _z++)
                {
                    var _sz = _z <= (dupZ ?? int.MaxValue) ? _z : _z - 1;
                    for (var _y = 0; _y < Math.Max(yLength, _Strucs[0].GetLength(0)); _y++)
                    {
                        var _sy = _y <= (dupY ?? int.MaxValue) ? _y : _y - 1;
                        for (var _x = 0; _x < Math.Max(xLength, _Strucs[0][0].GetLength(0)); _x++)
                        {
                            var _sx = _x <= (dupX ?? int.MaxValue) ? _x : _x - 1;
                            _newCells[_z][_y][_x] = _Strucs[_sz][_sy][_sx];
                            _newLevels[_z][_y][_x] = _Shadings[_sz][_sy][_sx];
                        }
                    }
                }

                // replace
                _Strucs = _newCells;
                _Shadings = _newLevels;

                _Map.MapContext.CacheLocatorGroups();

                // stuff changed
                if (_zD)
                {
                    DoSetAxisExtent(nameof(Z));
                }

                if (_yD)
                {
                    DoSetAxisExtent(nameof(Y));
                }

                if (_xD)
                {
                    DoSetAxisExtent(nameof(X));
                }

                DoPropertyChanged(nameof(CenterPoint));

                ReLink();
            }
        }
        #endregion

        #region public void ResizeDown(long zHeight, long yLength, long xLength, int? dropZ, int? dropY, int? dropX)
        public void ResizeDown(long zHeight, long yLength, long xLength, int? dropZ, int? dropY, int? dropX)
        {
            if (zHeight > 0 && yLength > 0 && xLength > 0)
            {
                var _zTop = (1d * Z) + zHeight;
                var _yTop = (1d * Y) + yLength;
                var _xTop = (1d * X) + xLength;

                // must fit inside Int32 range
                if ((_zTop <= Convert.ToDouble(int.MaxValue))
                    && (_yTop <= Convert.ToDouble(int.MaxValue))
                    && (_xTop <= Convert.ToDouble(int.MaxValue)))
                {
                    var _zD = zHeight != ZHeight;
                    var _yD = yLength != YLength;
                    var _xD = xLength != XLength;

                    ZExtent = Convert.ToDouble(zHeight);
                    YExtent = Convert.ToDouble(yLength);
                    XExtent = Convert.ToDouble(xLength);
                    Map.RoomIndex.ReIndex(this);

                    // new arrays
                    var _newCells = GetCellStructureArray((int)zHeight, (int)yLength, (int)xLength);
                    var _newLevels = GetLightShadeLevelArray((int)zHeight, (int)yLength, (int)xLength);

                    // copy
                    for (var _z = 0; _z < Math.Min(zHeight, _Strucs.GetLength(0)); _z++)
                    {
                        var _sz = _z < (dropZ ?? int.MaxValue) ? _z : _z + 1;
                        for (var _y = 0; _y < Math.Min(yLength, _Strucs[0].GetLength(0)); _y++)
                        {
                            var _sy = _y < (dropY ?? int.MaxValue) ? _y : _y + 1;
                            for (var _x = 0; _x < Math.Min(xLength, _Strucs[0][0].GetLength(0)); _x++)
                            {
                                var _sx = _x < (dropX ?? int.MaxValue) ? _x : _x + 1;
                                _newCells[_z][_y][_x] = _Strucs[_sz][_sy][_sx];
                                _newLevels[_z][_y][_x] = _Shadings[_sz][_sy][_sx];
                            }
                        }
                    }

                    // replace
                    _Strucs = _newCells;
                    _Shadings = _newLevels;

                    _Map.MapContext.CacheLocatorGroups();

                    // stuff changed
                    if (_zD)
                    {
                        DoSetAxisExtent(nameof(Z));
                    }

                    if (_yD)
                    {
                        DoSetAxisExtent(nameof(Y));
                    }

                    if (_xD)
                    {
                        DoSetAxisExtent(nameof(X));
                    }

                    DoPropertyChanged(nameof(CenterPoint));

                    ReLink();
                }
            }
        }
        #endregion

        #region public void Flip(Axis flipAxis)
        public void Flip(Axis flipAxis)
        {
            var _newCells = GetCellStructureArray((int)ZHeight, (int)YLength, (int)XLength);
            long _zIdx(long refVal) => flipAxis == Axis.Z ? (ZHeight - 1) - refVal : refVal;
            long _yIdx(long refVal) => flipAxis == Axis.Y ? (YLength - 1) - refVal : refVal;
            long _xIdx(long refVal) => flipAxis == Axis.X ? (XLength - 1) - refVal : refVal;
            for (long _z = 0; _z < ZHeight; _z++)
            {
                for (long _y = 0; _y < YLength; _y++)
                {
                    for (long _x = 0; _x < XLength; _x++)
                    {
                        _newCells[_z][_y][_x] = _Strucs[_zIdx(_z)][_yIdx(_y)][_xIdx(_x)].FlipAxis(flipAxis);
                    }
                }
            }

            // new cells
            _Strucs = _newCells;

            // links, lighting, and notifications
            ReLink();
        }
        #endregion

        #region public void Swap(Axis primeAxis)
        /// <summary>
        /// <para>PrimeAxis:</para>
        /// <para>Z = flips with Y</para>
        /// <para>Y = flips with X</para>
        /// <para>X = flips with Z</para>
        /// </summary>
        /// <param name="primeAxis"></param>
        public void Swap(Axis primeAxis)
        {
            var _zH = ZHeight;
            var _yL = YLength;
            var _xL = XLength;
            void _doLoop(Action<long, long, long> swap)
            {
                for (long _z = 0; _z < ZHeight; _z++)
                {
                    for (long _y = 0; _y < YLength; _y++)
                    {
                        for (long _x = 0; _x < XLength; _x++)
                        {
                            swap(_z, _y, _x);
                        }
                    }
                }
            }
            ;

            CellStructure[][][] _newCells = null;
            LightShadeLevel[][][] _newLevels = null;
            switch (primeAxis)
            {
                case Axis.Z:
                    _newCells = GetCellStructureArray((int)YLength, (int)ZHeight, (int)XLength);
                    _newLevels = GetLightShadeLevelArray((int)YLength, (int)ZHeight, (int)XLength);
                    _zH = YLength;
                    _yL = ZHeight;
                    _doLoop((z, y, x) =>
                    {
                        // copy cells
                        _newCells[y][z][x] = _Strucs[z][y][x].SwapAxis(Axis.Z, Axis.Y);
                        _newLevels[y][z][x] = _Shadings[z][y][x];
                    });
                    break;

                case Axis.Y:
                    _newCells = GetCellStructureArray((int)ZHeight, (int)XLength, (int)YLength);
                    _newLevels = GetLightShadeLevelArray((int)ZHeight, (int)XLength, (int)YLength);
                    _xL = YLength;
                    _yL = XLength;
                    _doLoop((z, y, x) =>
                    {
                        // copy cells
                        _newCells[z][x][y] = _Strucs[z][y][x].SwapAxis(Axis.Y, Axis.X);
                        _newLevels[z][x][y] = _Shadings[z][y][x];
                    });
                    break;

                case Axis.X:
                    _newCells = GetCellStructureArray((int)XLength, (int)YLength, (int)ZHeight);
                    _newLevels = GetLightShadeLevelArray((int)XLength, (int)YLength, (int)ZHeight);
                    _zH = XLength;
                    _xL = ZHeight;
                    _doLoop((z, y, x) =>
                    {
                        // copy cells
                        _newCells[x][y][z] = _Strucs[z][y][x].SwapAxis(Axis.X, Axis.Z);
                        _newLevels[x][y][z] = _Shadings[z][y][x];
                    });
                    break;

                default:
                    return;
            }

            // cells
            _Strucs = _newCells;
            _Shadings = _newLevels;

            // links, lighting, and notifications
            Resize(_zH, _yL, _xL);
        }
        #endregion

        #region public bool CanSplit(Axis axis, int clipAt)
        public bool CanSplit(Axis axis, int clipAt)
        {
            // bounds checks
            if (clipAt < 0)
            {
                return false;
            }

            switch (axis)
            {
                case Axis.X: if (clipAt > (XLength - 2)) { return false; } break;
                case Axis.Y: if (clipAt > (YLength - 2)) { return false; } break;
                case Axis.Z: if (clipAt > (ZHeight - 2)) { return false; } break;
            }
            return true;
        }
        #endregion

        #region public void Split(Axis axis, int clipAt)
        public void Split(Axis axis, int clipAt)
        {
            // bounds checks
            if (clipAt < 0)
            {
                return;
            }

            switch (axis)
            {
                case Axis.X: if (clipAt > (XLength - 2)) { return; } break;
                case Axis.Y: if (clipAt > (YLength - 2)) { return; } break;
                case Axis.Z: if (clipAt > (ZHeight - 2)) { return; } break;
            }

            // offsets based on axis
            var _zOff = axis == Axis.Z ? clipAt + 1 : 0;
            var _yOff = axis == Axis.Y ? clipAt + 1 : 0;
            var _xOff = axis == Axis.X ? clipAt + 1 : 0;

            // new room
            var _newRoom = new Room($@"{Name}.2",
                new CellPosition(Z + _zOff, Y + _yOff, X + _xOff),
                new GeometricSize(ZExtent - _zOff, YExtent - _yOff, XExtent - _xOff),
                Map, DeepShadows, !IsPartOfBackground);

            // copy cell
            for (var _nrz = 0; _nrz < _newRoom.ZHeight; _nrz++)
            {
                for (var _nry = 0; _nry < _newRoom.YLength; _nry++)
                {
                    for (var _nrx = 0; _nrx < _newRoom.XLength; _nrx++)
                    {
                        _newRoom.SetCellStructure(_nrz, _nry, _nrx, _Strucs[_nrz + _zOff][_nry + _yOff][_nrx + _xOff]);
                    }
                }
            }

            // rename and resize
            Name = $@"{Name}.1";
            Resize(
                (axis == Axis.Z ? _zOff : ZHeight),
                (axis == Axis.Y ? _yOff : YLength),
                (axis == Axis.X ? _xOff : XLength));

            Map.Rooms.Add(_newRoom);
            _newRoom.ReLink(true);
        }
        #endregion

        // ---------- sensory interaction ----------

        #region public override IEnumerable<(SoundRef sound, IGeometricRegion region, double magnitude)> GetSoundRefs(IGeometricContext target)
        public override IEnumerable<(SoundRef sound, IGeometricRegion region, double magnitude)> GetSoundRefs(IGeometricContext target,
            Func<Guid, bool> soundFilter)
        {
            if (!IsPartOfBackground)
            {
                // sound participants in the room
                foreach (var _sound in Map.MapContext.ListEffectsInRoom<SoundParticipant>(this)
                    .Where(_s => soundFilter(_s.SoundGroup.SoundPresence.Audible.SoundGroupID)))
                {
                    var _rgn = _sound.GetGeometricRegion();
                    yield return (_sound.SoundGroup.SoundPresence, _rgn, _sound.SoundGroup.SoundPresence.GetRelativeMagnitude(_rgn, target));
                }

                // sound channel sound references
                foreach (var _channel in from _l in Links.All
                                         from _sckvp in _l.ChannelsTo(this)
                                         let _sc = _sckvp.Value
                                         where _sc.ChannelFlow.project == this
                                         && _sc.SoundPresence != null
                                         && soundFilter(_sc.SoundPresence.Audible.SoundGroupID)
                                         select _sc)
                {
                    var _cell = _channel.ChannelFlow.link.InteractionCell(_channel.ChannelFlow.project);
                    yield return (_channel.SoundPresence, _channel.SoundPresence.Cell ?? _cell, _channel.SoundPresence.GetRelativeMagnitude(_cell, target));
                }
            }
            else
            {
                // shared sounds
                foreach (var _sound in base.GetSoundRefs(target, soundFilter))
                {
                    yield return _sound;
                }
            }
            yield break;
        }
        #endregion

        #region public override IEnumerable<SoundGroup> GetSoundGroups(Func<Guid, bool> soundFilter)
        public override IEnumerable<SoundGroup> GetSoundGroups(Func<Guid, bool> soundFilter)
        {
            if (!IsPartOfBackground)
            {
                // sound participants in the room
                foreach (var _sound in Map.MapContext.ListEffectsInRoom<SoundParticipant>(this)
                    .Where(_s => soundFilter(_s.SoundGroup.SoundPresence.Audible.SoundGroupID)))
                {
                    yield return _sound.SoundGroup;
                }

                // sound channel sound references
                foreach (var _channel in from _l in Links.All
                                         from _sckvp in _l.ChannelsTo(this)
                                         let _sc = _sckvp.Value
                                         where _sc.ChannelFlow.project == this
                                         && _sc.SoundPresence != null
                                         && soundFilter(_sc.SoundPresence.Audible.SoundGroupID)
                                         select _sc)
                {
                    yield return _channel.SoundGroup;
                }
            }
            else
            {
                // shared sounds
                foreach (var _sound in base.GetSoundGroups(soundFilter))
                {
                    yield return _sound;
                }
            }
            yield break;
        }
        #endregion

        #region public override IEnumerable<IIllumination> GetIlluminators(bool linking)
        public override IEnumerable<IIllumination> GetIlluminators(bool linking)
        {
            if (!IsPartOfBackground)
            {
                // only room illuminations
                foreach (var _illum in Map.MapContext.ListEffectsInRoom<Illumination>(this)
                    .OfType<IIllumination>()
                    .Where(_i => _i.IsUsable && _i.PlanarPresence.HasMaterialPresence()))
                {
                    yield return _illum;
                }

                // link illuminations to enclosed room only
                foreach (var _light in from _lnk in Links.All
                                       from _l in _lnk.GetLights(this, linking)
                                       select _l)
                {
                    yield return _light;
                }
            }
            else
            {
                // shared lighting
                foreach (var _light in base.GetIlluminators(linking))
                {
                    yield return _light;
                }
            }
            yield break;
        }
        #endregion

        #region public override void RefreshTerrainShading()
        /// <summary>Recalculates all shading levels for the room</summary>
        public override void RefreshTerrainShading()
        {
            // clear all shading flags
            var _ptLevels = GetLightShadeLevelArray((int)ZHeight + 1, (int)YLength + 1, (int)XLength + 1);

            // TODO: magical pitch dark...?
            _Shadings = GetLightShadeLevelArray((int)ZHeight, (int)YLength, (int)XLength);

            // grab all lights in room
            var _lights = GetAmbientIlluminators().Union(GetIlluminators(false)).Distinct().ToList();

            #region grab all magic darks and magic lights
            // and all magic light and dark on the map
            var _mDarks = (from _cap in Map.MapContext.LocatorZones.AllCaptures()
                           where _cap.Capturer is MagicDark
                           let _md = _cap.Capturer as MagicDark
                           let _mpe = _md.Source as MagicPowerEffect
                           orderby _mpe.PowerLevel descending
                           select new { Capture = _cap, MagicEffect = _mpe, Dark = _md }).ToList();

            var _mLights = (from _cap in Map.MapContext.LocatorZones.AllCaptures()
                            where _cap.Capturer is MagicLight
                            let _ml = _cap.Capturer as MagicLight
                            let _mpe = _ml.Source as MagicPowerEffect
                            orderby _mpe.PowerLevel descending
                            select new { Capture = _cap, MagicEffect = _mpe, Light = _ml }).ToList();
            #endregion

            // every cell in room will be visited by every light

            // NOTE: using local map coordinates...
            Parallel.ForEach(AllIntersectionPartitions(), (points) =>
            {
                var _factory = new SegmentSetFactory(Map, this, this, ITacticalInquiryHelper.EmptyArray,
                    SegmentSetProcess.Effect);
                foreach (var _pt in points)
                {
                    var _z = _pt.Z - Z;
                    var _y = _pt.Y - Y;
                    var _x = _pt.X - X;
                    var _p3d = _pt.Point3D();

                    var _doCalc = true;
                    var _useLights = _lights.OrderByDescending(_l => _l.MaximumLight).ToList();

                    #region magic dark and magic light
                    // get the best darkness
                    var _bestDark = _mDarks.FirstOrDefault(_d => _d.Capture.ContainsCell(_pt, null, PlanarPresence.Material));
                    if (_bestDark != null)
                    {
                        var _bestMLight = _mLights.FirstOrDefault(_l => _l.Capture.ContainsCell(_pt, null, PlanarPresence.Material));
                        if (_bestMLight != null)
                        {
                            if (_bestDark.MagicEffect.PowerLevel > _bestMLight.MagicEffect.PowerLevel)
                            {
                                // magic dark wins
                                _ptLevels[_z][_y][_x] = LightShadeLevel.MagicDark;
                                _doCalc = false;
                            }
                            else if (_bestDark.MagicEffect.PowerLevel < _bestMLight.MagicEffect.PowerLevel)
                            {
                                // magic light wins
                                if (_bestMLight.Light.IsVeryBright)
                                {
                                    _ptLevels[_z][_y][_x] = LightShadeLevel.VeryBright;
                                }
                                else
                                {
                                    _ptLevels[_z][_y][_x] = LightShadeLevel.Bright;
                                }
                                _doCalc = false;
                            }
                            else
                            {
                                // cancellation (no magic lighting to check shading)
                                _useLights = (from _l in _useLights
                                              where !(_l is MagicLight)
                                              select _l).ToList();
                            }
                        }
                        else
                        {
                            // only dark, no magic light
                            _ptLevels[_z][_y][_x] = LightShadeLevel.MagicDark;
                            _doCalc = false;
                        }
                    }
                    #endregion

                    // still doing calculations?
                    if (_doCalc && (_useLights.Count > 0))
                    {
                        // then the standard shading
                        var _currentRange = LightRange.OutOfRange;
                        var _max = _useLights.First().MaximumLight;
                        foreach (var _l in _useLights)
                        {
                            var _lp = _l.InteractionPoint3D(_p3d);
                            var _cellLeft = new CellLightRanger(_l, _p3d);
                            var _newRange = _cellLeft.CurrentRange;
                            if (_newRange > _currentRange)
                            {
                                if (Map.SegmentCells(_lp, _p3d, _factory, PlanarPresence.Material).IsLineOfEffect)
                                {
                                    _currentRange = _newRange;
                                }
                            }
                            if (_currentRange >= _max)
                            {
                                break;
                            }
                        }

                        // flag as processed to minimize overlap processing
                        _ptLevels[_z][_y][_x] = _currentRange switch
                        {
                            LightRange.ExtentBoost => LightShadeLevel.ExtentBoost,
                            LightRange.FarShadow => LightShadeLevel.FarShadow,
                            LightRange.FarBoost => LightShadeLevel.FarBoost,
                            LightRange.NearShadow => LightShadeLevel.NearShadow,
                            LightRange.NearBoost => LightShadeLevel.NearBoost,
                            LightRange.Bright => LightShadeLevel.Bright,
                            LightRange.VeryBright => LightShadeLevel.VeryBright,
                            LightRange.Solar => LightShadeLevel.VeryBright,
                            _ => _ptLevels[_z][_y][_x]
                        };
                    }
                }
            });

            Parallel.ForEach(AllCubicPartitions(), (cells) =>
            {
                foreach (var _loc in cells)
                {
                    var _z = _loc.Z - Z;
                    var _y = _loc.Y - Y;
                    var _x = _loc.X - X;

                    if (_ptLevels[_z][_y][_x] == LightShadeLevel.MagicDark)
                    {
                        // if the cell was calculated as magic dark, then lock it in
                        _Shadings[_z][_y][_x] = LightShadeLevel.MagicDark;
                    }
                    else
                    {
                        var _count = new byte[9];
                        _count[(int)_ptLevels[_z][_y][_x] + 1]++;
                        _count[(int)_ptLevels[_z][_y][_x + 1] + 1]++;
                        _count[(int)_ptLevels[_z][_y + 1][_x] + 1]++;
                        _count[(int)_ptLevels[_z][_y + 1][_x + 1] + 1]++;
                        _count[(int)_ptLevels[_z + 1][_y][_x] + 1]++;
                        _count[(int)_ptLevels[_z + 1][_y][_x + 1] + 1]++;
                        _count[(int)_ptLevels[_z + 1][_y + 1][_x] + 1]++;
                        _count[(int)_ptLevels[_z + 1][_y + 1][_x + 1] + 1]++;

                        var _someLight = false;
                        for (byte _cx = 8; _cx > 0; _cx--)
                        {
                            var _sum = _count[_cx];
                            if (_sum >= 2)
                            {
                                // 2 means we get this level
                                _Shadings[_z][_y][_x] = (LightShadeLevel)_cx - 1;
                                break;
                            }
                            else if (_sum == 1)
                            {
                                if (_someLight || (_cx < 4))
                                {
                                    // 1 and some previous light, we get this level
                                    _Shadings[_z][_y][_x] = (LightShadeLevel)_cx - 1;
                                    break;
                                }
                                else
                                {
                                    _someLight = true;
                                }
                            }
                        }
                    }
                }
            });
        }
        #endregion

        #region public VisualEffect CellEffect(IGeometricRegion location, int z, int y, int x, TerrainVisualizer visualizer)
        public VisualEffect CellEffect(IGeometricRegion location, int z, int y, int x, TerrainVisualizer visualizer)
        {
            VisualEffect _effect = VisualEffect.Unseen;
            var _cLoc = new CellLocation(z, y, x);
            var _distance = location.NearDistanceToCell((ICellLocation)_cLoc);
            var _z = z - Z;
            var _y = y - Y;
            var _x = x - X;

            #region determine visual effect
            // if sight is not usable, check in-range (and line of effect)
            if (visualizer.NoSight)
            {
                #region blindsight only
                // only terrain visualization senses with no sight requirements
                var _sightless = visualizer.FilteredSenses
                    .Where(_s => _distance <= _s.Range)
                    .OrderByDescending(_s => _s.Range);
                var _noTrans = _sightless.FirstOrDefault(_s => !_s.UsesSenseTransit);
                var _useTrans = _sightless.FirstOrDefault(_s => _s.UsesSenseTransit);
                if ((_useTrans != null)
                    && ((_noTrans == null) || (_noTrans.Range < _useTrans.Range))
                    && _useTrans.CarrySenseInteraction(Map, location, _cLoc, ITacticalInquiryHelper.EmptyArray))
                {
                    _effect = VisualEffectProcessor.GetFormOnlyLevel(_distance, _useTrans.Range);
                }
                else if (_noTrans != null)
                {
                    _effect = VisualEffectProcessor.GetFormOnlyLevel(_distance, _noTrans.Range);
                }
                #endregion

                // no need to test out-of-room line of effect, as sense was already carried
                return _effect;
            }
            else
            {
                // should be at least one terrain visualizing sense that uses sight...
                if (_Shadings[_z][_y][_x] == LightShadeLevel.MagicDark)
                {
                    // default to 50%
                    _effect = VisualEffect.DimTo50;

                    #region any sense that ignores visual effects
                    // there is at least one sense that ignores visual effects
                    if (visualizer.IgnoreVisual)
                    {
                        // so see if any allow terrain visualization (may actually be 
                        foreach (var _magicDarkPiercing
                            in visualizer.IgnoresVisualEffects.Where(_s => _distance <= _s.Range))
                        {
                            if (!_magicDarkPiercing.UsesSenseTransit
                                || _magicDarkPiercing.CarrySenseInteraction(Map, location, _cLoc, ITacticalInquiryHelper.EmptyArray))
                            {
                                if (_magicDarkPiercing.UsesSight)
                                {
                                    // obtained highest detail
                                    _effect = VisualEffect.Normal;
                                    break;
                                }
                                else
                                {
                                    var _eff = VisualEffectProcessor.GetFormOnlyLevel(_distance, _magicDarkPiercing.Range);
                                    if (_eff > _effect)
                                    {
                                        _effect = _eff;
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }

                // still need to probe for visualization effect?
                // NOTE: since we've gotten past magic darkness, only concerned with light
                if (_effect == VisualEffect.Unseen)
                {
                    var _level = _Shadings[_z][_y][_x];
                    var _sight = visualizer.UsesSight.Where(_s => _s.Range >= _distance);
                    if (_sight.Any())
                    {
                        if (_sight.Any(_ir => _ir.IgnoresVisualEffects))
                        {
                            // sighted sense ignoring visual effects doesn't care about light levels
                            _effect = VisualEffect.Normal;
                        }
                        else if ((_level == LightShadeLevel.Bright) || (_level == LightShadeLevel.VeryBright))
                        {
                            #region brightly lit
                            if (_sight.Any(_ir => _ir.UsesLight))
                            {
                                if (_level == LightShadeLevel.VeryBright)
                                {
                                    _effect = VisualEffect.Brighter;
                                }
                                else
                                {
                                    _effect = VisualEffect.Normal;
                                }
                            }
                            else
                            {
                                // dark vision (and true seeing...)
                                _effect = VisualEffectProcessor.GetMonochromeLevel(_distance, _sight.Where(_ir => !_ir.UsesLight).Max(_ir => _ir.Range));
                            }
                            #endregion
                        }
                        else if ((_level == LightShadeLevel.NearShadow) || (_level == LightShadeLevel.NearBoost))
                        {
                            #region regular shadowy
                            if (_sight.Any(_ir => _ir.UsesLight && _ir.LowLight))
                            {
                                // low light vision and true seeing
                                _effect = VisualEffect.Normal;
                            }
                            else if (_sight.Any(_ir => !_ir.UsesLight))
                            {
                                if (_sight.Any(_ir => _ir.UsesLight))
                                {
                                    // dark vision (but sensor also uses normal vision)
                                    _effect = VisualEffect.Normal;
                                }
                                else
                                {
                                    // dark vision only
                                    _effect = VisualEffectProcessor.GetMonochromeLevel(_distance, _sight.Where(_ir => !_ir.UsesLight).Max(_ir => _ir.Range));
                                }
                            }
                            else
                            {
                                if (_level == LightShadeLevel.NearBoost)
                                {
                                    _effect = VisualEffect.DimTo75;
                                }
                                else
                                {
                                    _effect = VisualEffect.DimTo50;
                                }
                            }
                            #endregion
                        }
                        else if (_sight.Any(_ir => !_ir.UsesLight))
                        {
                            _effect = VisualEffectProcessor.GetMonochromeLevel(_distance, _sight.Where(_ir => !_ir.UsesLight)
                                .Max(_ir => _ir.Range));
                        }
                        else if ((_level == LightShadeLevel.FarShadow) || (_level == LightShadeLevel.FarBoost))
                        {
                            if (_sight.Any(_ir => _ir.UsesLight && _ir.LowLight))
                            {
                                // low light vision is all that's left if we haven't found darkvision
                                if (_level == LightShadeLevel.FarBoost)
                                {
                                    _effect = VisualEffect.DimTo75;
                                }
                                else
                                {
                                    _effect = VisualEffect.DimTo50;
                                }
                            }
                            else
                            {
                                // far shade but on fringe for near shade
                                if (_level == LightShadeLevel.FarBoost)
                                {
                                    _effect = VisualEffect.DimTo25;
                                }
                            }
                        }
                        else
                        {
                            // beyond far shade
                            if ((_level == LightShadeLevel.ExtentBoost) && _sight.Any(_ir => _ir.UsesLight && _ir.LowLight))
                            {
                                _effect = VisualEffect.DimTo25;
                            }
                        }
                    }

                    // if not normal or monochrome, allow non-sighted sense a chance to affect visualization
                    if ((_effect != VisualEffect.Normal)
                        && (_effect != VisualEffect.Brighter)
                        && (!_effect.IsMonochrome()))
                    {
                        // so see if any allow terrain visualization 
                        foreach (var _formSense in visualizer.NotUsesSight.Where(_s => _distance <= _s.Range))
                        {
                            if (!_formSense.UsesSenseTransit
                                || _formSense.CarrySenseInteraction(Map, location, _cLoc, ITacticalInquiryHelper.EmptyArray))
                            {
                                var _eff = VisualEffectProcessor.GetFormOnlyLevel(_distance, _formSense.Range);
                                if (_eff > _effect)
                                {
                                    _effect = _eff;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            // yield next effect
            return _effect;
        }
        #endregion

        #region public IEnumerable<VisualEffect> YieldEffects(IGeometricRegion location, bool alphaChannel, TerrainVisualizer visualizer)
        public IEnumerable<VisualEffect> YieldEffects(IGeometricRegion location, bool alphaChannel, TerrainVisualizer visualizer)
        {
            // TODO: control opaque/transparent

            // if no active senses perform terrain visualization, no point in generating models
            if (visualizer.FilteredSenses.Count != 0)
            {
                //var _roomCellAware = new RoomCellsAwareness(location, this, locationRooms, renderedRooms);

                // render per observer point (include: special senses; darkvision/darkvision magic darkness/blind-sight)
                for (var _z = LowerZ; _z <= UpperZ; _z++)
                {
                    for (var _y = LowerY; _y <= UpperY; _y++)
                    {
                        for (var _x = LowerX; _x <= UpperX; _x++)
                        {
                            yield return CellEffect(location, _z, _y, _x, visualizer);
                        }
                    }
                }
            }

            // done yielding
            yield break;
        }
        #endregion

        #region public IEnumerable<PanelShadingInfo> YieldPanels(ISensorHost sensors)
        public IEnumerable<PanelShadingInfo> YieldPanels(ISensorHost sensors)
        {
            // panel shades at links...
            var _inside = (from _lnk in Links.All
                           let _cell = _lnk.InteractionCell(this)
                           from _shade in Map.MapContext.AllInCell<IPanelShade>(_cell, PlanarPresence.Both)
                           from _info in _shade.GetPanelShadings(sensors, true, _cell)
                           select _info);

            var _outside = (from _lnk in Links.All
                            let _cell = _lnk.OutsideInteractionCell(this)
                            from _shade in Map.MapContext.AllInCell<IPanelShade>(_cell, PlanarPresence.Both)
                            from _info in _shade.GetPanelShadings(sensors, false, _cell)
                            select _info);

            // union both
            return _inside.Union(_outside);
        }
        #endregion

        #region public BuildableGroup GenerateModel(IEnumerable<VisualEffect> effects)
        public BuildableGroup GenerateModel(IEnumerable<VisualEffect> effects)
        {
            var _localOpaque = new Model3DGroup();
            var _localAlpha = new Model3DGroup();
            var _globalContext = new BuildableContext();

            // loop over region
            var _z = 0;
            var _y = 0;
            var _x = 0;
            var _cellOpaque = new Model3DGroup();
            var _cellAlpha = new Model3DGroup();
            foreach (var _effect in effects)
            {
                var _cz = _Z + _z;
                var _cy = _Y + _y;
                var _cx = _X + _x;

                // render faces
                _cellOpaque = _cellOpaque.Children.Count > 0 ? new Model3DGroup() : _cellOpaque;
                _cellAlpha = _cellOpaque.Children.Count > 0 ? new Model3DGroup() : _cellAlpha;
                var _group = new BuildableGroup { Context = _globalContext, Opaque = _cellOpaque, Alpha = _cellAlpha };
                _Map[_cz - 1, _cy, _cx, this].AddOuterSurface(_group, _cz - 1, _cy, _cx, AnchorFace.ZHigh, _effect, _ZDrop, this);
                _Map[_cz + 1, _cy, _cx, this].AddOuterSurface(_group, _cz + 1, _cy, _cx, AnchorFace.ZLow, _effect, _ZLift, this);
                _Map[_cz, _cy - 1, _cx, this].AddOuterSurface(_group, _cz, _cy - 1, _cx, AnchorFace.YHigh, _effect, _YDrop, this);
                _Map[_cz, _cy + 1, _cx, this].AddOuterSurface(_group, _cz, _cy + 1, _cx, AnchorFace.YLow, _effect, _YLift, this);
                _Map[_cz, _cy, _cx - 1, this].AddOuterSurface(_group, _cz, _cy, _cx - 1, AnchorFace.XHigh, _effect, _XDrop, this);
                _Map[_cz, _cy, _cx + 1, this].AddOuterSurface(_group, _cz, _cy, _cx + 1, AnchorFace.XLow, _effect, _XLift, this);

                // internal structures
                ref readonly var _currStruc = ref _Map[_cz, _cy, _cx, this];
                _currStruc.AddInnerStructures(_group, _cz, _cy, _cx, _effect);
                _currStruc.AddExteriorSurface(_group, _cz, _cy, _cx, AnchorFace.ZHigh, _effect, this);
                _currStruc.AddExteriorSurface(_group, _cz, _cy, _cx, AnchorFace.ZLow, _effect, this);
                _currStruc.AddExteriorSurface(_group, _cz, _cy, _cx, AnchorFace.YHigh, _effect, this);
                _currStruc.AddExteriorSurface(_group, _cz, _cy, _cx, AnchorFace.YLow, _effect, this);
                _currStruc.AddExteriorSurface(_group, _cz, _cy, _cx, AnchorFace.XHigh, _effect, this);
                _currStruc.AddExteriorSurface(_group, _cz, _cy, _cx, AnchorFace.XLow, _effect, this);

                // merge buildables
                if (_cellOpaque.Children.Count > 0)
                {
                    _cellOpaque.Transform = new TranslateTransform3D(_x * 5d, _y * 5d, _z * 5d);
                    _cellOpaque.Freeze();
                    _localOpaque.Children.Add(_cellOpaque);
                }
                if (_cellAlpha.Children.Count > 0)
                {
                    _cellAlpha.Transform = new TranslateTransform3D(_x * 5d, _y * 5d, _z * 5d);
                    _cellAlpha.Freeze();
                    _localAlpha.Children.Add(_cellAlpha);
                }

                #region cell coordinates
                // end of loop counter increments
                _x++;
                if (_x >= XLength)
                {
                    _x = 0;
                    _y++;
                    if (_y >= YLength)
                    {
                        _y = 0;
                        _z++;

                        // NOTE: this is a failsafe, shouldn't have more cells than effects
                        if (_z >= ZHeight)
                        {
                            break;
                        }
                    }
                }
                #endregion
            }

            // merge buildable context
            var _move = new TranslateTransform3D(X * 5d, Y * 5d, Z * 5D);
            Model3DGroup _getFinal(bool alpha, Model3DGroup gathered)
            {
                var _final = new Model3DGroup();
                foreach (var _m in _globalContext.GetModel3D(alpha))
                {
                    _final.Children.Add(_m);
                }

                if (gathered.Children.Count > 0)
                {
                    gathered.Transform = _move;
                    _final.Children.Add(gathered);
                }
                if (_final.Children.Count > 0)
                {
                    _final.Freeze();
                    return _final;
                }
                return null;
            };

            // return
            return new BuildableGroup
            {
                Alpha = _getFinal(true, _localAlpha),
                Opaque = _getFinal(false, _localOpaque)
            };
        }
        #endregion

        // TODO: observer drawing list (for client/server delivery)

        #region public override LightRange GetLightLevel(ICellLocation location)
        public override LightRange GetLightLevel(ICellLocation location)
        {
            var _z = location.Z - Z;
            var _y = location.Y - Y;
            var _x = location.X - X;
            return (_Shadings[_z][_y][_x]) switch
            {
                LightShadeLevel.VeryBright => LightRange.VeryBright, // TODO: solar?
                LightShadeLevel.Bright => LightRange.Bright,
                LightShadeLevel.NearBoost => LightRange.NearBoost,
                LightShadeLevel.NearShadow => LightRange.NearShadow,
                LightShadeLevel.FarBoost => LightRange.FarBoost,
                LightShadeLevel.FarShadow => LightRange.FarShadow,
                LightShadeLevel.ExtentBoost => LightRange.ExtentBoost,
                _ => LightRange.OutOfRange,
            };
        }
        #endregion

        #region public IEnumerable<uint> LinearIDs { get; }
        /// <summary>Linearizes the IDs for service transmittal</summary>
        public IEnumerable<ulong> LinearIDs
        {
            get
            {
                for (var _z = 0; _z < ZHeight; _z++)
                {
                    for (var _y = 0; _y < YLength; _y++)
                    {
                        for (var _x = 0; _x < XLength; _x++)
                        {
                            yield return _Strucs[_z][_y][_x].ID;
                        }
                    }
                }

                yield break;
            }
        }
        #endregion

        public override bool IsInMagicDarkness(ICellLocation location)
            => _Shadings[location.Z - Z][location.Y - Y][location.X - X] == LightShadeLevel.MagicDark;

        public RoomInfo ToRoomInfo()
        {
            var _info = ToInfo<RoomInfo>();
            _info.CellIDs = LinearIDs.ToArray();
            return _info;
        }
    }
}
