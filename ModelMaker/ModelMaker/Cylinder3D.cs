using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;

namespace ModelMaker
{
    public class Cylinder3D : PolarElement3D
    {
        public Cylinder3D()
            : base()
        {
        }

        // dependency properties for all of these
        // TODO: stretch texture (texture map over all surface) or cut texture (generate extra points for surface that are not part of any triangle)

        protected override Geometry3D RegenMesh()
        {
            if (IsFacetted)
            {
                MeshGeometry3D _mesh = VolumetricMeshMaker.SegmentedCylindricalSurface(Radius, Height, Sides, Planes, StartAngle, EndAngle);
                //MeshGeometry3D _mesh = VolumetricMeshMaker.PolygonalCylindricalSurface(Radius, Height, Sides, Planes, StartAngle, EndAngle);
                //var _trans = new Transform3DGroup();
                //_trans.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), -90)));
                //_trans.Children.Add(new TranslateTransform3D(new Vector3D(0, 0, -2.5d)));
                //_mesh = _trans.Transform(_mesh);
                _mesh.Freeze();
                return _mesh;
            }
            else
            {
                MeshGeometry3D _mesh = VolumetricMeshMaker.CylindricalSurface(Radius, Height, Sides, Planes, StartAngle, EndAngle);
                _mesh.Freeze();
                return _mesh;
            }
        }
    }
}
