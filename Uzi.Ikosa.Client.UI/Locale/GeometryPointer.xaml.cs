using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Uzi.Visualize;

namespace Uzi.Ikosa.Client.UI
{
    /// <summary>
    /// Interaction logic for GeometryPointer.xaml
    /// </summary>
    public partial class GeometryPointer : UserControl
    {
        public GeometryPointer()
        {
            try { InitializeComponent(); } catch { }
        }

        #region public IGeometricRegion SourceGeometry { get; set; } DEPENDENCY
        public IGeometricRegion SourceGeometry
        {
            get { return (IGeometricRegion)GetValue(SourceGeometryProperty); }
            set { SetValue(SourceGeometryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SourceCell.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceGeometryProperty =
            DependencyProperty.Register(@"SourceGeometry", typeof(IGeometricRegion), typeof(GeometryPointer), new UIPropertyMetadata(null, OnHeadingsChanged));
        #endregion

        #region public Point3D SourcePoint { get; set; } DEPENDENCY
        public Point3D SourcePoint
        {
            get { return (Point3D)GetValue(SourcePointProperty); }
            set { SetValue(SourceGeometryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SourceCell.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourcePointProperty =
            DependencyProperty.Register(@"SourcePoint", typeof(Point3D), typeof(GeometryPointer), new UIPropertyMetadata(new Point3D(), OnHeadingsChanged));
        #endregion

        #region public IGeometricRegion TargetGeometry { get; set; } DEPENDENCY
        public IGeometricRegion TargetGeometry
        {
            get { return (IGeometricRegion)GetValue(TargetGeometryProperty); }
            set { SetValue(TargetGeometryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TargetCell.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TargetGeometryProperty =
            DependencyProperty.Register(@"TargetGeometry", typeof(IGeometricRegion), typeof(GeometryPointer), new UIPropertyMetadata(null, OnHeadingsChanged));
        #endregion

        #region public AnchorFace DownFace { get; set; } DEPENDENCY
        public AnchorFace DownFace
        {
            get { return (AnchorFace)GetValue(DownFaceProperty); }
            set { SetValue(DownFaceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DownFace.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DownFaceProperty =
            DependencyProperty.Register(@"DownFace", typeof(AnchorFace), typeof(GeometryPointer), new UIPropertyMetadata(AnchorFace.ZLow, OnHeadingsChanged));
        #endregion

        #region public int Heading { get; set; } DEPENDENCY
        public int Heading
        {
            get { return (int)GetValue(HeadingProperty); }
            set { SetValue(HeadingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Heading.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeadingProperty =
            DependencyProperty.Register(@"Heading", typeof(int), typeof(GeometryPointer), new UIPropertyMetadata(0, OnHeadingsChanged));
        #endregion

        private static void OnHeadingsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var _pointer = dependencyObject as GeometryPointer;
            _pointer.RedrawPointer();
        }

        private const string ElevFormat = @"{0:+0.#;-0.#;\-\-}";

        #region public void RedrawPointer()
        public void RedrawPointer()
        {
            if (TargetGeometry != null)
            {
                // displacement vector
                var _target = TargetGeometry.GetPoint3D();
                var _source = SourceGeometry != null ? SourceGeometry.GetPoint3D() : SourcePoint;
                var _vector = _target - _source;

                // distance (to geometry or source point)
                var _distance = SourceGeometry != null
                    ? TargetGeometry.NearDistance(SourceGeometry)
                    : TargetGeometry.NearDistance(_source);
                txtDistance.Text = _distance.ToString(@"0.#");

                // up vector
                var _upVector = new Vector3D(0, 0, 1);
                switch (DownFace)
                {
                    case AnchorFace.XLow:
                        _upVector = new Vector3D(1, 0, 0);
                        txtElevation.Text = string.Format(ElevFormat, _target.X - _source.X);
                        _target.X = _source.X;
                        break;
                    case AnchorFace.XHigh:
                        _upVector = new Vector3D(-1, 0, 0);
                        txtElevation.Text = string.Format(ElevFormat, _source.X - _target.X);
                        _target.X = _source.X;
                        break;
                    case AnchorFace.YLow:
                        txtElevation.Text = string.Format(ElevFormat, _target.Y - _source.Y);
                        _upVector = new Vector3D(0, 1, 0);
                        _target.Y = _source.Y;
                        break;
                    case AnchorFace.YHigh:
                        txtElevation.Text = string.Format(ElevFormat, _source.Y - _target.Y);
                        _upVector = new Vector3D(0, -1, 0);
                        _target.Y = _source.Y;
                        break;
                    case AnchorFace.ZHigh:
                        txtElevation.Text = string.Format(ElevFormat, _source.Z - _target.Z);
                        _upVector = new Vector3D(0, 0, -1);
                        _target.Z = _source.Z;
                        break;
                    default:
                        txtElevation.Text = string.Format(ElevFormat, _target.Z - _source.Z);
                        _target.Z = _source.Z;
                        break;
                }

                // use target cell projected onto same plane
                _vector = _target - _source;
                if (Vector3D.CrossProduct(_upVector, _vector).Length == 0)
                {
                    // _upVector and _vector are parallel
                    // hide lineDirection and txtDistance (since elevation will be distance)
                    lineDirection.StrokeThickness = 0d;
                    txtDistance.Text = string.Empty;
                    txDirection.Angle = 0;
                }
                else
                {
                    // not parallel
                    lineDirection.StrokeThickness = 4d;

                    // forward vector based on heading...
                    var _heading = DownFace.GetHeadingVector(Heading);

                    // angle projected onto normal plane
                    txDirection.Angle = _upVector.AxisAngleBetween(_vector, _heading);
                }
            }
        }
        #endregion
    }
}
