using System.Windows.Markup;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using System.Windows;
using System.Windows.Media;

namespace Uzi.Visualize
{
    [MarkupExtensionReturnType(typeof(MeshGeometry3D))]
    public class ConeMesh : MeshExtension
    {
        public ConeMesh()
        {
            Direction = new Vector3D(0, 0, 1);
            BaseRadius = 1d;
            TopRadius = 1d;
            ThetaDiv = 20;
            OriginScale = new Vector3D(1, 1, 1);
            DestinationScale = new Vector3D(1, 1, 1);
        }

        public Point3D Origin { get; set; }
        public Vector3D Direction { get; set; }
        public double BaseRadius { get; set; }
        public double TopRadius { get; set; }
        public double Height { get; set; }
        public int ThetaDiv { get; set; }
        public bool BaseCap { get; set; }
        public bool TopCap { get; set; }
        public bool FlatMap { get; set; }
        public Vector3D OriginScale { get; set; }
        public Vector3D DestinationScale { get; set; }
        public Vector3D Skew { get; set; }

        protected override MeshGeometry3D GenerateMesh()
        {
            // destination transforms
            var _destTrans = new Transform3DGroup();
            _destTrans.Children.Add(new ScaleTransform3D(DestinationScale));
            _destTrans.Children.Add(new TranslateTransform3D(Skew));

            var _builder = new MeshBuilder();
            _builder.AddCone(Origin, Direction, BaseRadius, TopRadius, Height, BaseCap & !FlatMap, TopCap & !FlatMap,
                ThetaDiv, new ScaleTransform3D(OriginScale), _destTrans);

            var _mesh = _builder.ToMesh();
            if (FlatMap)
            {
                // fixup texture mapping
                _mesh.TextureCoordinates = [];
                var _center = new Point(0, 0);
                var _vector = new Vector(-0.5, 0);
                var _point = _center + _vector;
                var _rotate = new RotateTransform(360d / (ThetaDiv - 1));
                for (var _tx = 0; _tx < ThetaDiv; _tx++)
                {
                    _mesh.TextureCoordinates.Add(_point);
                    _mesh.TextureCoordinates.Add(_center);
                    _point = _rotate.Transform(_point);
                }
            }

            _mesh.Freeze();
            return _mesh;
        }
    }
}