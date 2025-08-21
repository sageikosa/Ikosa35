using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Adjuncts;
using Uzi.Core;
using Uzi.Visualize;
using Uzi.Ikosa.Tactical;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Movement;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class PointsHandler : IInteractHandler
    {
        private static PointsHandler _Static = new PointsHandler();
        public static PointsHandler Static { get { return _Static; } }

        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
            if (workSet != null)
            {
                var _faces = new List<AnchorFace>();
                void _addFace(int src, int trg, AnchorFace face)
                {
                    if (src > trg)
                    {
                        _faces.Add(face);
                    }
                    else if (src < trg)
                    {
                        _faces.Add(face.ReverseFace());
                    }
                }
                ;
                if (workSet.InteractData is GetMeleeSourcePoints _msp)
                {
                    #region melee source points
                    // axis/face selection
                    _addFace(_msp.SourceCell.X, _msp.TargetCell.X, AnchorFace.XLow);
                    _addFace(_msp.SourceCell.Y, _msp.TargetCell.Y, AnchorFace.YLow);
                    _addFace(_msp.SourceCell.Z, _msp.TargetCell.Z, AnchorFace.ZLow);

                    if (workSet.Target is IAdjunctSet _aSet)
                    {
                        // NOTE: this is always the attack source
                        var _located = _aSet.GetLocated();
                        if (_located != null)
                        {
                            var _locator = _located.Locator;

                            var _grav = _locator.GetGravityFace();
                            var _move = _locator.ActiveMovement;

                            // NOTE: determine whether its a level attack before culling supported gravity faces
                            var _levelAtk = !_faces.Contains(_grav) && !_faces.Contains(_grav.ReverseFace());

                            // is the down gravity face one of the faces (but not the only one?)
                            var _downWard = false;
                            if (_faces.Contains(_grav) && (_faces.Count > 1))
                            {
                                // if supported, remove gravity face from list
                                if (_locator.Map.DirectionalBlockage(_locator.ICore as ICoreObject, 
                                    new CellLocation(_msp.SourceCell), _move, _grav, _grav, null, _locator.PlanarPresence) > 0)
                                {
                                    // also, melee target points will need to ignore under edge points
                                    _faces.Remove(_grav);
                                    _downWard = true;
                                }
                            }

                            // collect side columns
                            var _columns = new List<SideColumn>();
                            foreach (var _corner in _locator.Map[_msp.SourceCell].TargetCorners(_move))
                            {
                                // only points that are on desired surfaces
                                if (_corner.Faces.Any(_f => _faces.Contains(_f)))
                                {
                                    // try to add to an existing column
                                    var _added = false;
                                    foreach (var _sideCol in _columns.Where(_c => !_c.IsFull))
                                    {
                                        _added = _sideCol.Adding(_corner);
                                        if (_added)
                                        {
                                            break;
                                        }
                                    }

                                    // must add a new column
                                    if (!_added)
                                    {
                                        _columns.Add(new SideColumn(_corner, _grav));
                                    }
                                }
                            }

                            // offset vector
                            var _offPt = _msp.SourceCell.Point3D();
                            var _offset = new Vector3D(_offPt.X, _offPt.Y, _offPt.Z);

                            if (_levelAtk || _downWard)
                            {
                                // 5% from top, and 30% from bottom
                                workSet.Feedback.Add(new GetPointsFeedback(this,
                                    _columns.SelectMany(_sc => _sc.FinalCorners(0.95d, 0.3d).Select(_p => _p + _offset)).ToList(),
                                    _downWard, _grav));
                            }
                            else
                            {
                                workSet.Feedback.Add(new GetPointsFeedback(this,
                                    _columns.SelectMany(_sc => _sc.FinalCorners().Select(_p => _p + _offset)).ToList(),
                                    _downWard, _grav));
                            }
                        }
                    }
                    #endregion
                }
                else if (workSet.InteractData is GetMeleeTargetPoints _mtp)
                {
                    #region melee target points
                    // axis/face selection (inversion of melee source points)
                    _addFace(_mtp.SourceCell.X, _mtp.TargetCell.X, AnchorFace.XHigh);
                    _addFace(_mtp.SourceCell.Y, _mtp.TargetCell.Y, AnchorFace.YHigh);
                    _addFace(_mtp.SourceCell.Z, _mtp.TargetCell.Z, AnchorFace.ZHigh);

                    // ensure we have suitable target context
                    if (workSet.Target is IAdjunctSet _aSet)
                    {
                        // NOTE: this could be the target, or the actor is no target provided
                        var _located = _aSet.GetLocated();
                        if (_located != null)
                        {
                            var _locator = _located.Locator;
                            var _move = _locator.ActiveMovement;
                            var _grav = _locator.GetGravityFace();

                            // the keeper face is the reverse of the attacker's gravity face
                            var _keepFace = _mtp.DownFace.ReverseFace();
                            var _keepUnderEdge = true;

                            // if the attacker was striking "downward", prepare to filter under edge points
                            if (_mtp.DownWard && _faces.Contains(_keepFace))
                            {
                                // only two faces, so keep only one
                                if (_faces.Count == 2)
                                {
                                    // since only had two faces, only the keeper one is good
                                    _faces = _faces.Where(_f => _f == _keepFace).ToList();
                                }
                                else
                                {
                                    // remove any corner having two faces, when one is not reverse gravity's face
                                    // NOTE: will still keep corners that only have one matching face
                                    _keepUnderEdge = false;
                                }
                            }

                            // TODO: check terrain visualization in target cell, may need regular cell corners instead

                            // offset vector
                            var _offPt = _mtp.TargetCell.Point3D();
                            var _offset = new Vector3D(_offPt.X, _offPt.Y, _offPt.Z);

                            // collect output points
                            var _outPoints = new List<Point3D>();
                            foreach (var _corner in _locator.Map[_mtp.TargetCell].TargetCorners(_move))
                            {
                                // only points that are on desired surfaces
                                var _matchFaces = _corner.Faces.Intersect(_faces).ToList();
                                if (_matchFaces.Any())
                                {
                                    // keeping under edge, not specifically two faces, or has keeper face
                                    if (_keepUnderEdge || (_matchFaces.Count != 2) || _matchFaces.Contains(_keepFace))
                                    {
                                        _outPoints.Add(WarpPoint(_corner.Point3D) + _offset);
                                    }
                                }
                            }

                            // feedback
                            workSet.Feedback.Add(new GetPointsFeedback(this, _outPoints, _faces.Contains(_grav.ReverseFace()), _grav));
                        }
                    }
                    #endregion
                }
                else if (workSet.InteractData is GetRangedTargetPoints _rtp)
                {
                    #region ranged target points
                    if (workSet.Target is IAdjunctSet _aSet)
                    {
                        // TODO: make min target corners body-dependent
                        var _minBody = 1.5d;
                        if (_aSet is Creature)
                        {
                            _minBody = (_aSet as Creature).Body.Height * 0.3d;
                        }

                        var _located = _aSet.GetLocated();
                        if (_located != null)
                        {
                            var _locator = _located.Locator;
                            var _move = _locator.ActiveMovement;
                            var _region = _locator.GeometricRegion;
                            var _center = _region.GetPoint3D();
                            var _map = _locator.Map;

                            // which set of cells gets special sideColumn treatment...
                            #region sideColum support
                            var _bottom = 0;
                            var _grav = _locator.GetGravityFace();
                            switch (_grav)
                            {
                                case AnchorFace.ZHigh: _bottom = _region.UpperZ; break;
                                case AnchorFace.YHigh: _bottom = _region.UpperY; break;
                                case AnchorFace.XHigh: _bottom = _region.UpperX; break;
                                case AnchorFace.ZLow: _bottom = _region.LowerZ; break;
                                case AnchorFace.YLow: _bottom = _region.LowerY; break;
                                case AnchorFace.XLow: _bottom = _region.LowerX; break;
                            }
                            bool _usesColumns(ICellLocation cell)
                            {
                                switch (_grav)
                                {
                                    case AnchorFace.ZHigh:
                                    case AnchorFace.ZLow:
                                        return _bottom == cell.Z;
                                    case AnchorFace.YHigh:
                                    case AnchorFace.YLow:
                                        return _bottom == cell.Y;
                                    case AnchorFace.XHigh:
                                    case AnchorFace.XLow:
                                        return _bottom == cell.X;
                                }
                                return false;
                            };
                            #endregion

                            // collect output points
                            var _outPoints = new List<Point3D>();

                            // each cell may have different face requirements
                            foreach (var _cell in _region.AllCellLocations())
                            {
                                // axis/face selection (inversion of melee source points)
                                _faces = [];
                                _addFace(_rtp.SourceCell.X, _cell.X, AnchorFace.XHigh);
                                _addFace(_rtp.SourceCell.Y, _cell.Y, AnchorFace.YHigh);
                                _addFace(_rtp.SourceCell.Z, _cell.Z, AnchorFace.ZHigh);

                                // check if its an upward attack (has gravity face pointing at source)
                                var _upwardAtk = _faces.Contains(_grav);

                                // this cell must be unbound on one of the filtering faces for the cell
                                // ... this prevents occluded cells in larger sets from being considered
                                if (_faces.Any(_f => _region.IsCellUnboundAtFace(_cell, _f)))
                                {
                                    // offset vector
                                    var _offPt = _cell.Point3D();
                                    var _offset = new Vector3D(_offPt.X, _offPt.Y, _offPt.Z);

                                    // corners
                                    var _columns = new List<SideColumn>();
                                    foreach (var _corner in _map[_cell].TargetCorners(_move))
                                    {
                                        // point must be on a desired surface that is not occluded
                                        if (_corner.Faces.Any(_f => _faces.Contains(_f)
                                            && _region.IsCellUnboundAtFace(_cell, _f)))
                                        {
                                            // try to add to an existing column
                                            var _added = false;
                                            foreach (var _sideCol in _columns.Where(_c => !_c.IsFull))
                                            {
                                                _added = _sideCol.Adding(_corner);
                                                if (_added)
                                                {
                                                    break;
                                                }
                                            }

                                            // must add a new column
                                            if (!_added)
                                            {
                                                _columns.Add(new SideColumn(_corner, _grav));
                                            }
                                        }
                                    }

                                    if (!_upwardAtk && _usesColumns(_cell))
                                    {
                                        // adjust bottom point if possible
                                        foreach (var _pt in from _col in _columns
                                                            from _crn in _col.FinalCorners(_minBody)
                                                            select WarpPoint(_crn + _offset, _center))
                                        {
                                            // ... and not already contained in output set
                                            if (!_outPoints.Contains(_pt))
                                            {
                                                _outPoints.Add(_pt);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // whatever corners selected
                                        foreach (var _pt in from _col in _columns
                                                            from _crn in _col.FinalCorners()
                                                            select WarpPoint(_crn + _offset, _center))
                                        {
                                            // ... and not already contained in output set
                                            if (!_outPoints.Contains(_pt))
                                            {
                                                _outPoints.Add(_pt);
                                            }
                                        }
                                    }
                                }
                            }

                            // feedback
                            workSet.Feedback.Add(new GetPointsFeedback(this, _outPoints, _faces.Contains(_grav.ReverseFace()), _grav));
                        }
                    }
                    #endregion
                }
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(GetMeleeSourcePoints);
            yield return typeof(GetMeleeTargetPoints);
            yield return typeof(GetRangedTargetPoints);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return false;
        }

        #endregion

        private static Point3D WarpPoint(Point3D point)
        {
            var _centroid = (new Point3D(2.5d, 2.5d, 2.5d)) - point;
            _centroid.Normalize();
            return point + (_centroid * 0.0025d);
        }

        private static Point3D WarpPoint(Point3D point, Point3D center)
        {
            var _centroid = center - point;
            _centroid.Normalize();
            return point + (_centroid * 0.0025d);
        }

        #region SideColumn class
        private class SideColumn
        {
            public SideColumn(TargetCorner createWith, AnchorFace gravity)
            {
                Gravity = gravity;
                if (createWith.Faces.Contains(gravity))
                {
                    Bottom = createWith;
                }
                else
                {
                    Top = createWith;
                }
            }

            public AnchorFace Gravity { get; private set; }
            public TargetCorner Top { get; private set; }
            public TargetCorner Bottom { get; private set; }
            public bool IsFull { get { return (Top != null) && (Bottom != null); } }

            #region public bool Adding(TargetCorner test)
            public bool Adding(TargetCorner test)
            {
                // compare with the one already collected
                var _comp = Top ?? Bottom ?? null;
                if (_comp != null)
                {
                    // if all our non-gravity faces match a face in the test, we add it
                    var _gravAxis = Gravity.GetAxis();
                    if (_comp.Faces.Where(_f => _f.GetAxis() != _gravAxis)
                        .All(_f => test.Faces.Contains(_f)))
                    {
                        // add as bottom or top
                        if (test.Faces.Contains(Gravity))
                        {
                            Bottom = test;
                        }
                        else
                        {
                            Top = test;
                        }

                        return true;
                    }
                }
                return false;
            }
            #endregion

            #region public IEnumerable<Point3D> FinalCorners()
            public IEnumerable<Point3D> FinalCorners()
            {
                if (Bottom != null)
                {
                    yield return WarpPoint(Bottom.Point3D);
                }

                if (Top != null)
                {
                    yield return WarpPoint(Top.Point3D);
                }
            }
            #endregion

            #region public IEnumerable<Point3D> FinalCorners(double topFactor, double bottomFactor)
            public IEnumerable<Point3D> FinalCorners(double topFactor, double bottomFactor)
            {
                if (IsFull)
                {
                    // vector from bottom to top
                    var _vect = Top.Point3D - Bottom.Point3D;
                    yield return WarpPoint(Bottom.Point3D + (_vect * topFactor));
                    yield return WarpPoint(Bottom.Point3D + (_vect * bottomFactor));
                }
                yield break;
            }
            #endregion

            #region public IEnumerable<Point3D> FinalCorners(double bottomLift)
            /// <summary>Only called if bottom can be adjusted.</summary>
            public IEnumerable<Point3D> FinalCorners(double bottomLift)
            {
                // default empty vector
                var _vect = new Vector3D();
                if (Top != null)
                {
                    // if we have a top, adjust vector and emit point
                    if (Bottom != null)
                    {
                        _vect = (Top.Point3D - Bottom.Point3D);
                    }

                    yield return WarpPoint(Top.Point3D);
                }
                if (Bottom != null)
                {
                    // if we have room to adjust the bottom point, do so
                    if (_vect.Length > bottomLift)
                    {
                        // adjust along the column's vector
                        _vect.Normalize();

                        // scaled to the lift amount
                        yield return WarpPoint(Bottom.Point3D + (_vect * bottomLift));
                    }
                    // ELSE, do not return bottom
                }
                yield break;
            }
            #endregion
        }
        #endregion
    }
}
