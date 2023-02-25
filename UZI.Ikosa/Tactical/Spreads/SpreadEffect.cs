using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Movement;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class SpreadEffect : ISourcedObject
    {
        public SpreadEffect(MapContext mapContext, object source, Intersection origin,
            CellPosition center, int cellRadius, ISpreadCapable spread, PlanarPresence planar)
        {
            // NOTE: maxRange from IRegionCapable (Dimension)
            // NOTE: origin from delivery...

            _MapContext = mapContext;
            _Origin = origin;
            _Center = center;
            _Source = source;
            _Spread = spread;

            _Radius = cellRadius;
            _Extent = cellRadius * 2;
            _Planar = planar;

            // map translation values
            _ZBase = origin.Z - _Radius;
            _YBase = origin.Y - _Radius;
            _XBase = origin.X - _Radius;

            // processing window setup
            _ZProcMin = _Radius - 1;
            _YProcMin = _Radius - 1;
            _XProcMin = _Radius - 1;
            _ZProcMax = _Radius;
            _YProcMax = _Radius;
            _XProcMax = _Radius;

            // max cell extent
            _Cells = CubicArray<byte>(_Extent, _Extent, _Extent);
        }

        #region state
        protected MapContext _MapContext;

        protected object _Source;

        /// <summary>Geometric origin</summary>
        protected IGeometryAnchorSupplier _Origin;

        /// <summary>Cell in which the spread definitely occupies</summary>
        protected CellPosition _Center;

        protected ISpreadCapable _Spread;

        protected readonly int _Radius;
        protected readonly int _Extent;
        protected readonly PlanarPresence _Planar;

        // base of array in map cell coordinates
        protected readonly int _ZBase;
        protected readonly int _YBase;
        protected readonly int _XBase;

        // processing window mins and maxes per dimension
        protected readonly int _ZProcMin;
        protected readonly int _YProcMin;
        protected readonly int _XProcMin;
        protected readonly int _ZProcMax;
        protected readonly int _YProcMax;
        protected readonly int _XProcMax;

        /// <summary>innermost run represents X range</summary>
        protected byte[][][] _Cells;

        //protected ThreeDimensionalBitArray _Actual;     // actual that are used

        // TODO: capacity-left/distance-spread
        // TODO: track creatures/objects effected (prevent double-hit)
        #endregion

        #region CubicArray initializer
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
        #endregion

        public MapContext MapContext => _MapContext;
        public IGeometryAnchorSupplier Origin => _Origin;
        public object Source => _Source;
        public ISpreadCapable SpreadCapable => _Spread;

        public int CellRadius => _Radius;
        public PlanarPresence PlanarPresence => _Planar;

        public Cubic GetMaxCubic()
        {
            var _offset = new CellPosition(CellRadius, CellRadius, CellRadius);
            return new CubicBuilder(new GeometricSize(_Extent, _Extent, _Extent), _offset)
                .BuildGeometry(LocationAimMode.Intersection, Origin.Location) as Cubic;
        }

        // TODO: get/set _Cell values

        // spread track array accessors
        private byte GetCellValue(CellPosition spreadCell)
            => _Cells[spreadCell.Z - _ZBase][spreadCell.Y - _YBase][spreadCell.X - _XBase];

        private void SetCellValue(CellPosition spreadCell, byte newValue)
            => _Cells[spreadCell.Z - _ZBase][spreadCell.Y - _YBase][spreadCell.X - _XBase] = newValue;

        // translate 

        // map coordinate translator
        private CellPosition GetMapCell(int z, int y, int x)
            => new CellPosition(_ZBase + z, _YBase + y, _XBase + x);

        private bool CanSpreadInto(ConcurrentDictionary<Guid, Guid> affected, CellPosition sourceCell, CellPosition targetCell, AnchorFace sourceFace, HedralGrip sourceOpen, bool canAlter)
        {
            var _targetFace = sourceFace.ReverseFace();
            var _targetStruc = MapContext.Map[targetCell];

            // will terrain allow passage?
            var _canEnter = sourceOpen.Intersect(
                _targetStruc.HedralGripping(LinkMovement.Static, _targetFace).Invert()).HasAny;
            if (_canEnter)
            {
                // can the spread affect object blockers?
                if (canAlter)
                {
                    // potential barriers to entrance
                    var _barriers = (from _l in MapContext.LocatorsInCell(targetCell, _Planar)
                                     let _tact = _l.ICore as ITacticalInquiry
                                     where _tact?.CanBlockSpread ?? false
                                     select new
                                     {
                                         Locator = _l,
                                         Tactical = _tact
                                     })
                                     .ToList();
                    foreach (var _obstruction in _barriers)
                    {
                        // must not have been affected yet, and must block spread
                        if (!affected.ContainsKey(_obstruction.Tactical.ID)
                            && _obstruction.Tactical.BlocksSpread(_targetFace, sourceCell, targetCell))
                        {
                            // track and apply, only applies if this call did an add
                            if (affected.TryAdd(_obstruction.Tactical.ID, _obstruction.Tactical.ID))
                            {
                                _Spread.ApplySpreadToBlocker(this, _obstruction.Locator);
                            }
                        }
                    }
                }

                // get blockers
                var _blockers = (from _l in MapContext.LocatorsInCell(targetCell, _Planar)
                                 let _tact = _l.ICore as ITacticalInquiry
                                 where _tact?.CanBlockSpread ?? false
                                 select new
                                 {
                                     Locator = _l,
                                     Tactical = _tact
                                 })
                                 .ToList();

                // can enter must not have any blocker
                _canEnter &= !_blockers.Any(_blk => _blk.Tactical.BlocksSpread(_targetFace, sourceCell, targetCell));

                if (_canEnter && canAlter)
                {
                    // apply to any tactical that may prevent further spreading
                    foreach (var _blk in _blockers)
                    {
                        if (!affected.ContainsKey(_blk.Tactical.ID))
                        {
                            // track and apply, only applies if this call did an add
                            if (affected.TryAdd(_blk.Tactical.ID, _blk.Tactical.ID))
                            {
                                _Spread.ApplySpreadToBlocker(this, _blk.Locator);
                            }
                        }
                    }
                }
            }
            return _canEnter;
        }

        private void SeedCells()
        {
            // seed starts at an intersection, and tries to treat connected 8 cells as equally as they are connectable
            // all subsequent processing is center to center, so doesn't need as much finesse as the seed step, 
            // and can assume that ApplySpreadToBlockers has already been called for each processed cell

            // calculate seed value for Radius
            var _seedValue = (byte)((CellRadius << 1) - 1);
            SetCellValue(_Center, _seedValue);

            var _centerStruc = MapContext.Map[_Center];

            // get blocking to next cells
            var (_outZ, _outY, _outX) = (
                _Center.Z == _Origin.Location.Z ? AnchorFace.ZLow : AnchorFace.ZHigh,
                _Center.Y == _Origin.Location.Y ? AnchorFace.YLow : AnchorFace.YHigh,
                _Center.X == _Origin.Location.X ? AnchorFace.ZLow : AnchorFace.XHigh);

            var (_canTransZ, _canTransY, _canTransX) = (true, true, true);
            var _canAlter = _Spread.CanAlterBlockers;
            if (_canAlter)
            {
                // potential blockers in cell
                var _locs = MapContext.LocatorsInCell(_Center, _Planar)
                    .Where(_l => (_l.ICore is ITacticalInquiry _tact) && _tact.CanBlockSpread)
                    .ToList();
                foreach (var _l in _locs)
                {
                    _Spread.ApplySpreadToBlocker(this, _l);
                }

                // get remaining tacticals
                var _tacticals = MapContext.LocatorsInCell(_Center, _Planar)
                    .Where(_l => (_l.ICore is ITacticalInquiry _tact) && _tact.CanBlockSpread)
                    .Select(_l => _l.ICore as ITacticalInquiry)
                    .ToList();

                // can exit must not have any blocker
                _canTransZ &= !_tacticals.Any(_t => _t.BlocksSpread(_outZ, _Center, _Center.Add(_outZ)));
                _canTransY &= !_tacticals.Any(_t => _t.BlocksSpread(_outY, _Center, _Center.Add(_outY)));
                _canTransX &= !_tacticals.Any(_t => _t.BlocksSpread(_outX, _Center, _Center.Add(_outX)));
            }

            // if exit possible, calculate structural allowance (inversion of grip)
            var _zOutOpen = _canTransZ ? _centerStruc.HedralGripping(LinkMovement.Static, _outZ).Invert() : new HedralGrip(false);
            var _yOutOpen = _canTransY ? _centerStruc.HedralGripping(LinkMovement.Static, _outY).Invert() : new HedralGrip(false);
            var _xOutOpen = _canTransX ? _centerStruc.HedralGripping(LinkMovement.Static, _outX).Invert() : new HedralGrip(false);

            // final exits
            _canTransZ &= _zOutOpen.HasAny;
            _canTransY &= _yOutOpen.HasAny;
            _canTransX &= _xOutOpen.HasAny;

            // entrances
            var _affected = new ConcurrentDictionary<Guid, Guid>();
            var _zCell = _Center.Add(_outZ);
            var _yCell = _Center.Add(_outY);
            var _xCell = _Center.Add(_outX);
            _canTransZ &= CanSpreadInto(_affected, _Center, _zCell, _outZ, _zOutOpen, _canAlter);
            _canTransY &= CanSpreadInto(_affected, _Center, _yCell, _outY, _yOutOpen, _canAlter);
            _canTransX &= CanSpreadInto(_affected, _Center, _xCell, _outX, _xOutOpen, _canAlter);

            // seed cells
            if (_canTransZ)
            {
                SetCellValue(_zCell, _seedValue);
            }
            if (_canTransY)
            {
                SetCellValue(_yCell, _seedValue);
            }
            if (_canTransX)
            {
                SetCellValue(_xCell, _seedValue);
            }

            // TODO: next ¿(3)? checks (number dependent on previous 3 checks that succeeded
            // TODO: final counter cell (depending on intermediate 3 cell check)
        }

        // TODO: start processing spread
        // TODO: seed initial 8 cells as diagonals from origin (if possible) recalc processing window as necessary
        // TODO: parallelize each subsequent step...
    }
}
