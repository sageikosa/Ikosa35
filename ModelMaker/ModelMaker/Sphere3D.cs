using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;

namespace ModelMaker
{
    public class Sphere3D:ModelElement3D
    {
        public Sphere3D()
            : base()
        {
        }

        public static DependencyProperty RadiusProperty = DependencyProperty.Register("Radius", typeof(double), typeof(Sphere3D),
           new PropertyMetadata(1d, new PropertyChangedCallback(OnMeshChanged)));

        public static DependencyProperty LongSidesProperty = DependencyProperty.Register("LongSides", typeof(int), typeof(Sphere3D),
           new PropertyMetadata(12, new PropertyChangedCallback(OnMeshChanged)));

        public static DependencyProperty LatSidesProperty = DependencyProperty.Register("LatSides", typeof(int), typeof(Sphere3D),
           new PropertyMetadata(6, new PropertyChangedCallback(OnMeshChanged)));

        public static DependencyProperty LongAngleAProperty = DependencyProperty.Register("LongAngleA", typeof(double), typeof(Sphere3D),
           new PropertyMetadata(0d, new PropertyChangedCallback(OnMeshChanged)));

        public static DependencyProperty LongAngleBProperty = DependencyProperty.Register("LongAngleB", typeof(double), typeof(Sphere3D),
           new PropertyMetadata(360d, new PropertyChangedCallback(OnMeshChanged)));

        public static DependencyProperty LatAngleAProperty = DependencyProperty.Register("LatAngleA", typeof(double), typeof(Sphere3D),
           new PropertyMetadata(-90d, new PropertyChangedCallback(OnMeshChanged)));

        public static DependencyProperty LatAngleBProperty = DependencyProperty.Register("LatAngleB", typeof(double), typeof(Sphere3D),
           new PropertyMetadata(90d, new PropertyChangedCallback(OnMeshChanged)));

        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }
        
        public int LongSides
        {
            get { return (int)GetValue(LongSidesProperty); }
            set { SetValue(LongSidesProperty, value); }
        }

        public int LatSides
        {
            get { return (int)GetValue(LatSidesProperty); }
            set { SetValue(LatSidesProperty, value); }
        }

        public double LongAngleA
        {
            get { return (double)GetValue(LongAngleAProperty); }
            set { SetValue(LongAngleAProperty, value); }
        }

        public double LongAngleB
        {
            get { return (double)GetValue(LongAngleBProperty); }
            set { SetValue(LongAngleBProperty, value); }
        }

        public double LatAngleA
        {
            get { return (double)GetValue(LatAngleAProperty); }
            set { SetValue(LatAngleAProperty, value); }
        }

        public double LatAngleB
        {
            get { return (double)GetValue(LatAngleBProperty); }
            set { SetValue(LatAngleBProperty, value); }
        }

        private static void OnMeshChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Sphere3D _sph = (Sphere3D)sender;
            MeshGeometry3D _mesh = VolumetricMeshMaker.SphericalSurface(_sph.Radius, _sph.LongSides, _sph.LatSides, _sph.LongAngleA, _sph.LongAngleB, _sph.LatAngleA, _sph.LatAngleB);
            _mesh.Freeze();
            _sph._Content.Geometry = _mesh;
        }

        protected override Geometry3D RegenMesh()
        {
            MeshGeometry3D _mesh = VolumetricMeshMaker.SphericalSurface(Radius, LongSides, LatSides, LongAngleA, LongAngleB, LatAngleA, LatAngleB);
            _mesh.Freeze();
            return _mesh;
        }
    }
}
