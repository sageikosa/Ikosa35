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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Uzi.Ikosa.Tactical;
using Uzi.Core;
using Uzi.Ikosa.Senses;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Services;
using Uzi.Visualize;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Host
{
    /// <summary>
    /// Interaction logic for MapExtents.xaml
    /// </summary>
    public partial class MapExtents : UserControl, IAwarenessLevels, IZoomIcons
    {
        const float _fact = 0.2f;
        const float _off = 2.5f;

        public static RoutedCommand CameraMove = new RoutedCommand();
        public static RoutedCommand CameraForward = new RoutedCommand();
        public static RoutedCommand CameraBackward = new RoutedCommand();
        public static RoutedCommand CameraLeft = new RoutedCommand();
        public static RoutedCommand CameraRight = new RoutedCommand();
        public static RoutedCommand CameraUp = new RoutedCommand();
        public static RoutedCommand CameraDown = new RoutedCommand();
        public static RoutedCommand CameraRollUpLeft = new RoutedCommand();
        public static RoutedCommand CameraRollUpRight = new RoutedCommand();
        public static RoutedCommand CameraPivotLeft = new RoutedCommand();
        public static RoutedCommand CameraPivotRight = new RoutedCommand();
        public static RoutedCommand CameraTiltForward = new RoutedCommand();
        public static RoutedCommand CameraTiltBackward = new RoutedCommand();
        public static RoutedCommand CameraLevel = new RoutedCommand();

        public MapExtents()
        {
            InitializeComponent();
        }

        #region private void MoveInDirection(int _direction)
        private void MoveInDirection(int _direction)
        {
            var _newPos = povCamera.Position;
            switch (_direction)
            {
                case 0:
                    _newPos.Offset(5, 0, 0);
                    povCamera.Position = _newPos;
                    _CellLoc = new CellLocation(_CellLoc.Z, _CellLoc.Y, _CellLoc.X + 1);
                    break;
                case 1:
                    _newPos.Offset(5, 5, 0);
                    povCamera.Position = _newPos;
                    _CellLoc = new CellLocation(_CellLoc.Z, _CellLoc.Y + 1, _CellLoc.X + 1);
                    break;
                case 2:
                    _newPos.Offset(0, 5, 0);
                    povCamera.Position = _newPos;
                    _CellLoc = new CellLocation(_CellLoc.Z, _CellLoc.Y + 1, _CellLoc.X);
                    break;
                case 3:
                    _newPos.Offset(-5, 5, 0);
                    povCamera.Position = _newPos;
                    _CellLoc = new CellLocation(_CellLoc.Z, _CellLoc.Y + 1, _CellLoc.X - 1);
                    break;
                case 4:
                    _newPos.Offset(-5, 0, 0);
                    povCamera.Position = _newPos;
                    _CellLoc = new CellLocation(_CellLoc.Z, _CellLoc.Y, _CellLoc.X - 1);
                    break;
                case 5:
                    _newPos.Offset(-5, -5, 0);
                    povCamera.Position = _newPos;
                    _CellLoc = new CellLocation(_CellLoc.Z, _CellLoc.Y - 1, _CellLoc.X - 1);
                    break;
                case 6:
                    povCamera.Position.Offset(0, -5, 0);
                    _newPos.Offset(0, -5, 0);
                    povCamera.Position = _newPos;
                    _CellLoc = new CellLocation(_CellLoc.Z, _CellLoc.Y - 1, _CellLoc.X);
                    break;
                case 7:
                    povCamera.Position.Offset(5, -5, 0);
                    _newPos.Offset(5, -5, 0);
                    povCamera.Position = _newPos;
                    _CellLoc = new CellLocation(_CellLoc.Z, _CellLoc.Y - 1, _CellLoc.X + 1);
                    break;
            }
            txtYCoord.Text = _CellLoc.Y.ToString();
            txtXCoord.Text = _CellLoc.X.ToString();
        }
        #endregion

        private void RedrawAll()
        {
            var _senses = FilteredSenses();
            RedrawTerrain(_senses);
            RedrawLocators(_senses);
        }

        private void cbCamera_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        #region movement commands
        private void cbMove_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _cmd = e.Parameter.ToString().Split(',');
            MoveInDirection((_Heading + int.Parse(_cmd[0])) % 8);
            if (_cmd.Length > 1)
            {
                switch (int.Parse(_cmd[1]))
                {
                    case -1:
                        cbDown_Executed(sender, e);
                        return;
                    case 1:
                        cbUp_Executed(sender, e);
                        return;
                }
            }
            e.Handled = true;
            RedrawAll();
        }

        private void cbUp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _newPos = povCamera.Position;
            _newPos.Offset(0, 0, 5);
            povCamera.Position = _newPos;
            _CellLoc = new CellLocation(_CellLoc.Z + 1, _CellLoc.Y, _CellLoc.X);
            RedrawAll();
            txtZCoord.Text = _CellLoc.Z.ToString();
        }

        private void cbDown_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _newPos = povCamera.Position;
            _newPos.Offset(0, 0, -5);
            povCamera.Position = _newPos;
            _CellLoc = new CellLocation(_CellLoc.Z - 1, _CellLoc.Y, _CellLoc.X);
            RedrawAll();
            txtZCoord.Text = _CellLoc.Z.ToString();
        }
        #endregion

        #region rotation commands
        private void cbRollUpLeft_Executed(object sender, ExecutedRoutedEventArgs e)
        {
        }

        private void cbRollUpRight_Executed(object sender, ExecutedRoutedEventArgs e)
        {
        }

        private void cbPivotLeft_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _Heading = (_Heading + 1) % 8;
            SetLook();
        }

        private void cbPivotRight_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _Heading = (_Heading + 7) % 8;
            SetLook();
        }

        private void cbTiltForward_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _Incline--;
            if (_Incline < -2)
                _Incline = -2;
            else
                SetLook();
        }

        private void cbTiltBackward_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _Incline++;
            if (_Incline > 2)
                _Incline = 2;
            else
                SetLook();
        }
        #endregion

        #region private void SetLook()
        private void SetLook()
        {
            if (_Incline == 2)
            {
                var _animate = new Vector3DAnimation(povCamera.LookDirection, new Vector3D(0, 0, 1),
                    new Duration(TimeSpan.FromMilliseconds(250)));
                povCamera.BeginAnimation(PerspectiveCamera.LookDirectionProperty, _animate);
                switch (_Heading)
                {
                    case 0:
                        povCamera.UpDirection = new Vector3D(-1, 0, 0);
                        break;
                    case 1:
                        povCamera.UpDirection = new Vector3D(-1, -1, 0);
                        break;
                    case 2:
                        povCamera.UpDirection = new Vector3D(0, -1, 0);
                        break;
                    case 3:
                        povCamera.UpDirection = new Vector3D(1, -1, 0);
                        break;
                    case 4:
                        povCamera.UpDirection = new Vector3D(1, 0, 0);
                        break;
                    case 5:
                        povCamera.UpDirection = new Vector3D(1, 1, 0);
                        break;
                    case 6:
                        povCamera.UpDirection = new Vector3D(0, 1, 0);
                        break;
                    case 7:
                        povCamera.UpDirection = new Vector3D(-1, 1, 0);
                        break;
                }
            }
            else if (_Incline == -2)
            {
                var _animate = new Vector3DAnimation(povCamera.LookDirection, new Vector3D(0, 0, -1),
                    new Duration(TimeSpan.FromMilliseconds(250)));
                povCamera.BeginAnimation(PerspectiveCamera.LookDirectionProperty, _animate);
                switch (_Heading)
                {
                    case 0:
                        povCamera.UpDirection = new Vector3D(1, 0, 0);
                        break;
                    case 1:
                        povCamera.UpDirection = new Vector3D(1, 1, 0);
                        break;
                    case 2:
                        povCamera.UpDirection = new Vector3D(0, 1, 0);
                        break;
                    case 3:
                        povCamera.UpDirection = new Vector3D(-1, 1, 0);
                        break;
                    case 4:
                        povCamera.UpDirection = new Vector3D(-1, 0, 0);
                        break;
                    case 5:
                        povCamera.UpDirection = new Vector3D(-1, -1, 0);
                        break;
                    case 6:
                        povCamera.UpDirection = new Vector3D(0, -1, 0);
                        break;
                    case 7:
                        povCamera.UpDirection = new Vector3D(1, -1, 0);
                        break;
                }
            }
            else
            {
                povCamera.UpDirection = new Vector3D(0, 0, 1);
                switch (_Heading)
                {
                    case 0:
                        {
                            var _animate = new Vector3DAnimation(povCamera.LookDirection, new Vector3D(1, 0, _Incline),
                                new Duration(TimeSpan.FromMilliseconds(250)), FillBehavior.HoldEnd);
                            povCamera.BeginAnimation(PerspectiveCamera.LookDirectionProperty, _animate);
                        }
                        break;
                    case 1:
                        {
                            var _animate = new Vector3DAnimation(povCamera.LookDirection, new Vector3D(1, 1, _Incline),
                                new Duration(TimeSpan.FromMilliseconds(250)));
                            povCamera.BeginAnimation(PerspectiveCamera.LookDirectionProperty, _animate);
                        }
                        break;
                    case 2:
                        {
                            var _animate = new Vector3DAnimation(povCamera.LookDirection, new Vector3D(0, 1, _Incline),
                                new Duration(TimeSpan.FromMilliseconds(250)));
                            povCamera.BeginAnimation(PerspectiveCamera.LookDirectionProperty, _animate);
                        }
                        break;
                    case 3:
                        {
                            var _animate = new Vector3DAnimation(povCamera.LookDirection, new Vector3D(-1, 1, _Incline),
                                new Duration(TimeSpan.FromMilliseconds(250)));
                            povCamera.BeginAnimation(PerspectiveCamera.LookDirectionProperty, _animate);
                        }
                        break;
                    case 4:
                        {
                            var _animate = new Vector3DAnimation(povCamera.LookDirection, new Vector3D(-1, 0, _Incline),
                                new Duration(TimeSpan.FromMilliseconds(250)));
                            povCamera.BeginAnimation(PerspectiveCamera.LookDirectionProperty, _animate);
                        }
                        break;
                    case 5:
                        {
                            var _animate = new Vector3DAnimation(povCamera.LookDirection, new Vector3D(-1, -1, _Incline),
                                new Duration(TimeSpan.FromMilliseconds(250)));
                            povCamera.BeginAnimation(PerspectiveCamera.LookDirectionProperty, _animate);
                        }
                        break;
                    case 6:
                        {
                            var _animate = new Vector3DAnimation(povCamera.LookDirection, new Vector3D(0, -1, _Incline),
                                new Duration(TimeSpan.FromMilliseconds(250)));
                            povCamera.BeginAnimation(PerspectiveCamera.LookDirectionProperty, _animate);
                        }
                        break;
                    case 7:
                        {
                            var _animate = new Vector3DAnimation(povCamera.LookDirection, new Vector3D(1, -1, _Incline),
                                new Duration(TimeSpan.FromMilliseconds(250)));
                            povCamera.BeginAnimation(PerspectiveCamera.LookDirectionProperty, _animate);
                        }
                        break;
                }
            }
        }
        #endregion

        private void cbLevel_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private LocalMap _Map;

        #region public void LoadMap(LocalMap localMap)
        public void LoadMap(LocalMap localMap)
        {
            _Map = localMap;
            DataContext = _Map;

            // viewport textboxes
            txtXPos.Text = _Map.BackgroundViewport.X.ToString();
            txtYPos.Text = _Map.BackgroundViewport.Y.ToString();
            txtZPos.Text = _Map.BackgroundViewport.Z.ToString();
            txtXSize.Text = _Map.BackgroundViewport.XLength.ToString();
            txtYSize.Text = _Map.BackgroundViewport.YLength.ToString();
            txtZSize.Text = _Map.BackgroundViewport.ZHeight.ToString();
        }
        #endregion

        private CellLocation _CellLoc = new CellLocation(1, 1, 1);
        private int _Heading = 1;
        private int _Incline = -1;

        #region private void UpdateLighting()
        private void UpdateLighting()
        {
            var _notifiers = _Map.Rooms
                .SelectMany(_g => _g.NotifyLighting())
                .Distinct()
                .ToList();

            AwarenessSet.RecalculateAllSensors(_Map, _notifiers, true);

            _Map.ShadingZones.RecacheShadingZones();
            _Map.ShadingZones.ReshadeBackground();
            var _senses = FilteredSenses();
            RedrawTerrain(_senses);
            RedrawLocators(_senses);
        }
        #endregion

        #region private void RedrawTerrain(List<SensoryBase> filteredSenses)
        private void RedrawTerrain(List<SensoryBase> filteredSenses)
        {
            grpRooms.Children.Clear();
            grpAlphaRooms.Children.Clear();
            DrawingTools.DebugGroup = grpRooms;
            List<SensoryBase> _senses = filteredSenses ?? FilteredSenses();
            var _visualizer = new TerrainVisualizer(_senses, LightRange.Bright);
            var _rooms = new HashSet<Guid>(_Map.Rooms.Where(_r => _r.ContainsCell(_CellLoc)).Select(_r => _r.ID));

            foreach (var _mdl in from _room in _Map.Rooms.AsParallel()
                                 select _room.GenerateModel(_room.YieldEffects(_CellLoc, false, _visualizer)))
            {
                if (_mdl.Opaque != null)
                    grpRooms.Children.Add(_mdl.Opaque);
                if (_mdl.Alpha != null)
                    grpAlphaRooms.Children.Add(_mdl.Alpha);
            }

            foreach (var _grp in from _zone in _Map.ShadingZones.YieldShadeZoneEffects(_CellLoc, false, _visualizer).AsParallel()
                                 select _Map.ShadingZones.RenderShadingZoneEffects(_zone, _CellLoc, false, _visualizer))
            {
                if (_grp.Opaque != null)
                    grpRooms.Children.Add(_grp.Opaque);
                if (_grp.Alpha != null)
                    grpAlphaRooms.Children.Add(_grp.Alpha);
            }
            DrawingTools.DebugGroup = null;
        }
        #endregion

        #region private List<SensoryBase> FilteredSenses()
        private List<SensoryBase> FilteredSenses()
        {
            var _senses = new List<SensoryBase>();
            if ((chkVision.IsChecked ?? false) || (chkLowLight.IsChecked ?? false))
            {
                _senses.Add(new Vision(chkLowLight.IsChecked ?? false, typeof(Vision)));
            }
            if (chkDarkVision.IsChecked ?? false)
            {
                _senses.Add(new Darkvision(60, typeof(Darkvision)));
            }
            if (chkBlindSight.IsChecked ?? false)
            {
                _senses.Add(new BlindSight(60, true, typeof(BlindSight)));
            }
            return _senses;
        }
        #endregion

        #region private void RedrawLocators(List<SensoryBase> filteredSense)
        private void RedrawLocators(List<SensoryBase> filteredSense)
        {
            List<SensoryBase> _senses = filteredSense ?? FilteredSenses();
            grpTokens.Children.Clear();
            grpIcons.Children.Clear();
            foreach (var _loc in _Map.MapContext.AllTokensOf<ObjectPresenter>())
            {
                var _presentable = _loc.GetPresentable(_CellLoc, null, null, this, _Heading, AnchorFace.ZLow, _senses);
                if (_presentable.Presentations.Any(_p => _p is IconPresentation))
                {
                    grpIcons.Children.Add(_presentable.Model3D);
                }
                else
                {
                    grpTokens.Children.Add(_presentable.Model3D);
                }
            }
        }
        #endregion

        private void chkSense_Checked(object sender, RoutedEventArgs e)
        {
            RedrawAll();
        }

        #region IAwarenessLevels Members
        public AwarenessLevel GetAwarenessLevel(Guid guid) { return AwarenessLevel.Aware; }
        public bool ShouldDraw(Guid guid) { return true; }
        #endregion

        private void btnViewport_Click(object sender, RoutedEventArgs e)
        {
            if (_Map != null)
            {
                var _viewDlg = new ViewportEdit(_Map.BackgroundViewport);
                var _rslt = _viewDlg.ShowDialog();
                if (_rslt ?? false)
                {
                    _Map.BackgroundViewport = _viewDlg.GetCubic();
                    txtXPos.Text = _Map.BackgroundViewport.X.ToString();
                    txtYPos.Text = _Map.BackgroundViewport.Y.ToString();
                    txtZPos.Text = _Map.BackgroundViewport.Z.ToString();
                    txtXSize.Text = _Map.BackgroundViewport.XLength.ToString();
                    txtYSize.Text = _Map.BackgroundViewport.YLength.ToString();
                    txtZSize.Text = _Map.BackgroundViewport.ZHeight.ToString();
                    UpdateLighting();
                }
            }
        }

        #region IZoomIcons Members

        public double ZoomLevel
        {
            get
            {
                return 1d;
            }
            set
            {
            }
        }

        public double UnZoomLevel
        {
            get
            {
                return 0.3d;
            }
            set
            {
            }
        }

        public Guid ZoomedIcon
        {
            get
            {
                return Guid.Empty;
            }
            set
            {
            }
        }

        #endregion
    }
}
