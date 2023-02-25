using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Uzi.Packaging;
using Uzi.Visualize.Packaging;
using System.Windows.Markup;
using System.IO;

namespace CharacterModeler
{
    /// <summary>
    /// Interaction logic for FaceDesigner.xaml
    /// </summary>
    public partial class FaceDesigner : UserControl
    {
        public FaceDesigner()
        {
            InitializeComponent();
            this.DataContext = new FaceModel();
            noseChanged(this, null);
            mouthChanged(this, null);
            browsChanged(this, null);
            pupilsChanged(this, null);
        }

        public CorePackage Package { get; set; }
        public ResourceManager Manager { get; set; }

        private void browsChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if ((sldrBrowElevation != null) && (sldrBrowLength != null)
                && (sldrBrowOffset != null) && (sldrBrowRotation != null)
                && (sldrBrowSeparation != null))
            {
                // drawing data
                var _pathLeft = new PathGeometry();
                _pathLeft.Figures = new PathFigureCollection();
                var _pathRight = new PathGeometry();
                _pathRight.Figures = new PathFigureCollection();
                var _segmentsLeft = new List<PathSegment>();
                var _segmentsRight = new List<PathSegment>();

                // gather re-usable metrics for convenience
                var _length = sldrBrowLength.Value;
                var _elevation = sldrBrowElevation.Value;
                var _leftFar = 48 - (sldrBrowSeparation.Value + _length);
                var _leftNear = 48 - (sldrBrowSeparation.Value);
                var _rightFar = 48 + (sldrBrowSeparation.Value + _length);
                var _rightNear = 48 + (sldrBrowSeparation.Value);
                rotLeftBrow.CenterX = (_leftFar + _leftNear) / 2;
                rotLeftBrow.CenterY = _elevation;
                rotRightBrow.CenterX = (_rightFar + _rightNear) / 2;
                rotRightBrow.CenterY = _elevation;

                // tops
                var _size = new Size(_length / 2, Math.Abs(sldrBrowOffset.Value));
                _segmentsLeft.Add(new ArcSegment(new Point(_leftNear, _elevation),
                    _size, 180, sldrBrowOffset.Value >= 0,
                    sldrBrowOffset.Value < 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise, true));
                _segmentsRight.Add(new ArcSegment(new Point(_rightFar, _elevation),
                    _size, 180, sldrBrowOffset.Value >= 0,
                    sldrBrowOffset.Value < 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise, true));

                // finish by starting
                _pathLeft.Figures.Add(new PathFigure(new Point(_leftFar, _elevation), _segmentsLeft, false));
                _pathRight.Figures.Add(new PathFigure(new Point(_rightNear, _elevation), _segmentsRight, false));
                _pathLeft.Freeze();
                _pathRight.Freeze();

                // then path them
                pathLeftBrow.Data = _pathLeft;
                pathRightBrow.Data = _pathRight;
            }
        }

