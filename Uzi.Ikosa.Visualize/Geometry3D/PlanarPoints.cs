using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    /// <summary>Represents an array of Point3D defining a plane.</summary>
    public struct PlanarPoints : IEnumerable<Point3D>
    {
        /// <summary>Represents an array of Point3D defining a plane.</summary>
        public PlanarPoints(params Point3D[] points)
        {
            _Points = points;

            // plane
            _Norm = Vector3D.CrossProduct((_Points[1] - _Points[0]), (_Points[1] - _Points[2]));
            _Norm.Normalize();
        }

        /// <summary>Represents an array of Point3D defining a plane.</summary>
        public PlanarPoints(Vector3D normal, params Point3D[] points)
        {
            _Points = points;

            // plane
            _Norm = normal;
        }

        #region data
        private Point3D[] _Points;
        private Vector3D _Norm;
        #endregion

        public int PointCount
            => _Points.Length;

        public Vector3D Normal => _Norm;

        public Point3D this[int index]
            => _Points[index];

        /// <summary>test is inside bounds if all normalized edge to test cross-product vectors are equal.</summary>
        public bool InsidePlanarBounds(Point3D test)
        {
            // initialize
            var _test = new Vector3D();
            var _len = PointCount;
            for (int _bx = 0; _bx < _len; _bx++)
            {
                // get vector to test point from edge point
                var _bearing = (_Points[_bx] - test);

                // get vector to next edge point (allow wrapping to start edge point)
                var _edge = _Points[_bx] - _Points[(_bx < (_len - 1)) ? _bx + 1 : 0];

                // calculate the cross product (normalized)
                var _norm = Vector3D.CrossProduct(_edge, _bearing);
                _norm.Normalize();

                if (_bx == 0)
                {
                    // capture first normal vector
                    _test = _norm;
                }
                else if (_test != _norm)
                {
                    _bearing.Normalize();
                    _edge.Normalize();
                    if ((_bearing != _edge) 
                        || (_bearing.Length > _edge.Length))    // ???
                    {
                        // exit if comparison failure on subsequent vectors
                        return false;
                    }
                }
            }

            // all processed successfully
            return true;
        }

        /// <summary>
        /// Returns Point3D representing the intersection of the segment between two points with the plane defined by the planarBounds.
        /// If the segment is parallel, does not penetrate the plane, or does not penetrate the plane within the bounds, a NULL point is returned.
        /// </summary>
        public Point3D? SegmentIntersection(Point3D pt1, Point3D pt2)
        {
            // segment
            var _segment = (pt2 - pt1);

            // segment-planar dot
            var _planeDotSegment = Vector3D.DotProduct(_Norm, _segment);
            if (_planeDotSegment == 0)
            {
                // parallel
                return null;
            }
            else
            {
                // fractional distance along segment of intersect point
                var _fraction = Vector3D.DotProduct(_Norm, (_Points[0] - pt1)) / _planeDotSegment;
                if ((_fraction < 0) || (_fraction > 1))
                {
                    // outside the segment
                    return null;
                }
                else
                {
                    // planar intersect point
                    var _iPt = pt1 + (_segment * _fraction);
                    if (!InsidePlanarBounds(_iPt))
                    {
                        // outside the bounds
                        return null;
                    }
                    else
                    {
                        // intersection
                        return _iPt;
                    }
                }
            }
        }

        /// <summary>transforms planar points</summary>
        public void Transform(Transform3D transform)
        {
            for (var _px = 0; _px < _Points.Length; _px++)
            {
                _Points[_px] = transform?.Transform(_Points[_px]) ?? _Points[_px];
            }

            // plane
            _Norm = Vector3D.CrossProduct((_Points[1] - _Points[0]), (_Points[1] - _Points[2]));
            _Norm.Normalize();
        }

        #region IEnumerable<Point3D> Members

        public IEnumerator<Point3D> GetEnumerator()
        {
            foreach (var _p in _Points)
            {
                yield return _p;
            }

            yield break;
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (var _p in _Points)
            {
                yield return _p;
            }

            yield break;
        }

        #endregion
    }
}
