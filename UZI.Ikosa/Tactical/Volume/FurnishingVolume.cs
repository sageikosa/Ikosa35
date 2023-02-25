using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Objects;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class FurnishingVolume
    {
        public FurnishingVolume(Furnishing furnishing)
        {
            _Furnishing = furnishing;
            _Movement = ObjectStaticMovement.Static;
            _Gravity = _Furnishing.GetLocated()?.Locator?.GetGravityFace() ?? AnchorFace.ZLow;
        }

        #region data
        private Furnishing _Furnishing;
        private ObjectStaticMovement _Movement;
        private AnchorFace _Gravity;
        #endregion

        public Furnishing Furnishing => _Furnishing;

        private bool CanOccupy(LocalMap map, IGeometricRegion region, PlanarPresence planar)
            => map.CanOccupy(_Furnishing, region, ObjectStaticMovement.Static,
                Furnishing.ToEnumerable().ToDictionary(_f => _f.ID, _f => (ICore)_f), planar);

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
                foreach (var _iti in map.MapContext.AllInCell<IMoveAlterer>(_cLoc, planar)
                    .Where(_ima => _ima != Furnishing))
                {
                    if (_iti.HindersTransit(_Movement, _rgn))
                    {
                        foreach (var _opening in _iti.OpensTowards(_Movement, _cLoc, _Gravity, Furnishing)
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

        #region public (Cubic Cube, Vector3D Offset) GetCubicFit(Cubic region, IGeometricSize size, bool zHigh, bool yHigh, bool xHigh)
        public (Cubic Cube, Vector3D Offset) GetCubicFit(Cubic region, IGeometricSize size, bool zHigh, bool yHigh, bool xHigh,
            PlanarPresence planar)
        {
            if (region != null)
            {
                // get location
                int _alterOrd(int ord, long currExt, long newExt, bool highSnap)
                {
                    if (highSnap)
                    {
                        if (currExt < newExt)
                            return ord - 1;
                        if (currExt > newExt)
                            return ord + 1;
                    }
                    return ord;
                };
                var _location = region.SameSize(size)
                    ? region as ICellLocation
                    : new CellPosition(
                        _alterOrd(region.Z, region.ZHeight, size.ZHeight, zHigh),
                        _alterOrd(region.Y, region.YLength, size.YLength, yHigh),
                        _alterOrd(region.X, region.XLength, size.XLength, xHigh));

                // build cube to try
                var _cube = new Cubic(_location, size);
                if ((Furnishing?.Setting is LocalMap _map) && (_cube != null))
                {
                    var _gravity = Furnishing.ObjectPresenter.GetGravityFace();
                    var _planar = Furnishing.ObjectPresenter.PlanarPresence;
                    var _extents = Furnishing.Orientation.SnappableExtents;
                    var _size = Furnishing.Orientation.SnappableSize;

                    // expected offsets (with sign for intra model offset)
                    var _zOffset = (Furnishing.Orientation.ZHighSnap)
                        ? Offset(_map, _cube.EdgeCubic(AnchorFace.ZHigh), _gravity, AnchorFace.ZLow, planar)
                        : Offset(_map, _cube.EdgeCubic(AnchorFace.ZLow), _gravity, AnchorFace.ZHigh, planar);

                    var _yOffset = (Furnishing.Orientation.YHighSnap)
                        ? Offset(_map, _cube.EdgeCubic(AnchorFace.YHigh), _gravity, AnchorFace.YLow, planar)
                        : Offset(_map, _cube.EdgeCubic(AnchorFace.YLow), _gravity, AnchorFace.YHigh, planar);

                    var _xOffset = (Furnishing.Orientation.XHighSnap)
                        ? Offset(_map, _cube.EdgeCubic(AnchorFace.XHigh), _gravity, AnchorFace.XLow, planar)
                        : Offset(_map, _cube.EdgeCubic(AnchorFace.XLow), _gravity, AnchorFace.XHigh, planar);

                    // do offsets force size overruns?
                    var _offExtents = new Vector3D(
                        _extents.X + Math.Abs(_xOffset),
                        _extents.Y + Math.Abs(_yOffset),
                        _extents.Z + Math.Abs(_zOffset));
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
                        if (Furnishing.Orientation.ZHighSnap && (_offSize.ZHeight != _size.ZHeight))
                            _adjFaces = _adjFaces.Add(AnchorFace.ZLow);
                        if (Furnishing.Orientation.YHighSnap && (_offSize.YLength != _size.YLength))
                            _adjFaces = _adjFaces.Add(AnchorFace.YLow);
                        if (Furnishing.Orientation.XHighSnap && (_offSize.XLength != _size.XLength))
                            _adjFaces = _adjFaces.Add(AnchorFace.XLow);
                        _candidate = new Cubic(_cube.Add(_adjFaces), _offSize);
                    }

                    // check occupation of fixed up candidate
                    if (!CanOccupy(_map, _candidate, _planar))
                        return (null, default);

                    // offsets from the far side...
                    var _zReverse = (Furnishing.Orientation.ZHighSnap)
                        ? Offset(_map, _candidate.EdgeCubic(AnchorFace.ZLow), _gravity, AnchorFace.ZHigh, planar)
                        : Math.Abs(Offset(_map, _candidate.EdgeCubic(AnchorFace.ZHigh), _gravity, AnchorFace.ZLow, planar));

                    var _yReverse = (Furnishing.Orientation.YHighSnap)
                        ? Offset(_map, _candidate.EdgeCubic(AnchorFace.YLow), _gravity, AnchorFace.YHigh, planar)
                        : Math.Abs(Offset(_map, _candidate.EdgeCubic(AnchorFace.YHigh), _gravity, AnchorFace.YLow, planar));

                    var _xReverse = (Furnishing.Orientation.XHighSnap)
                        ? Offset(_map, _candidate.EdgeCubic(AnchorFace.XLow), _gravity, AnchorFace.XHigh, planar)
                        : Math.Abs(Offset(_map, _candidate.EdgeCubic(AnchorFace.XHigh), _gravity, AnchorFace.XLow, planar));

                    // is the (resized) candidate incapable of fitting within?
                    // TODO: allow a little bit of swag on extents
                    if (((_offExtents.Z + _zReverse) > (_candidate.ZHeight * 5d))
                        || ((_offExtents.Y + _yReverse) > (_candidate.YLength * 5d))
                        || ((_offExtents.X + _xReverse) > (_candidate.XLength * 5d)))
                        return (null, default);

                    // _candidate*5 - (_reverse + Abs(_offset))
                    return (_candidate, new Vector3D(_xOffset, _yOffset, _zOffset));
                }
            }
            return (null, default);
        }
        #endregion
    }
}