        private void pupilsChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if ((sldrIrisElevation != null) && (sldrIrisSeparation != null))
            {
                var _pupilHoriz = (sldrIrisWidth.Value - sldrPupilWidth.Value) / 2;
                var _pupilVert = (sldrIrisHeight.Value - sldrPupilHeight.Value) / 2;
                elpLeftIris.SetValue(Canvas.LeftProperty, 48 - (sldrIrisSeparation.Value + sldrIrisWidth.Value));
                elpRightIris.SetValue(Canvas.LeftProperty, 48 + sldrIrisSeparation.Value);
                elpLeftPupil.SetValue(Canvas.LeftProperty, 48 - (sldrIrisSeparation.Value + sldrIrisWidth.Value - _pupilHoriz));
                elpRightPupil.SetValue(Canvas.LeftProperty, 48 + sldrIrisSeparation.Value + _pupilHoriz);

                var _irisTop = sldrIrisElevation.Value;
                var _pupilTop = sldrIrisElevation.Value + _pupilVert;
                elpLeftIris.SetValue(Canvas.TopProperty, _irisTop);
                elpRightIris.SetValue(Canvas.TopProperty, _irisTop);
                elpLeftPupil.SetValue(Canvas.TopProperty, _pupilTop);
                elpRightPupil.SetValue(Canvas.TopProperty, _pupilTop);
            }
        }

        private void eyesChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if ((sldrEyeElevation != null) && (sldrEyeWidth != null) && (sldrEyeSeparation!=null)
                && (sldrEyeTopOffset!=null) && (sldrEyeBottomOffset!=null)
                )
            {
                // drawing data
                var _pathLeft = new PathGeometry();
                _pathLeft.Figures = new PathFigureCollection();
                var _pathRight = new PathGeometry();
                _pathRight.Figures = new PathFigureCollection();
                var _segmentsLeft = new List<PathSegment>();
                var _segmentsRight = new List<PathSegment>();

                // gather re-usable metrics for convenience
                var _width = sldrEyeWidth.Value;
                var _elevation = sldrEyeElevation.Value;
                var _leftFar = 48 - (sldrEyeSeparation.Value + _width);
                var _leftNear = 48 - (sldrEyeSeparation.Value);
                var _rightFar = 48 + (sldrEyeSeparation.Value + _width); 
                var _rightNear = 48 + (sldrEyeSeparation.Value);
                rotLeftEye.CenterX = (_leftFar + _leftNear) / 2;
                rotLeftEye.CenterY = _elevation;
                rotRightEye.CenterX = (_rightFar + _rightNear) / 2;
                rotRightEye.CenterY = _elevation;

                // tops
                var _topSize = new Size(_width/2, Math.Abs(sldrEyeTopOffset.Value));
                _segmentsLeft.Add(new ArcSegment(new Point(_leftNear, _elevation), _topSize, 180,
                    true, SweepDirection.Clockwise, false));
                _segmentsRight.Add(new ArcSegment(new Point(_rightFar, _elevation), _topSize, 180,
                    true, SweepDirection.Clockwise, false));

                // bottoms
                var _bottomSize = new Size(_width / 2, Math.Abs(sldrEyeBottomOffset.Value));
                _segmentsLeft.Add(new ArcSegment(new Point(_leftFar, _elevation), _bottomSize, 180,
                    true, SweepDirection.Clockwise, false));
                _segmentsRight.Add(new ArcSegment(new Point(_rightNear, _elevation), _bottomSize, 180,
                    true, SweepDirection.Clockwise, false));

                // finish by starting
                _pathLeft.Figures.Add(new PathFigure(new Point(_leftFar, _elevation), _segmentsLeft, true));
                _pathRight.Figures.Add(new PathFigure(new Point(_rightNear, _elevation), _segmentsRight, true));
                _pathLeft.Freeze();
                _pathRight.Freeze();

                // then path them
                pathLeftEye.Data = _pathLeft;
                pathRightEye.Data = _pathRight;
            }
        }

        private void noseChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if ((sldrNoseTopWidth != null) && (sldrNoseBottomWidth != null)
                && (sldrNoseElevation != null) && (sldrNoseHeight != null)
                && (chkLeft != null) && (chkRight != null)
                && (cboBridgeBottom != null) && (sldrNoseBottomOffset != null))
            {
                var _path = new PathGeometry();
                _path.Figures = new PathFigureCollection();
                var _segments = new List<PathSegment>();

                var _top = sldrNoseElevation.Value;
                var _bottom = _top + sldrNoseHeight.Value;
                var _topHalfWidth = sldrNoseTopWidth.Value / 2;
                var _bottomHalfWidth = sldrNoseBottomWidth.Value / 2;

                // "left" side
                _segments.Add(new LineSegment(new Point(48 - _bottomHalfWidth, _bottom), chkLeft.IsChecked ?? false));

                // bottom
                var _bridgeStyle = (cboBridgeBottom.SelectedItem == null)
                    ? BridgeEndStyle.None
                    : (BridgeEndStyle)cboBridgeBottom.SelectedItem;
                switch (_bridgeStyle)
                {
                    case BridgeEndStyle.None:
                        _segments.Add(new LineSegment(new Point(48 + _bottomHalfWidth, _bottom), false));
                        break;
                    case BridgeEndStyle.Point:
                        _segments.Add(new LineSegment(new Point(48, _bottom + sldrNoseBottomOffset.Value), true));
                        _segments.Add(new LineSegment(new Point(48 + _bottomHalfWidth, _bottom), true));
                        break;
                    case BridgeEndStyle.Arc:
                        _segments.Add(new ArcSegment(new Point(48 + _bottomHalfWidth, _bottom),
                            new Size(_bottomHalfWidth, Math.Abs(sldrNoseBottomOffset.Value)),
                            180, sldrNoseBottomOffset.Value >= 0,
                            sldrNoseBottomOffset.Value < 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise, true));
                        break;
                }

                // "right" side
                _segments.Add(new LineSegment(new Point(48 + _topHalfWidth, _top), chkRight.IsChecked ?? false));

                // from starting point
                _path.Figures.Add(new PathFigure(new Point(48 - _topHalfWidth, _top), _segments, false));

                pathBridge.Data = _path;
            }
        }

        private void chkNose(object sender, RoutedEventArgs e)
        {
            noseChanged(sender, null);
        }

        private void cboBridgeChanged(object sender, SelectionChangedEventArgs e)
        {
            noseChanged(sender, null);
        }

        private void mouthChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if ((sldrMouthBottomOffset != null) && (sldrMouthHeight != null) && (sldrMouthBottomWidth != null)
                && (sldrMouthTopOffset != null) && (sldrMouthElevation != null) && (sldrMouthTopWidth != null)
                )
            {
                // drawing data
                var _path = new PathGeometry();
                _path.Figures = new PathFigureCollection();
                var _segments = new List<PathSegment>();

                // gather re-usable metrics for convenience
                var _topWidth = sldrMouthTopWidth.Value;
                var _topLeft = 48d - (_topWidth / 2d);
                var _topPosition = sldrMouthElevation.Value;
                var _bottomWidth = sldrMouthBottomWidth.Value;
                var _bottomLeft = 48d - (_bottomWidth / 2d);
                var _bottomPosition = _topPosition + sldrMouthHeight.Value;

                // top
                _segments.Add(new ArcSegment(new Point(_topLeft + _topWidth, _topPosition),
                    new Size(_topWidth/2d, Math.Abs(sldrMouthTopOffset.Value)),
                    180, sldrMouthTopOffset.Value < 0,
                    sldrMouthTopOffset.Value >= 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise,
                    false));

                // right side
                _segments.Add(new LineSegment(new Point(_bottomLeft + _bottomWidth, _bottomPosition), false));

                // bottom
                _segments.Add(new ArcSegment(new Point(_bottomLeft, _bottomPosition),
                    new Size(_bottomWidth/2d, Math.Abs(sldrMouthBottomOffset.Value)),
                    180, sldrMouthBottomOffset.Value < 0,
                    sldrMouthBottomOffset.Value >= 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise,
                    false));

                // left side
                _segments.Add(new LineSegment(new Point(_topLeft, _topPosition), false));

                // starting point
                _path.Figures.Add(new PathFigure(new Point(48d - (_topWidth / 2d), _topPosition), _segments, true));
                _path.Freeze();

                // mouth path and lips path
                pathMouth.Data = _path;
            }
        }

        private void chkMouth(object sender, RoutedEventArgs e)
        {
            mouthChanged(sender, null);
        }

        private void fangsChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if ((sldrFangElevation != null) && (sldrFangHeight != null)
                && (sldrFangSeparation != null) && (sldrFangWidth != null))
            {
                // drawing data
                var _pathLeft = new PathGeometry();
                _pathLeft.Figures = new PathFigureCollection();
                var _pathRight = new PathGeometry();
                _pathRight.Figures = new PathFigureCollection();
                var _segmentsLeft = new List<PathSegment>();
                var _segmentsRight = new List<PathSegment>();

                // gather re-usable metrics for convenience
                var _width = sldrFangWidth.Value;
                var _height = sldrFangHeight.Value;
                var _elevation = sldrFangElevation.Value;
                var _leftFar = 48 - (sldrFangSeparation.Value + _width);
                var _leftNear = 48 - (sldrFangSeparation.Value);
                var _rightFar = 48 + (sldrFangSeparation.Value + _width);
                var _rightNear = 48 + (sldrFangSeparation.Value);

                // left
                _segmentsLeft.Add(new LineSegment(new Point(_leftNear, _elevation), false));
                _segmentsLeft.Add(new LineSegment(new Point((_leftNear + _leftFar) / 2, _elevation + _height), false));

                // right
                _segmentsRight.Add(new LineSegment(new Point(_rightFar, _elevation), false));
                _segmentsRight.Add(new LineSegment(new Point((_rightNear + _rightFar) / 2, _elevation + _height), false));

                // finish by starting
                _pathLeft.Figures.Add(new PathFigure(new Point(_leftFar, _elevation), _segmentsLeft, true));
                _pathRight.Figures.Add(new PathFigure(new Point(_rightNear, _elevation), _segmentsRight, true));
                _pathLeft.Freeze();
                _pathRight.Freeze();

                // then path them
                pathLeftFang.Data = _pathLeft;
                pathRightFang.Data = _pathRight;

                if ((chkFangLeft != null) && (chkFangRight != null))
                {
                    pathLeftFang.Visibility = (chkFangLeft.IsChecked ?? false) ? Visibility.Visible : Visibility.Hidden;
                    pathRightFang.Visibility = (chkFangRight.IsChecked ?? false) ? Visibility.Visible : Visibility.Hidden;
                }
            }
        }

        private void chkFang(object sender, RoutedEventArgs e)
        {
            fangsChanged(sender, null);
        }

        private void tusksChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if ((sldrTuskElevation != null) && (sldrTuskHeight != null)
                && (sldrTuskSeparation != null) && (sldrTuskWidth != null))
            {
                // drawing data
                var _pathLeft = new PathGeometry();
                _pathLeft.Figures = new PathFigureCollection();
                var _pathRight = new PathGeometry();
                _pathRight.Figures = new PathFigureCollection();
                var _segmentsLeft = new List<PathSegment>();
                var _segmentsRight = new List<PathSegment>();

                // gather re-usable metrics for convenience
                var _width =           sldrTuskWidth.Value;
                var _height = sldrTuskHeight.Value;
                var _elevation = sldrTuskElevation.Value;
                var _leftFar = 48 -   (sldrTuskSeparation.Value + _width);
                var _leftNear = 48 -  (sldrTuskSeparation.Value);
                var _rightFar = 48 +  (sldrTuskSeparation.Value + _width);
                var _rightNear = 48 + (sldrTuskSeparation.Value);

                // left
                _segmentsLeft.Add(new LineSegment(new Point(_leftNear, _elevation), false));
                _segmentsLeft.Add(new LineSegment(new Point((_leftNear + _leftFar) / 2,_elevation - _height), false));

                // right
                _segmentsRight.Add(new LineSegment(new Point(_rightFar, _elevation ), false));
                _segmentsRight.Add(new LineSegment(new Point((_rightNear + _rightFar) / 2, _elevation - _height),  false));

                // finish by starting
                _pathLeft.Figures.Add(new PathFigure( new Point(_leftFar, _elevation), _segmentsLeft, true));
                _pathRight.Figures.Add(new PathFigure(new Point(_rightNear, _elevation), _segmentsRight, true));
                _pathLeft.Freeze();
                _pathRight.Freeze();

                // then path them
                 pathLeftTusk.Data = _pathLeft;
                pathRightTusk.Data = _pathRight;

                if ((chkTuskLeft != null) && (chkTuskRight != null))
                {
                    pathLeftTusk.Visibility = (chkTuskLeft.IsChecked ?? false) ? Visibility.Visible : Visibility.Hidden;
                    pathRightTusk.Visibility = (chkTuskRight.IsChecked ?? false) ? Visibility.Visible : Visibility.Hidden;
                }
            }
        }

        private void chkTusk(object sender, RoutedEventArgs e)
        {
            tusksChanged(sender, null);
        }

        private void cbOpen_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cbOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter.ToString() == @"Template")
            {
                // template from file
                var _opener = new System.Windows.Forms.OpenFileDialog();
                _opener.CheckFileExists = true;
                _opener.CheckPathExists = true;
                _opener.Filter = @"Face Template Files (*.face)|*.face";
                _opener.Title = @"Open Face Template...";
                _opener.ValidateNames = true;
                var _rslt = _opener.ShowDialog();
                if (_rslt == System.Windows.Forms.DialogResult.OK)
                {
                    using (var _stream = File.OpenRead(_opener.FileName))
                        try
                        {
                            this.DataContext = XamlReader.Load(_stream) as FaceModel;
                        }
                        catch
                        {
                        }
                }
            }
            else
            {
                // package and select resource manager
            }
        }

        private void cbSaveAs_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cbSaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter.ToString() == @"Template")
            {
                // template to file
                var _saver = new System.Windows.Forms.SaveFileDialog();
                _saver.Filter = @"Face Template Files (*.face)|*.face";
                _saver.DefaultExt = @"face";
                if (_saver.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using (var _stream = File.OpenWrite(_saver.FileName))
                        try
                        {
                             XamlWriter.Save(this.DataContext, _stream);
                        }
                        catch
                        {
                        }
                }
           }
            else
            {
                // image to file
                var _saver = new System.Windows.Forms.SaveFileDialog();
                _saver.Filter = @"PNG (*.png)|*.png";
                _saver.DefaultExt = @"png";
                if (_saver.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // render the canvas
                    var _render = new RenderTargetBitmap(288, 288, 96, 96, PixelFormats.Pbgra32);
                    _render.Render(cnvFace);

                    // encode as PNG
                    var _encoder = new PngBitmapEncoder();
                    _encoder.Frames.Add(BitmapFrame.Create(_render));

                    // save
                    using (var _stream = File.Create(_saver.FileName))
                        _encoder.Save(_stream);
                }
            }
        }

        private void cbClose_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Package != null;
            e.Handled = true;
        }

        private void cbClose_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO: close package
        }

        private void cbSave_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (Package != null) && (Manager != null);
            e.Handled = true;
        }

        private void cbSave_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO: add image and solid color brushes to resource manager
            // TODO: save package?
        }
    }
}
