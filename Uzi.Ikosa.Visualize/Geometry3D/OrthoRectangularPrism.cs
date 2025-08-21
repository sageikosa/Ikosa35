using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    public class OrthoRectangularPrism : PlanarShell
    {
        #region construction
        public OrthoRectangularPrism(Point3D point, Vector3D size)
            : base()
        {
            var _C = new Vector3D(0, 0, size.Z);
            var _B = new Vector3D(0, size.Y, 0);
            var _A = new Vector3D(size.X, 0, 0);

            // points
            var _xyz = point;
            var _xyC = point + _C;
            var _xBz = point + _B;
            var _xBC = _xBz + _C;
            var _Ayz = _xyz + _A;
            var _AyC = _xyC + _A;
            var _ABz = _xBz + _A;
            var _ABC = _xBC + _A;

            // planes
            _Faces.Add(new PlanarPoints(_xyz, _xyC, _xBC, _xBz));
            _Faces.Add(new PlanarPoints(_Ayz, _ABz, _ABC, _AyC));
            _Faces.Add(new PlanarPoints(_xyz, _Ayz, _AyC, _xyC));
            _Faces.Add(new PlanarPoints(_xBz, _xBC, _ABC, _ABz));
            _Faces.Add(new PlanarPoints(_xyz, _xBz, _ABz, _Ayz));
            _Faces.Add(new PlanarPoints(_xyC, _AyC, _ABC, _xBC));
        }
        #endregion
    }
}
