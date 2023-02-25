using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Objects;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class ConveyanceVolume
    {
        public ConveyanceVolume(Conveyance conveyance)
        {
            _Conveyance = conveyance;
            _Movement = ObjectStaticMovement.Static;
            _Gravity = _Conveyance.GetLocated()?.Locator?.GetGravityFace() ?? AnchorFace.ZLow;
        }

        #region data
        private Conveyance _Conveyance;
        private ObjectStaticMovement _Movement;
        private AnchorFace _Gravity;
        #endregion

        public Conveyance Conveyance => _Conveyance;

        private bool CanOccupy(LocalMap map, IGeometricRegion region, PlanarPresence planar)
            => map.CanOccupy(_Conveyance, region, ObjectStaticMovement.Static,
                Conveyance.ToEnumerable().ToDictionary(_f => _f.ID, _f => (ICore)_f), planar);

        #region private double Offset(LocalMap map, Cubic edgeCubic, AnchorFace gravity, AnchorFace reverse)
        private double Offset(LocalMap map, Cubic edgeCubic, AnchorFace gravity, AnchorFace reverse, PlanarPresence planar)
        {
            // all cell structure that affects offset
            var _offset = 0d;
            foreach (var _struct in edgeCubic.AllCellLocations()
                .Select(_cLoc => map[_cLoc]))
            {
                var _vec = _struct.OrthoOffset(_Movement, gravity);
                switch (reverse)
                {
                    case AnchorFace.ZLow: _offset = (_vec.Z < _offset) ? _offset = _vec.Z : _offset; break;
                    case AnchorFace.YLow: _offset = (_vec.Y < _offset) ? _offset = _vec.Y : _offset; break;
                    case AnchorFace.XLow: _offset = (_vec.X < _offset) ? _offset = _vec.X : _offset; break;
                    case AnchorFace.ZHigh: _offset = (_vec.Z > _offset) ? _offset = _vec.Z : _offset; break;
                    case AnchorFace.YHigh: _offset = (_vec.Y > _offset) ? _offset = _vec.Y : _offset; break;
                    case AnchorFace.XHigh: _offset = (_vec.X > _offset) ? _offset = _vec.X : _offset; break;
                    default:
                        break;
                }
            }

            // check object blockings
            var _amount = new List<double>();

            // object blockages in edge (excluding furnishing in question!)
            foreach (var _cLoc in edgeCubic.AllCellLocations())
            {
                var _rgn = new CellLocation(_cLoc);
                // TODO: conveyance may be conveying something, which also needs to be ignored...
                foreach (var _iti in map.MapContext.AllInCell<IMoveAlterer>(_cLoc, planar)
                    .Where(_ima => _ima != Conveyance))
                {
                    if (_iti.HindersTransit(_Movement, _rgn))
                    {
                        foreach (var _opening in _iti.OpensTowards(_Movement, _cLoc, _Gravity, Conveyance)
                            .Where(_o => _o.Face == reverse))
                        {
                            _amount.Add(5d - _opening.Amount);
                        }
                    }
                }
            }

            // offset is the difference from standard cell dimension
            var _max = (_amount.Any() ? _amount.Max() : 0);
            if (reverse.IsLowFace())
            {
                // min
                var _min = 0 - _max;
                return Math.Min(_offset, _min);
            }

            // max
            return Math.Max(_offset, _max);
        }
        #endregion

        #region public (Cubic Cube, Vector3D Offset) GetCubicFit(Cubic region, IGeometricSize size)
        public (Cubic Cube, Vector3D Offset) GetCubicFit(Cubic region, IGeometricSize size)
        {
            if (region != null)
            {
                var _location = region.SameSize(size)
                    ? region as ICellLocation
                    : new CellPosition(region.Z, region.Y, region.X);

                // build cube to try
                var _cube = new Cubic(_location, size);
                if ((Conveyance?.Setting is LocalMap _map) && (_cube != null))
                {
                    var _gravity = Conveyance.ObjectPresenter.GetGravityFace();
                    var _planar = Conveyance.ObjectPresenter.PlanarPresence;
                    var _extents = Conveyance.Orientation.SnappableExtents;
                    var _size = Conveyance.Orientation.SnappableSize;

                    // expected gravity offsets (with sign for intra model offset)
                    var _zOffLo = Offset(_map, _cube.EdgeCubic(AnchorFace.ZLow), _gravity, AnchorFace.ZHigh, _planar);
                    var _zOffHi = Offset(_map, _cube.EdgeCubic(AnchorFace.ZHigh), _gravity, AnchorFace.ZLow, _planar);
                    var _yOffLo = Offset(_map, _cube.EdgeCubic(AnchorFace.YLow), _gravity, AnchorFace.YHigh, _planar);
                    var _yOffHi = Offset(_map, _cube.EdgeCubic(AnchorFace.YHigh), _gravity, AnchorFace.YLow, _planar);
                    var _xOffLo = Offset(_map, _cube.EdgeCubic(AnchorFace.XLow), _gravity, AnchorFace.XHigh, _planar);
                    var _xOffHi = Offset(_map, _cube.EdgeCubic(AnchorFace.XHigh), _gravity, AnchorFace.XLow, _planar);

                    // determine inferred snappage
                    bool _isOffset(AnchorFace face)
                    {
                        switch (face)
                        {
                            case AnchorFace.XLow: return _xOffLo < 0;
                            case AnchorFace.YLow: return _yOffLo < 0;
                            case AnchorFace.ZLow: return _zOffLo < 0;
                            case AnchorFace.XHigh: return _xOffHi > 0;
                            case AnchorFace.YHigh: return _yOffHi > 0;
                            case AnchorFace.ZHigh: return _zOffHi > 0;
                            default: return _zOffLo < 0;
                        }
                    }

                    // if snapped across both sides of an axis and not enough room, NULL
                    var _xLoOff = _isOffset(AnchorFace.XLow);
                    var _xHiOff = _isOffset(AnchorFace.XHigh);
                    if (_xLoOff && _xHiOff && (_extents.X > ((_cube.XLength * 5) - Math.Abs(_xOffLo) - Math.Abs(_xOffHi))))
                    {
                        return (null, default);
                    }
                    var _yLoOff = _isOffset(AnchorFace.YLow);
                    var _yHiOff = _isOffset(AnchorFace.YHigh);
                    if (_yLoOff && _yHiOff && (_extents.Y > ((_cube.YLength * 5) - Math.Abs(_yOffLo) - Math.Abs(_yOffHi))))
                    {
                        return (null, default);
                    }
                    var _zLoOff = _isOffset(AnchorFace.ZLow);
                    var _zHiOff = _isOffset(AnchorFace.ZHigh);
                    if (_zLoOff && _zHiOff && (_extents.Z > ((_cube.ZHeight * 5) - Math.Abs(_zOffLo) - Math.Abs(_zOffHi))))
                    {
                        return (null, default);
                    }

                    // do offsets force size overruns?
                    var _offExtents = new Vector3D(
                        _extents.X + Math.Abs(_xOffLo) + Math.Abs(_xOffHi),
                        _extents.Y + Math.Abs(_yOffLo) + Math.Abs(_yOffHi),
                        _extents.Z + Math.Abs(_zOffLo) + Math.Abs(_zOffHi));
                    var _offSize = new GeometricSize(
                        Math.Ceiling(_offExtents.Z / 5d),
                        Math.Ceiling(_offExtents.Y / 5d),
                        Math.Ceiling(_offExtents.X / 5d));

                    // TODO: allow a little bit of swag on extents

                    var _candidate = _cube;
                    if (!_offSize.SameSize(_size))
                    {
                        // adjust position of cubic if offset away from high faces
                        var _adjFaces = AnchorFaceList.None;
                        if (_zHiOff && !_zLoOff && (_offSize.ZHeight != _size.ZHeight))
                            _adjFaces = _adjFaces.Add(AnchorFace.ZLow);
                        if (_yHiOff && !_yLoOff && (_offSize.YLength != _size.YLength))
                            _adjFaces = _adjFaces.Add(AnchorFace.YLow);
                        if (_xHiOff && !_xLoOff && (_offSize.XLength != _size.XLength))
                            _adjFaces = _adjFaces.Add(AnchorFace.XLow);
                        _candidate = new Cubic(_cube.Add(_adjFaces), _offSize);

                        // check occupation of fixed up candidate
                        if (!CanOccupy(_map, _candidate, _planar))
                            return (null, default);

                        // offsets from the far side (gravity axis + implied snap [if any])...
                        _zOffLo = Offset(_map, _candidate.EdgeCubic(AnchorFace.ZLow), _gravity, AnchorFace.ZHigh, _planar);
                        _zOffHi = Offset(_map, _candidate.EdgeCubic(AnchorFace.ZHigh), _gravity, AnchorFace.ZLow, _planar);
                        _yOffLo = Offset(_map, _candidate.EdgeCubic(AnchorFace.YLow), _gravity, AnchorFace.YHigh, _planar);
                        _yOffHi = Offset(_map, _candidate.EdgeCubic(AnchorFace.YHigh), _gravity, AnchorFace.YLow, _planar);
                        _xOffLo = Offset(_map, _candidate.EdgeCubic(AnchorFace.XLow), _gravity, AnchorFace.XHigh, _planar);
                        _xOffHi = Offset(_map, _candidate.EdgeCubic(AnchorFace.XHigh), _gravity, AnchorFace.XLow, _planar);

                        // is the (resized) candidate incapable of fitting within?
                        if (_extents.X > ((_candidate.XLength * 5) - Math.Abs(_xOffLo) - Math.Abs(_xOffHi)))
                        {
                            return (null, default);
                        }
                        if (_extents.Y > ((_candidate.YLength * 5) - Math.Abs(_yOffLo) - Math.Abs(_yOffHi)))
                        {
                            return (null, default);
                        }
                        if (_extents.Z > ((_candidate.ZHeight * 5) - Math.Abs(_zOffLo) - Math.Abs(_zOffHi)))
                        {
                            return (null, default);
                        }
                    }
                    else
                    {
                        // check occupation of cube
                        if (!CanOccupy(_map, _candidate, _planar))
                            return (null, default);
                    }

                    // _candidate*5 - (_reverse + Abs(_offset))
                    return (_candidate, new Vector3D(_xOffLo + _xOffHi, _yOffLo + _yOffHi, _zOffLo + _zOffHi));
                }
            }
            return (null, default);
        }
        #endregion
    }
}
