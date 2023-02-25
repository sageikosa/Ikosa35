using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    /// <summary>Represents a shell made of PlanarPoints (effectively planes)</summary>
    public abstract class PlanarShell
    {
        /// <summary>Represents a shell made of PlanarPoints (effectively planes)</summary>
        protected PlanarShell()
        {
            _Faces = new List<PlanarPoints>();
        }

        protected List<PlanarPoints> _Faces;

        public IEnumerable<PlanarPoints> Faces
            => _Faces.Select(_f => _f);

        public bool Intersects(Point3D p1, Point3D p2)
            => Faces.Any(_f => _f.SegmentIntersection(p1, p2).HasValue);

        public Segment3D? TransitSegment(Point3D p1, Point3D p2)
        {
            var _points = (from _plane in Faces
                           let _pt = _plane.SegmentIntersection(p1, p2)
                           where _pt.HasValue
                           select _pt.Value).Distinct().ToList();
            if (_points.Any())
            {
                // longest vector
                return (from _p in _points
                        from _r in _points
                        select new Segment3D(_p, _r))
                        .OrderByDescending(_b => _b.Vector.LengthSquared)
                        .First();
            }

            // none
            return null;
        }

        /// <summary>Transform entire planar shell</summary>
        public void Transform(Transform3D transform)
        {
            foreach (var _plane in Faces)
                _plane.Transform(transform);
        }
    }
}
