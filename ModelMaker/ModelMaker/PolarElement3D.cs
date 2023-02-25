using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;

namespace ModelMaker
{
    public abstract class PolarElement3D : ModelElement3D
    {
        public static DependencyProperty RadiusProperty = DependencyProperty.Register("Radius", typeof(double), typeof(PolarElement3D),
            new PropertyMetadata(1d, new PropertyChangedCallback(OnMeshChanged)));

        public static DependencyProperty StartAngleProperty = DependencyProperty.Register("StartAngle", typeof(double), typeof(PolarElement3D),
            new PropertyMetadata(0d, new PropertyChangedCallback(OnMeshChanged)));

        public static DependencyProperty EndAngleProperty = DependencyProperty.Register("EndAngle", typeof(double), typeof(PolarElement3D),
            new PropertyMetadata(360d, new PropertyChangedCallback(OnMeshChanged)));

        public static DependencyProperty SidesProperty = DependencyProperty.Register("Sides", typeof(int), typeof(PolarElement3D),
            new PropertyMetadata(12, new PropertyChangedCallback(OnMeshChanged)));

        public static DependencyProperty PlanesProperty = DependencyProperty.Register("Planes", typeof(int), typeof(PolarElement3D),
            new PropertyMetadata(3, new PropertyChangedCallback(OnMeshChanged)));

        public static DependencyProperty HeightProperty = DependencyProperty.Register("Height", typeof(double), typeof(PolarElement3D),
            new PropertyMetadata(1d, new PropertyChangedCallback(OnMeshChanged)));

        public static DependencyProperty IsFacettedProperty = DependencyProperty.Register("IsFacetted", typeof(bool), typeof(PolarElement3D),
            new PropertyMetadata(false, new PropertyChangedCallback(OnMeshChanged)));

        public double Height
        {
            get { return (double)GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        public double StartAngle
        {
            get { return (double)GetValue(StartAngleProperty); }
            set { SetValue(StartAngleProperty, value); }
        }

        public double EndAngle
        {
            get { return (double)GetValue(EndAngleProperty); }
            set { SetValue(EndAngleProperty, value); }
        }

        public int Sides
        {
            get { return (int)GetValue(SidesProperty); }
            set { SetValue(SidesProperty, value); }
        }

        public int Planes
        {
            get { return (int)GetValue(PlanesProperty); }
            set { SetValue(PlanesProperty, value); }
        }

        public bool IsFacetted
        {
            get { return (bool)GetValue(IsFacettedProperty); }
            set { SetValue(IsFacettedProperty, value); }
        }

        protected static void OnMeshChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            PolarElement3D _elem = (PolarElement3D)sender;
            _elem._Content.Geometry = _elem.RegenMesh();
        }

    }
}
