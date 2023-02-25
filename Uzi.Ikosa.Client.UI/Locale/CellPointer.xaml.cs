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
    /// Interaction logic for CellPointer.xaml
    /// </summary>
    public partial class CellPointer : UserControl
    {
        public CellPointer()
        {
            try { InitializeComponent(); } catch { }
        }

        #region public ICellLocation SourceCell { get; set; } DEPENDENCY
        public ICellLocation SourceCell
        {
            get { return (ICellLocation)GetValue(SourceCellProperty); }
            set { SetValue(SourceCellProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SourceCell.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceCellProperty =
            DependencyProperty.Register(@"SourceCell", typeof(ICellLocation), typeof(CellPointer), new UIPropertyMetadata(null, OnHeadingsChanged));
        #endregion

        #region public ICellLocation TargetCell { get; set; } DEPENDENCY
        public ICellLocation TargetCell
        {
            get { return (ICellLocation)GetValue(TargetCellProperty); }
            set { SetValue(TargetCellProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TargetCell.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TargetCellProperty =
            DependencyProperty.Register(@"TargetCell", typeof(ICellLocation), typeof(CellPointer), new UIPropertyMetadata(null, OnHeadingsChanged));
        #endregion

        #region public AnchorFace DownFace { get; set; } DEPENDENCY
        public AnchorFace DownFace
        {
            get { return (AnchorFace)GetValue(DownFaceProperty); }
            set { SetValue(DownFaceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DownFace.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DownFaceProperty =
            DependencyProperty.Register(@"DownFace", typeof(AnchorFace), typeof(CellPointer), new UIPropertyMetadata(AnchorFace.ZLow, OnHeadingsChanged));
        #endregion

        #region public int Heading { get; set; } DEPENDENCY
        public int Heading
        {
            get { return (int)GetValue(HeadingProperty); }
            set { SetValue(HeadingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Heading.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeadingProperty =
            DependencyProperty.Register(@"Heading", typeof(int), typeof(CellPointer), new UIPropertyMetadata(0, OnHeadingsChanged));
        #endregion

        private static void OnHeadingsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var _pointer = dependencyObject as CellPointer;
            _pointer.RedrawPointer();
        }

        private const string ElevFormat = @"{0:+0.#;-0.#;\-\-}";

        #region public void RedrawPointer()
        public void RedrawPointer()
        {
            if ((TargetCell != null)
                && (SourceCell != null))
            {
                // displacement vector
                var _target = TargetCell.GetPoint();
                var _source = SourceCell.GetPoint();
                var _vector = _target - _source;

                // distance
                txtDistance.Text = _vector.Length.ToString(@"0.#");

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
