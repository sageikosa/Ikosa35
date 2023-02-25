using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;

namespace ModelMaker
{
    public class Cone3D : PolarElement3D
    {
        public static DependencyProperty UpperRadiusProperty = DependencyProperty.Register("UpperRadius", typeof(double), typeof(Cone3D),
            new PropertyMetadata(0d, new PropertyChangedCallback(OnMeshChanged)));

        public double UpperRadius
        {
            get { return (double)GetValue(UpperRadiusProperty); }
            set { SetValue(UpperRadiusProperty, value); }
        }

        protected override Geometry3D RegenMesh()
        {
            MeshGeometry3D _mesh = VolumetricMeshMaker.ConicSurface(this.Radius, this.UpperRadius, this.Height, this.StartAngle, this.EndAngle,
                this.Sides, this.Planes);
            _mesh.Freeze();
            return _mesh;
        }
    }
}
