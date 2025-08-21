using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Uzi.Ikosa.Tactical;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Senses;
using Uzi.Core;
using System.Diagnostics;
using System.Windows.Media.Animation;
using Uzi.Visualize;
using Uzi.Ikosa.UI;
using HelixToolkit.Wpf;
using Uzi.Ikosa.Creatures.Templates;
using Uzi.Ikosa.Fidelity;
using System.Windows.Controls.Ribbon;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa.Workshop.Locale
{
    /// <summary>Interaction logic for Preview.xaml</summary>
    public partial class Preview : RibbonWindow,
        IAwarenessLevels, ICellStructureProvider, IZoomIcons, IPresentationInputBinder
    {
        #region Routed Commands
        // move
        public static RoutedCommand CameraUp = new RoutedCommand();
        public static RoutedCommand CameraDown = new RoutedCommand();
        public static RoutedCommand CameraMove = new RoutedCommand();

        // turn
        public static RoutedCommand CameraRollUpLeft = new RoutedCommand();
        public static RoutedCommand CameraRollUpRight = new RoutedCommand();
        public static RoutedCommand CameraPivotLeft = new RoutedCommand();
        public static RoutedCommand CameraPivotRight = new RoutedCommand();
        public static RoutedCommand CameraTiltForward = new RoutedCommand();
        public static RoutedCommand CameraTiltBackward = new RoutedCommand();
        public static RoutedCommand CameraLevel = new RoutedCommand();

        // anchor swing
        public static RoutedCommand CameraSwingLeft = new RoutedCommand();
        public static RoutedCommand CameraSwingRight = new RoutedCommand();
        public static RoutedCommand CameraSwingUpward = new RoutedCommand();
        public static RoutedCommand CameraSwingDownward = new RoutedCommand();

        // cursor-camera distance
        public static RoutedCommand ShowCursor = new RoutedCommand();
        public static RoutedCommand PullCamera = new RoutedCommand();
        public static RoutedCommand PushCamera = new RoutedCommand();
        public static RoutedCommand PullCursor = new RoutedCommand();
        public static RoutedCommand PushCursor = new RoutedCommand();

        // general commands
        public static RoutedCommand NewThing = new RoutedCommand();
        public static RoutedCommand DeleteThing = new RoutedCommand();
        public static RoutedCommand DuplicateThing = new RoutedCommand();
        public static RoutedCommand ViewportEdit = new RoutedCommand();
        public static RoutedCommand CopyThing = new RoutedCommand();
        public static RoutedCommand PasteThing = new RoutedCommand();

        // cell commands
        public static RoutedCommand EditParams = new RoutedCommand();
        public static RoutedCommand DrawCell = new RoutedCommand();
        public static RoutedCommand FlipCell = new RoutedCommand();
        public static RoutedCommand SwapCell = new RoutedCommand();

        // room commands
        public static RoutedCommand TweakRoom = new RoutedCommand();
        public static RoutedCommand SizeRoom = new RoutedCommand();
        public static RoutedCommand BumpRoom = new RoutedCommand();
        public static RoutedCommand FlipRoom = new RoutedCommand();
        public static RoutedCommand SwapRoom = new RoutedCommand();
        public static RoutedCommand SplitRoom = new RoutedCommand();

        // locator commands
        public static RoutedCommand BumpLocator = new RoutedCommand();
        public static RoutedCommand TweakLocator = new RoutedCommand();
        public static RoutedCommand TweakObjects = new RoutedCommand();
        public static RoutedCommand PickModel = new RoutedCommand();
        public static RoutedCommand TurnObject = new RoutedCommand();
        public static RoutedCommand TiltObject = new RoutedCommand();
        public static RoutedCommand TwistObject = new RoutedCommand();
        public static RoutedCommand ZoomIcon = new RoutedCommand();
        public static RoutedCommand DrawObject = new RoutedCommand();
        public static RoutedCommand ReIndex = new RoutedCommand();

        // creature
        public static RoutedCommand Skeletonize = new RoutedCommand();
        public static RoutedCommand Zombify = new RoutedCommand();
        public static RoutedCommand Ghoulify = new RoutedCommand();
        public static RoutedCommand MakeCelestial = new RoutedCommand();
        public static RoutedCommand MakeFiendish = new RoutedCommand();

        // portal
        public static RoutedCommand OpenFace = new RoutedCommand();
        public static RoutedCommand CloseFace = new RoutedCommand();
        public static RoutedCommand SlideAxis = new RoutedCommand();
        public static RoutedCommand PortalOpenState = new RoutedCommand();
        #endregion

        public Preview()
        {
            InitializeComponent();
            _Visualization = new Visualization(this, grpTokens, grpRooms, grpAlphaRooms, grpTransients, viewport);
        }

        #region protected override void OnClosed(EventArgs e)
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (_Map != null)
            {
                _Map.MapChanged -= new EventHandler(_Map_NeedsRedraw);
            }
        }
        #endregion

        #region data
        private readonly Visualization _Visualization;
        private CellLocation _CellLoc = new CellLocation(1, 1, 1);
        private CellLocation _CursorLoc = new CellLocation(0, 2, 2);
        private ICellLocation _CursorPosition = null;
        private int _Heading = 1;
        private int _Incline = -1;
        private LocalMap _Map;
        private int _CursorStep = 2;
        private Room _CursorRoom = null;
        private Locator _CursorLocator = null;
        private double _Zoom = 1.0d;
        private double _UnZoom = 0.3d;
        private Guid _ZoomIcon = Guid.Empty;
        #endregion

        #region public void LoadMap(LocalMap localMap)
        public void LoadMap(LocalMap localMap)
        {
            if (_Map != null)
            {
                _Map.MapChanged -= new EventHandler(_Map_NeedsRedraw);
            }
            _Map = localMap;
            DataContext = _Map;
            _Map.MapChanged += new EventHandler(_Map_NeedsRedraw);
            UpdateLighting();
            chkVision.IsChecked = true;
            chkDarkVision.IsChecked = true;
            CreateModelMenu(localMap.Resources);
        }
        #endregion

        void _Map_NeedsRedraw(object sender, EventArgs e) { RedrawAll(); }

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
            var _senses = filteredSenses ?? FilteredSenses();
            var _cGroup = _Map.RoomIndex.GetRoom(_CellLoc);
            Func<Room, bool> _drawRoom = null;
            if (_cGroup != null)
            {
                var _myRooms = new RoomAwarenesses();
                var _ctx = new CellLocContext(_Map.MapContext, _CellLoc, _cGroup);
                _myRooms.RecalculateAwareness(_ctx, _senses);
                _drawRoom = (room) => _myRooms[room.ID];
            }
            else
            {
                var _range = Convert.ToDouble(galRenderRange.SelectedValue);
                _drawRoom = (room) => room.NearDistanceToCell((ICellLocation)_CellLoc) <= _range;
            }
            var _visualizer = new TerrainVisualizer(_senses, LightRange.Bright);
            var _global = new BuildableContext();
            _Visualization.SetTerrain((from _room in _Map.Rooms.Where(_r => _drawRoom(_r)).AsParallel()
                                       select _room.GenerateModel(_room.YieldEffects(_CellLoc, false, _visualizer))).
                                      Union(from _zone in _Map.ShadingZones.YieldShadeZoneEffects(_CellLoc, false, _visualizer).AsParallel()
                                            select _Map.ShadingZones.RenderShadingZoneEffects(_zone, _CellLoc, false, _visualizer)));

            var _effect = _Map.GetVisualEffect(_CellLoc, _CellLoc, _visualizer);
            var _tiling = _Map[_CellLoc].InnerBrush(_effect);
            rectInner.Fill = _tiling;
            rectInner.Visibility =
                ((chkCursor.IsChecked ?? false) || (chkCutAway.IsChecked ?? false) || (_tiling == null))
                ? Visibility.Hidden
                : Visibility.Visible;
        }
        #endregion

        #region private List<SensoryBase> FilteredSenses()
        private List<SensoryBase> FilteredSenses()
        {
            var _vision = (chkVision != null) && (chkVision.IsChecked ?? false);
            var _lowLight = (chkLowLight != null) && (chkLowLight.IsChecked ?? false);
            var _senses = new List<SensoryBase>();
            if (_vision || _lowLight)
            {
                _senses.Add(new Vision(_lowLight, typeof(Vision)));
            }
            if ((chkDarkVision != null) && (chkDarkVision.IsChecked ?? false))
            {
                var _range = Convert.ToDouble(galDVRange.SelectedValue);
                _senses.Add(new Darkvision(_range, typeof(Darkvision)));
            }
            if ((chkBlindSight != null) && (chkBlindSight.IsChecked ?? false))
            {
                var _range = Convert.ToDouble(galBSRange.SelectedValue);
                _senses.Add(new BlindSight(_range, true, typeof(BlindSight)));
            }
            return _senses;
        }
        #endregion

        #region private void RedrawLocators(List<SensoryBase> filteredSense)
        private void RedrawLocators(List<SensoryBase> filteredSenses)
        {
            var _senses = filteredSenses ?? FilteredSenses();
            var _range = Convert.ToDouble(galRenderRange.SelectedValue);
            var _serial = _Map.MapContext.SerialState;
            _Visualization.SetTokens(
                from _presenter in _Map.MapContext.AllTokensOf<ObjectPresenter>().Where(_op => _op.GeometricRegion.NearDistanceToCell((ICellLocation)_CellLoc) <= _range)
                select _presenter.GetPresentable(_CellLoc, null, null, this, _Heading, AnchorFace.ZLow, _senses), false, _serial);
            DrawCursors();
        }
        #endregion

        private void cbCamera_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void ShowCoords()
        {
            txtCoord.Text = string.Format(@"{0}, {1}, {2}", _CellLoc.Z, _CellLoc.Y, _CellLoc.X);
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
            ShowCoords();
            SetCursorLocation();
        }
        #endregion

        #region private void RedrawAll()
        private void RedrawAll()
        {
            SetCursorLocation();
            var _senses = FilteredSenses();
            DrawCursors();
            RedrawTerrain(_senses);
            RedrawLocators(_senses);
        }
        #endregion

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
                        cbDown.Command.Execute(null);
                        return;
                    case 1:
                        cbUp.Command.Execute(null);
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
            ShowCoords();
            SetCursorLocation();
            RedrawAll();
        }

        private void cbDown_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _newPos = povCamera.Position;
            _newPos.Offset(0, 0, -5);
            povCamera.Position = _newPos;
            _CellLoc = new CellLocation(_CellLoc.Z - 1, _CellLoc.Y, _CellLoc.X);
            ShowCoords();
            SetCursorLocation();
            RedrawAll();
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
            {
                _Incline = -2;
            }
            else
            {
                SetLook();
            }
        }

        private void cbTiltBackward_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _Incline++;
            if (_Incline > 2)
            {
                _Incline = 2;
            }
            else
            {
                SetLook();
            }
        }
        #endregion

        #region private void SetLook()
        private void SetLook()
        {
            var _rotTime = 100;
            ctrlAxes.Heading = _Heading;
            ctrlAxes.Incline = _Incline;
            if (_Incline == 2)
            {
                var _animate = new Vector3DAnimation(povCamera.LookDirection, new Vector3D(0, 0, 1),
                    new Duration(TimeSpan.FromMilliseconds(_rotTime)));
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
                    new Duration(TimeSpan.FromMilliseconds(_rotTime)));
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
                                new Duration(TimeSpan.FromMilliseconds(_rotTime)), FillBehavior.HoldEnd);
                            povCamera.BeginAnimation(PerspectiveCamera.LookDirectionProperty, _animate);
                        }
                        break;
                    case 1:
                        {
                            var _animate = new Vector3DAnimation(povCamera.LookDirection, new Vector3D(1, 1, _Incline),
                                new Duration(TimeSpan.FromMilliseconds(_rotTime)));
                            povCamera.BeginAnimation(PerspectiveCamera.LookDirectionProperty, _animate);
                        }
                        break;
                    case 2:
                        {
                            var _animate = new Vector3DAnimation(povCamera.LookDirection, new Vector3D(0, 1, _Incline),
                                new Duration(TimeSpan.FromMilliseconds(_rotTime)));
                            povCamera.BeginAnimation(PerspectiveCamera.LookDirectionProperty, _animate);
                        }
                        break;
                    case 3:
                        {
                            var _animate = new Vector3DAnimation(povCamera.LookDirection, new Vector3D(-1, 1, _Incline),
                                new Duration(TimeSpan.FromMilliseconds(_rotTime)));
                            povCamera.BeginAnimation(PerspectiveCamera.LookDirectionProperty, _animate);
                        }
                        break;
                    case 4:
                        {
                            var _animate = new Vector3DAnimation(povCamera.LookDirection, new Vector3D(-1, 0, _Incline),
                                new Duration(TimeSpan.FromMilliseconds(_rotTime)));
                            povCamera.BeginAnimation(PerspectiveCamera.LookDirectionProperty, _animate);
                        }
                        break;
                    case 5:
                        {
                            var _animate = new Vector3DAnimation(povCamera.LookDirection, new Vector3D(-1, -1, _Incline),
                                new Duration(TimeSpan.FromMilliseconds(_rotTime)));
                            povCamera.BeginAnimation(PerspectiveCamera.LookDirectionProperty, _animate);
                        }
                        break;
                    case 6:
                        {
                            var _animate = new Vector3DAnimation(povCamera.LookDirection, new Vector3D(0, -1, _Incline),
                                new Duration(TimeSpan.FromMilliseconds(_rotTime)));
                            povCamera.BeginAnimation(PerspectiveCamera.LookDirectionProperty, _animate);
                        }
                        break;
                    case 7:
                        {
                            var _animate = new Vector3DAnimation(povCamera.LookDirection, new Vector3D(1, -1, _Incline),
                                new Duration(TimeSpan.FromMilliseconds(_rotTime)));
                            povCamera.BeginAnimation(PerspectiveCamera.LookDirectionProperty, _animate);
                        }
                        break;
                }
            }
            SetCursorLocation();
            DrawCursors();
        }
        #endregion

        private void cbLevel_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO: reset heading and incline, recalculate look direction
            //povCamera.UpDirection = new Vector3D(0, 0, 1);
            //povCamera.LookDirection = new Vector3D(1, 0, 0);
        }

        private void chkSense_Checked(object sender, RoutedEventArgs e) { RedrawAll(); }

        #region IAwarenessLevels Members
        public AwarenessLevel GetAwarenessLevel(Guid guid) { return AwarenessLevel.Aware; }
        public bool ShouldDraw(Guid guid) { return true; }
        #endregion

        #region public void SetCursorLocation()
        private void SetCursorLocation()
        {
            _CursorLoc = new CellLocation(_CellLoc);
            var _offset = AnchorFaceHelper.MovementFaces(AnchorFace.ZLow, _Heading, _Incline)
                    .ToArray().GetAnchorOffset();
            for (var _step = 0; _step < _CursorStep; _step++)
            {
                _CursorLoc = _CursorLoc.Add(_offset);
            }

            _CursorPosition = new CellPosition(_CursorLoc);

            // try to rebind to last locator selected
            if ((CursorLocator != null) && (_Map != null))
            {
                _CursorLocator = _Map.MapContext.LocatorsInCell(_CursorLoc, PlanarPresence.Both).FirstOrDefault(_l => _l == _CursorLocator);
            }

            // if not, get first locator
            if ((CursorLocator == null) && (_Map != null))
            {
                _CursorLocator = _Map.MapContext.LocatorsInCell(_CursorLoc, PlanarPresence.Both).FirstOrDefault();
            }

            // last room selected still contains cell?
            // if not, get new room
            if (!(_CursorRoom?.ContainsCell(_CursorLoc) ?? false))
            {
                _CursorRoom = _Map?.RoomIndex.GetRoom(_CursorLoc);
            }

            // show coordinates
            if (txtCursor != null)
            {
                txtCursor.Text = $@"{_CursorLoc.Z}, {_CursorLoc.Y}, {_CursorLoc.X}";
            }
        }
        #endregion

        #region private Locator CursorLocator { get; }
        private Locator CursorLocator
        {
            get
            {
                if (_CursorLocator != null)
                {
                    if ((_Map == null) || !_Map.MapContext.AllTokensOf<Locator>().Contains(_CursorLocator))
                    {
                        _CursorLocator = null;
                    }
                }
                return _CursorLocator;
            }
        }
        #endregion

        #region private Room CursorRoom { get; }
        private Room CursorRoom
        {
            get
            {
                if (_CursorRoom != null)
                {
                    if ((_Map == null) || (_Map.Rooms.IndexOf(_CursorRoom) < 0))
                    {
                        _CursorRoom = null;
                    }
                }
                return _CursorRoom;
            }
        }
        #endregion

        #region private void DrawCursors()
        private void DrawCursors()
        {
            if (grpDrawing != null)
            {
                grpDrawing.Children.Clear();

                // observer location
                var _room = _Map.RoomIndex.GetRoom(_CellLoc);
                if (_room != null)
                {
                    txtRoomName.Text = _room.Name;
                    txtRoomCoord.Text = $@"{_CellLoc.Z - _room.LowerZ}, {_CellLoc.Y - _room.LowerY}, {_CellLoc.X - _room.LowerX}";
                }
                else
                {
                    // observer not in a room
                    txtRoomName.Text = string.Empty;
                    txtRoomCoord.Text = string.Empty;
                    txtCellType.Text = string.Empty;
                    txtCellParam.Text = string.Empty;
                }

                // cursor's room
                _room = CursorRoom;
                if (_room != null)
                {
                    var _rCursor = _room.Subtract(_CursorLoc);
                    txtCursorRoomName.Text = _room.Name;
                    txtCursorRoomCoord.Text = $@"{_rCursor.Z}, {_rCursor.Y}, {_rCursor.X}";
                    ref readonly var _struc = ref _Map[_CursorLoc];
                    txtCellType.Text = _struc.Description;
                    txtCellParam.Text = _struc.ParamText;
                    if (chkCursor.IsChecked ?? false)
                    {
                        #region outline the room
                        var _lx = _room.LowerX * 5d;
                        var _ly = _room.LowerY * 5d;
                        var _lz = _room.LowerZ * 5d;
                        var _ex = _room.XLength * 5d;
                        var _ey = _room.YLength * 5d;
                        var _ez = _room.ZHeight * 5d;
                        var _builder = new MeshBuilder();
                        _builder.AddBoundingBox(new Rect3D(_lx, _ly, _lz, _ex, _ey, _ez), 0.05d, 4);
                        grpDrawing.Children.Add(new GeometryModel3D(_builder.ToMesh(true), new DiffuseMaterial(_room.DeepShadows ? Brushes.Magenta : Brushes.Red)));
                        #endregion
                    }
                }
                else
                {
                    // cursor not in a room
                    txtCursorRoomName.Text = string.Empty;
                    txtCursorRoomCoord.Text = string.Empty;
                }

                // cursor's locator
                var _locator = CursorLocator;
                if (_locator != null)
                {
                    txtCursorLocatorName.Text = _locator.Name;
                    if (chkCursor.IsChecked ?? false)
                    {
                        #region outline the locator
                        var _rgn = _locator.GeometricRegion;
                        var _lx = _rgn.LowerX * 5d;
                        var _ly = _rgn.LowerY * 5d;
                        var _lz = _rgn.LowerZ * 5d;
                        var _ux = (_rgn.UpperX + 1) * 5d;
                        var _uy = (_rgn.UpperY + 1) * 5d;
                        var _uz = (_rgn.UpperZ + 1) * 5d;
                        var _ex = _ux - _lx;
                        var _ey = _uy - _ly;
                        var _ez = _uz - _lz;
                        var _builder = new MeshBuilder();
                        _builder.AddBoundingBox(new Rect3D(_lx, _ly, _lz, _ex, _ey, _ez), 0.05d, 4);
                        grpDrawing.Children.Add(new GeometryModel3D(_builder.ToMesh(true), new DiffuseMaterial(Brushes.Blue)));
                        #endregion
                    }
                }
                else
                {
                    txtCursorLocatorName.Text = string.Empty;
                }

                if (_locator is ObjectPresenter _op)
                {
                    rmnuModel.IsEnabled = true;
                    SyncModelMenu(_op);
                    if (_op.ICore is Furnishing _furnish)
                    {
                        txtCursorLocatorData.Text =
                            $"H: {_furnish.Orientation.Heading}, T: {_furnish.Orientation.Twist}, U:{_furnish.Orientation.Upright}\nZS:{(_furnish.Orientation.ZHighSnap ? 1 : 0)}, YS:{(_furnish.Orientation.YHighSnap ? 1 : 0)}, XS:{(_furnish.Orientation.XHighSnap ? 1 : 0)}";
                    }
                    else
                    {
                        txtCursorLocatorData.Text = string.Empty;
                    }
                }
                else
                {
                    rmnuModel.IsEnabled = false;
                    txtCursorLocatorData.Text = string.Empty;
                }

                // hide inner material tiling if cursoring or looking for cut--away
                rectInner.Visibility =
                    ((chkCursor.IsChecked ?? false) || (chkCutAway.IsChecked ?? false) || (rectInner.Fill == null))
                    ? Visibility.Hidden
                    : Visibility.Visible;

                #region near plane distance
                if ((chkCursor.IsChecked ?? false) && (chkCutAway.IsChecked ?? false))
                {
                    // cut-away mode
                    switch (_CursorStep)
                    {
                        case 1: povCamera.NearPlaneDistance = 0.1d; break;
                        case 2: povCamera.NearPlaneDistance = 2.5d; break;
                        case 3: povCamera.NearPlaneDistance = 7.5d; break;
                        case 4: povCamera.NearPlaneDistance = 12.5d; break;
                        case 5: povCamera.NearPlaneDistance = 17.5d; break;
                        case 6: povCamera.NearPlaneDistance = 22.5d; break;
                    }
                }
                else
                {
                    // normal mode
                    povCamera.NearPlaneDistance = 0.1d;
                }
                #endregion

                // cursor on
                if (chkCursor.IsChecked ?? false)
                {
                    grpDrawing.Children.Add(DrawingTools.CellGlow(_CursorLoc.Z, _CursorLoc.Y, _CursorLoc.X));
                }
            }
        }
        #endregion

        private void SyncModelMenu(ObjectPresenter presenter)
        {
            foreach (var _rmi in rmnuModel.Items.OfType<RibbonMenuItem>())
            {
                _rmi.IsChecked = presenter.ModelKey == _rmi.CommandParameter.ToString();
            }
        }

        #region private void CreateModelMenu(ObjectPresenter presenter)
        private void CreateModelMenu(IResolveModel3D resolveModel3D)
        {
            rmnuModel.Items.Clear();
            foreach (var _resolvable in resolveModel3D.ResolvableModels.OrderBy(_m => _m.Model3DPart.Name))
            {
                rmnuModel.Items.Add(new RibbonMenuItem
                {
                    IsChecked = false,
                    Command = PickModel,
                    CommandParameter = _resolvable.Model3DPart.Name,
                    Header = _resolvable.Model3DPart.Name,
                    VerticalContentAlignment = VerticalAlignment.Top,
                    HorizontalContentAlignment = HorizontalAlignment.Left
                });
            }
        }
        #endregion

        #region toggle cursor
        private void cbCursor_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cbCursor_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            chkCursor.IsChecked = !(chkCursor.IsChecked ?? false);
            e.Handled = true;
        }
        #endregion

        #region pull cursor
        private void cbPullCursor_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _CursorStep > 1;
            e.Handled = true;
        }

        private void cbPullCursor_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _CursorStep--;
            if (!(chkCursor.IsChecked ?? false))
            {
                chkCursor.IsChecked = true;
            }
            else
            {
                SetCursorLocation();
                DrawCursors();
            }
            e.Handled = true;
        }
        #endregion

        #region push cursor
        private void cbPushCursor_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _CursorStep < 6;
            e.Handled = true;
        }

        private void cbPushCursor_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _CursorStep++;
            if (!(chkCursor.IsChecked ?? false))
            {
                chkCursor.IsChecked = true;
            }
            else
            {
                SetCursorLocation();
                DrawCursors();
            }
            e.Handled = true;
        }
        #endregion

        #region swing camera right
        private void cbCameraSwingRight_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = chkCursor.IsChecked ?? false;
            e.Handled = true;
        }

        private void cbCameraSwingRight_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if ((_Heading % 2) == 0)
            {
                if (Math.Abs(_Incline) != 2)
                {
                    for (var _step = 0; _step < _CursorStep; _step++)
                    {
                        cbMove.Command.Execute(@"2");
                    }
                }

                cbPivotRight.Command.Execute(null);
            }
            else
            {
                cbPivotRight.Command.Execute(null);
                if (Math.Abs(_Incline) != 2)
                {
                    for (var _step = 0; _step < _CursorStep; _step++)
                    {
                        cbMove.Command.Execute(@"2");
                    }
                }
            }

            e.Handled = true;
        }
        #endregion

        #region swing camera left
        private void cbCameraSwingLeft_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = chkCursor.IsChecked ?? false;
            e.Handled = true;
        }

        private void cbCameraSwingLeft_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if ((_Heading % 2) == 0)
            {
                if (Math.Abs(_Incline) != 2)
                {
                    for (var _step = 0; _step < _CursorStep; _step++)
                    {
                        cbMove.Command.Execute(@"6");
                    }
                }

                cbPivotLeft.Command.Execute(null);
            }
            else
            {
                cbPivotLeft.Command.Execute(null);
                if (Math.Abs(_Incline) != 2)
                {
                    for (var _step = 0; _step < _CursorStep; _step++)
                    {
                        cbMove.Command.Execute(@"6");
                    }
                }
            }
            e.Handled = true;
        }
        #endregion

        #region swing camera upward
        private void cbCameraSwingUpward_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (_Incline > -2) && (chkCursor.IsChecked ?? false);
            e.Handled = true;
        }

        private void cbCameraSwingUpward_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            switch (_Incline)
            {
                case 2:
                    for (var _step = 0; _step < _CursorStep; _step++)
                    {
                        cbMove.Command.Execute(@"4");
                    }

                    cbTiltForward.Command.Execute(null);
                    break;
                case 1:
                case 0:
                    for (var _step = 0; _step < _CursorStep; _step++)
                    {
                        cbUp.Command.Execute(null);
                    }

                    cbTiltForward.Command.Execute(null);
                    break;
                case -1:
                    for (var _step = 0; _step < _CursorStep; _step++)
                    {
                        cbMove.Command.Execute(@"0");
                    }

                    cbTiltForward.Command.Execute(null);
                    break;
            }
            e.Handled = true;
        }
        #endregion

        #region swing camera downward
        private void cbCameraSwingDownward_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (_Incline < 2) && (chkCursor.IsChecked ?? false);
            e.Handled = true;
        }

        private void cbCameraSwingDownward_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            switch (_Incline)
            {
                case -2:
                    for (var _step = 0; _step < _CursorStep; _step++)
                    {
                        cbMove.Command.Execute(@"4");
                    }

                    cbTiltBackward.Command.Execute(null);
                    break;
                case -1:
                case 0:
                    for (var _step = 0; _step < _CursorStep; _step++)
                    {
                        cbDown.Command.Execute(null);
                    }

                    cbTiltBackward.Command.Execute(null);
                    break;
                case 1:
                    for (var _step = 0; _step < _CursorStep; _step++)
                    {
                        cbMove.Command.Execute(@"0");
                    }

                    cbTiltBackward.Command.Execute(null);
                    break;
            }
            e.Handled = true;
        }
        #endregion

        #region pull camera
        private void cbPullCamera_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (_CursorStep > 1) && (chkCursor.IsChecked ?? false);
            e.Handled = true;
        }

        private void cbPullCamera_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            cbPullCursor.Command.Execute(null);
            switch (_Incline)
            {
                case -2:
                    cbDown.Command.Execute(null);
                    break;
                case -1:
                    cbMove.Command.Execute(@"0");
                    cbDown.Command.Execute(null);
                    break;
                case 0:
                    cbMove.Command.Execute(@"0");
                    break;
                case 1:
                    cbUp.Command.Execute(null);
                    cbMove.Command.Execute(@"0");
                    break;
                case 2:
                    cbUp.Command.Execute(null);
                    break;
            }
            e.Handled = true;
        }
        #endregion

        #region push camera
        private void cbPushCamera_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (_CursorStep < 10) && (chkCursor.IsChecked ?? false);
            e.Handled = true;
        }

        private void cbPushCamera_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            cbPushCursor.Command.Execute(null);
            switch (_Incline)
            {
                case -2:
                    cbUp.Command.Execute(null);
                    break;
                case -1:
                    cbMove.Command.Execute(@"4");
                    cbUp.Command.Execute(null);
                    break;
                case 0:
                    cbMove.Command.Execute(@"4");
                    break;
                case 1:
                    cbDown.Command.Execute(null);
                    cbMove.Command.Execute(@"4");
                    break;
                case 2:
                    cbDown.Command.Execute(null);
                    break;
            }
            e.Handled = true;
        }
        #endregion

        #region edit Params at Cursor
        private void cbEditParams_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            if ((chkCursor.IsChecked ?? false) && (_Map != null))
            {
                // must be in a room
                if (CursorRoom != null)
                {
                    // must have a cell space with param capability
                    if (!((_Map[_CursorLoc].CellSpace)?.GetType().Equals(typeof(CellSpace)) ?? true))
                    {
                        e.CanExecute = true;
                    }
                }
            }
            e.Handled = true;
        }

        private void cbEditParams_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // must be in a room
            var _room = CursorRoom;
            if (_room != null)
            {
                // must have a cell space with param capability
                ref readonly var _cellStruc = ref _Map[_CursorLoc];
                var _dlg = new ParamEdit(_cellStruc.CellSpace, _cellStruc.ParamData)
                {
                    Owner = GetWindow(this)
                };
                if (_dlg.ShowDialog() ?? false)
                {
                    var _z = (_CursorLoc.Z - _room.LowerZ);
                    var _y = (_CursorLoc.Y - _room.LowerY);
                    var _x = (_CursorLoc.X - _room.LowerX);
                    _room[_z, _y, _x] = new CellStructure(_cellStruc.CellSpace, _dlg.ParamData);
                }
            }
            e.Handled = true;
        }
        #endregion

        #region cbDrawCell
        private void cbDrawCell_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cbDrawCell_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is string)
            {
                var _str = e.Parameter.ToString();
                if (_str.Length > 0)
                {
                    var _keep = _str[0] == 'K';
                    if (_keep)
                    {
                        _str = _str.Substring(1);
                    }

                    if (_CellPalette != null)
                    {
                        var _csp = _CellPalette.gridPlaceHolder.Children[0] as CellSpacePalette;
                        DrawCell.Execute(_csp.CommandParameter(Convert.ToInt32(_str), _keep), this);
                    }
                }
            }
            else
            {
                var _t = e.Parameter as Tuple<ContentControl, ContentControl>;
                ContentControl _ccCellSpace = _t.Item1;
                ContentControl _ccParams = _t.Item2;
                if ((chkCursor.IsChecked ?? false) && (_ccCellSpace != null))
                {
                    var _room = CursorRoom;
                    if ((_room != null) && (_ccCellSpace.Content != null))
                    {
                        var _rCursor = _room.Subtract(_CursorLoc);
                        _room[_rCursor.Z, _rCursor.Y, _rCursor.X]
                            = new CellStructure(
                                _ccCellSpace.Content as CellSpace,
                                (_ccParams?.Content != null)
                                ? (_ccParams?.Content as IParamControl).ParamData
                                : _room[_rCursor.Z, _rCursor.Y, _rCursor.X].ParamData);
                    }
                }
            }
            e.Handled = true;
        }
        #endregion

        public bool CanCaptureCellStructure()
        {
            return (_Map != null) && (chkCursor.IsChecked ?? false);
        }

        public ref readonly CellStructure GetCellStructure()
        {
            ref readonly var _struc = ref _Map[_CursorPosition];
            return ref _struc;
        }

        #region private bool InRoom()
        private bool InRoom()
        {
            if ((chkCursor.IsChecked ?? false) && (_Map != null))
            {
                var _room = CursorRoom;
                if (_room != null)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region private void MustBeInRoom(object sender, CanExecuteRoutedEventArgs e)
        private void MustBeInRoom(object sender, CanExecuteRoutedEventArgs e)
        {
            // must be in a room
            e.CanExecute = InRoom();
            e.Handled = true;
        }
        #endregion

        #region private void MustHaveLocator(object sender, CanExecuteRoutedEventArgs e)
        private void MustHaveLocator(object sender, CanExecuteRoutedEventArgs e)
        {
            // must be in a room
            if ((chkCursor.IsChecked ?? false) && (_Map != null))
            {
                var _loc = CursorLocator;
                if (_loc != null)
                {
                    e.CanExecute = true;
                }
            }
            e.Handled = true;
        }
        #endregion

        #region private void cbTweakRoom_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cbTweakRoom_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Room _room;

            // must be in a room
            if (e.Parameter is Room)
            {
                _room = e.Parameter as Room;
            }
            else
            {
                _room = CursorRoom;
            }

            if (_room != null)
            {
                // show dialog
                var _oDlg = new ObjectEditorWindow(_room)
                {
                    Title = @"Tweak Room",
                    Owner = GetWindow(this),
                    WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner
                };
                _oDlg.ShowDialog();
            }
            e.Handled = true;
        }
        #endregion

        // multi-use

        #region cbNew
        private void cbNew_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter is Type _type)
            {
                if (_type == typeof(Room))
                {
                    // must not be in a room
                    if (_Map != null)
                    {
                        var _room = CursorRoom;
                        if (_room == null)
                        {
                            e.CanExecute = true;
                        }
                    }
                }
                else if (_type == typeof(Locator))
                {
                    if (_Map != null)
                    {
                        // must not be in a locator
                        var _locator = CursorLocator;
                        if (_locator == null)
                        {
                            e.CanExecute = true;
                        }
                    }
                }
                else if (_type == typeof(CellSpace))
                {
                    // TODO:
                }
            }
            else if (e.Parameter is string)
            {
                if (_Map != null)
                {
                    // must not be in a locator
                    var _locator = CursorLocator;
                    if (_locator == null)
                    {
                        e.CanExecute = true;
                    }
                }
            }
            e.Handled = true;
        }

        private void cbNew_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is Type _type)
            {
                if (_type == typeof(Room))
                {
                    // must not be in a room
                    if (_Map != null)
                    {
                        var _room = CursorRoom;
                        if (_room == null)
                        {
                            var _rDlg = new NewRoom()
                            {
                                Owner = GetWindow(this)
                            };
                            if (_rDlg.ShowDialog() ?? false)
                            {
                                var _newRoom = new Room(@"New Room", _CursorLoc, new GeometricSize(1, 1, 1), _Map, false, _rDlg.IsEnclosed);
                                _Map.Rooms.Add(_newRoom);
                                _newRoom.ReLink(true);
                                RedrawAll();
                                TweakRoom.Execute(_newRoom, this);
                            }
                        }
                    }
                }
                else if (_type == typeof(Locator))
                {
                    if (_Map != null)
                    {
                        // must not be in a locator
                        var _locator = CursorLocator;
                        if (_locator == null)
                        {
                            var _lDlg = new LocatorCreate(_Map, _CursorLoc)
                            {
                                Owner = GetWindow(this)
                            };
                            if (_lDlg.ShowDialog() ?? false)
                            {
                                _lDlg.GetLocator(_Map.MapContext);
                            }
                        }
                    }
                }
                else if (_type == typeof(CellSpace))
                {
                    // TODO:
                }
            }
            else if (e.Parameter is string _param)
            {
                if (_param.StartsWith(@"Creature."))
                {
                    var _model = $@"{_param.Split('.')[1]}1";
                    Species _species = null;
                    switch (_param)
                    {
                        case @"Creature.Goblin":
                            _species = new Goblin();
                            break;
                        case @"Creature.Hobgoblin":
                            _species = new Hobgoblin();
                            break;
                        case @"Creature.Kobold":
                            _species = new Kobold();
                            break;
                        case @"Creature.MonstrousSpider":
                            _species = new MonstrousSpider();
                            break;
                        case @"Creature.MonstrousCentipede":
                            _species = new MonstrousCentipede();
                            break;
                    }
                    if (_species != null)
                    {
                        var _abilities = _species.DefaultAbilities();
                        var _critter = new Creature(_param.Split('.')[1], _abilities);
                        _species.BindTo(_critter);
                        _critter.Devotion = new Devotion(@"Nature");
                        var _size = new GeometricSize(1, 1, 1);
                        var _cube = new Cubic(_CursorLoc, _size);
                        _ = new ObjectPresenter(_critter, _Map.MapContext, _model, _size, _cube);
                    }
                }
                else
                {
                    switch (_param)
                    {
                        case @"Wood Slider":
                        case @"Wood Door":
                        case @"Secret Wood Slider":
                        case @"Secret Wood":
                        case @"Iron Slider":
                        case @"Iron Door":
                        case @"Secret Iron Slider":
                        case @"Secret Iron":
                        case @"Stone Door Slider":
                        case @"Stone Door":
                        case @"Secret Stone Slider":
                        case @"Secret Stone":
                        case @"Bars Slider":
                        case @"Bars":
                            {
                                #region new portals
                                PortalledObjectBase _pObjA = null;
                                PortalledObjectBase _pObjB = null;
                                var _thick = 0.125d;
                                var _width = 5d;
                                var _height = 8.5d;
                                var _weight = 20d;
                                var _struct = 15;
                                var _key = string.Empty; ;
                                var _size = new GeometricSize(2, 1, 1);
                                var _cube = new Cubic(_CursorLoc, _size);
                                switch (_param)
                                {
                                    case @"Wood Slider":
                                    case @"Wood Door":
                                    case @"Secret Wood Slider":
                                    case @"Secret Wood":
                                        {
                                            _pObjA = new Door(@"Wood Door", WoodMaterial.Static, _thick / 2);
                                            _pObjB = new Door(@"Wood Door", WoodMaterial.Static, _thick / 2);
                                            _key = @"Door-Wood";
                                        }
                                        break;

                                    case @"Iron Slider":
                                    case @"Iron Door":
                                    case @"Secret Iron Slider":
                                    case @"Secret Iron":
                                        {
                                            _pObjA = new Door(@"Iron Door", IronMaterial.Static, _thick / 2);
                                            _pObjB = new Door(@"Iron Door", IronMaterial.Static, _thick / 2);
                                            _key = @"Door-Iron";
                                            _weight = 200d;
                                            _thick = 0.175d;
                                            _struct = 60;
                                        }
                                        break;

                                    case @"Stone Door Slider":
                                    case @"Stone Door":
                                    case @"Secret Stone Slider":
                                    case @"Secret Stone":
                                        {
                                            _pObjA = new Door(@"Stone Door", StoneMaterial.Static, _thick / 2);
                                            _pObjB = new Door(@"Stone Door", StoneMaterial.Static, _thick / 2);
                                            _key = @"Door-Stone";
                                            _weight = 200d;
                                            _thick = 0.25d;
                                            _struct = 60;
                                        }
                                        break;

                                    case @"Bars Slider":
                                    case @"Bars":
                                        {
                                            _pObjA = new Bars(@"Bars", IronMaterial.Static, _thick / 2);
                                            _pObjB = new Bars(@"Bars", IronMaterial.Static, _thick / 2);
                                            _key = @"bars_8.xaml";
                                            _struct = 60;
                                        }
                                        break;
                                }

                                var _slide = _param.EndsWith(@"Slider");
                                if (_slide)
                                {
                                    _height = 10d;
                                }

                                _pObjA.Width = _width;
                                _pObjA.Height = _height;
                                _pObjA.Thickness = _thick / 2;
                                _pObjA.TareWeight = _weight / 2;
                                _pObjA.MaxStructurePoints = _struct;
                                _pObjB.Width = _width;
                                _pObjB.Height = _height;
                                _pObjB.Thickness = _thick / 2;
                                _pObjB.TareWeight = _weight / 2;
                                _pObjB.MaxStructurePoints = _struct;

                                if (_slide)
                                {
                                    if (_param.StartsWith(@"Secret"))
                                    {
                                        var _portal = new SecretSlidingPortal(_pObjA.Name, AnchorFace.YLow, Axis.X, _width, 0, 0, 0, _pObjA, _pObjB);
                                        _ = new ObjectPresenter(_portal, _Map.MapContext, _key, _size, _cube);
                                    }
                                    else
                                    {
                                        var _portal = new SlidingPortal(_pObjA.Name, AnchorFace.YLow, Axis.X, _width, 0, 0, 0, _pObjA, _pObjB);
                                        _ = new ObjectPresenter(_portal, _Map.MapContext, _key, _size, _cube);
                                    }
                                }
                                else
                                {
                                    if (_param.StartsWith(@"Secret"))
                                    {
                                        var _portal = new SecretCornerPivotPortal(_pObjA.Name, AnchorFace.YLow, AnchorFace.XLow, _pObjA, _pObjB);
                                        _ = new ObjectPresenter(_portal, _Map.MapContext, _key, _size, _cube);
                                    }
                                    else
                                    {
                                        var _portal = new CornerPivotPortal(_pObjA.Name, AnchorFace.YLow, AnchorFace.XLow, _pObjA, _pObjB);
                                        _ = new ObjectPresenter(_portal, _Map.MapContext, _key, _size, _cube);
                                    }
                                }
                                #endregion
                            }
                            break;

                        case @"Mechanism Mount":
                            {
                                var _mount = new MechanismMount(@"Mech Mount");
                                var _size = _mount.GeometricSize;
                                var _cube = new Cubic(_CursorLoc, _size);
                                _ = new ObjectPresenter(_mount, _Map.MapContext, @"mech_mount", _size, _cube);
                            }
                            break;

                        case @"Cart":
                            {
                                var _key = string.Empty;
                                var _cube = new Cubic(_CursorLoc, new GeometricSize(1, 1, 1));
                                Conveyance _convey;
                                switch (_param)
                                {
                                    case @"Cart":
                                    default:
                                        _convey = new Cart(@"Cart", WoodMaterial.Static)
                                        {
                                            Width = 2,
                                            Height = 2.5,
                                            Length = 2.5,
                                            TareWeight = 50,
                                            MaxStructurePoints = 20
                                        };
                                        _key = @"Cart";
                                        break;
                                }
                                if (_convey != null)
                                {
                                    _ = new ObjectPresenter(_convey, _Map.MapContext, _key, new GeometricSize(1, 1, 1), _cube);
                                }
                            }
                            break;

                        default:
                            {
                                Furnishing _furnish = null;
                                var _key = string.Empty;
                                var _name = string.Empty;
                                var _size = new GeometricSize(1, 1, 1);
                                var _cube = new Cubic(_CursorLoc, _size);
                                switch (_param)
                                {
                                    // flexible flat objects
                                    case @"FlexFlat":
                                        _furnish = new FlexibleFlatPanel(@"Tarp",
                                            new FlatObjectSide(@"Front", ClothMaterial.Static, 0.05),
                                            new FlatObjectSide(@"Back", ClothMaterial.Static, 0.05))
                                        {
                                            Width = 5,
                                            Length = 0.1,
                                            Height = 5,
                                            TareWeight = 2,
                                            MaxStructurePoints = 10,
                                            FlexState = FlexibleFlatState.Flat
                                        };
                                        _key = @"block-text.xaml";
                                        break;

                                    // flat object
                                    case @"FlatPanel":
                                        _furnish = new FlatPanel(@"Panel",
                                            new FlatObjectSide(@"Front", WoodMaterial.Static, 0.05),
                                            new FlatObjectSide(@"Back", WoodMaterial.Static, 0.05))
                                        {
                                            Width = 5,
                                            Length = 0.1,
                                            Height = 5,
                                            TareWeight = 2,
                                            MaxStructurePoints = 10
                                        };
                                        _key = @"block-test.xaml";
                                        break;

                                    // surfaces
                                    case @"Table":
                                        _furnish = new Table(WoodMaterial.Static)
                                        {
                                            Width = 4,
                                            Height = 3,
                                            Length = 4,
                                            TareWeight = 30,
                                            MaxStructurePoints = 10
                                        };
                                        _key = @"Table-Wood";
                                        break;
                                    case @"Bench":
                                        _furnish = new Bench(WoodMaterial.Static)
                                        {
                                            Width = 4,
                                            Length = 1.5,
                                            Height = 1.5,
                                            TareWeight = 15,
                                            MaxStructurePoints = 10
                                        };
                                        _key = @"Table-Wood";
                                        break;
                                    case @"Stool":
                                        _furnish = new Stool(WoodMaterial.Static)
                                        {
                                            Width = 1.5,
                                            Length = 1.5,
                                            Height = 1.5,
                                            TareWeight = 5,
                                            MaxStructurePoints = 5
                                        };
                                        _key = @"Table-Wood";
                                        break;

                                    // storage
                                    case @"Cabinet":
                                        _furnish = new Cabinet(WoodMaterial.Static)
                                        {
                                            Width = 2.5,
                                            Length = 1.5,
                                            Height = 4,
                                            TareWeight = 20,
                                            MaxStructurePoints = 10
                                        };
                                        _key = @"cabinet_2_3.xaml";
                                        break;
                                    case @"Shelves":
                                        _furnish = new StandingShelves(WoodMaterial.Static, true, true)
                                        {
                                            Width = 2.5,
                                            Length = 1.5,
                                            Height = 4,
                                            TareWeight = 10,
                                            MaxStructurePoints = 5
                                        };
                                        _key = @"bookcase_4.xaml";
                                        break;
                                    case @"Drawers":
                                        _furnish = new Drawers(WoodMaterial.Static, 3)
                                        {
                                            Width = 2.5,
                                            Length = 1.5,
                                            Height = 3,
                                            TareWeight = 10,
                                            MaxStructurePoints = 10
                                        };
                                        _key = @"drawers_3";
                                        break;

                                    // solids
                                    case @"Statue":
                                        break;
                                    case @"Anvil":
                                        break;
                                    case @"Lectern":
                                        break;

                                    // hollow
                                    case @"Barrel":
                                        {
                                            _furnish = new Barrel(WoodMaterial.Static)
                                            {
                                                Width = 2,
                                                Height = 3,
                                                Length = 2,
                                                TareWeight = 10,
                                                MaxStructurePoints = 10,
                                                LiddedModelKey = @"Barrel-Closed"
                                            };
                                            var _lid = new BarrelLid(WoodMaterial.Static)
                                            {
                                                Width = 2,
                                                Length = 2,
                                                Height = 0.0825,
                                                TareWeight = 1,
                                                MaxStructurePoints = 5
                                            };
                                            _lid.BindToObject(_furnish);
                                            _key = @"Barrel-Open";
                                        }
                                        break;

                                    case @"Barrel Lid":
                                        _furnish = new BarrelLid(WoodMaterial.Static)
                                        {
                                            Width = 2,
                                            Length = 2,
                                            Height = 0.0825,
                                            TareWeight = 1,
                                            MaxStructurePoints = 5
                                        };
                                        _key = @"Barrel-Open";
                                        break;

                                    case @"Crate":
                                        {
                                            _furnish = new Crate(WoodMaterial.Static)
                                            {
                                                Width = 2,
                                                Height = 2,
                                                Length = 2,
                                                TareWeight = 10,
                                                MaxStructurePoints = 10,
                                                LiddedModelKey = @"Crate-Closed"
                                            };
                                            var _lid = new CrateLid(WoodMaterial.Static)
                                            {
                                                Width = 2,
                                                Length = 2,
                                                Height = 0.0825,
                                                TareWeight = 1,
                                                MaxStructurePoints = 5
                                            };
                                            _lid.BindToObject(_furnish);
                                            _key = @"Crate-Open";
                                        }
                                        break;

                                    case @"Crate Lid":
                                        _furnish = new CrateLid(WoodMaterial.Static)
                                        {
                                            Width = 2,
                                            Length = 2,
                                            Height = 0.0825,
                                            TareWeight = 1,
                                            MaxStructurePoints = 5
                                        };
                                        _key = @"Crate-Open";
                                        break;

                                    // other (mid-surface)
                                    case @"Bed":
                                        break;
                                    case @"Chair":
                                        _furnish = new Chair(WoodMaterial.Static)
                                        {
                                            Width = 1.5,
                                            Length = 1.5,
                                            Height = 3,
                                            TareWeight = 10,
                                            MaxStructurePoints = 10
                                        };
                                        _key = @"chair_fullback.xaml";
                                        break;
                                    case @"Desk":
                                        break;
                                }
                                if (_furnish != null)
                                {
                                    _ = new ObjectPresenter(_furnish, _Map.MapContext, _key, _size, _cube);
                                }
                            }
                            break;
                    }
                }
            }

            e.Handled = true;
        }
        #endregion

        #region cbDelete
        private void cbDelete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter is Type _type)
            {
                if (_type == typeof(Room))
                {
                    MustBeInRoom(sender, e);
                }
                else if (_type == typeof(Locator))
                {
                    MustHaveLocator(sender, e);
                }
            }
            e.Handled = true;
        }

        private void cbDelete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is Type _type)
            {
                if (_type == typeof(Room))
                {
                    if (_Map != null)
                    {
                        var _room = CursorRoom;
                        if (_room != null)
                        {
                            _Map.Rooms.Remove(_room);
                        }
                    }
                }
                else if (_type == typeof(Locator))
                {
                    if (_Map != null)
                    {
                        var _loc = CursorLocator;
                        if (_loc != null)
                        {
                            _Map.MapContext.Remove(_loc);
                        }
                    }
                }
            }
            e.Handled = true;
        }
        #endregion

        #region cbDuplicate
        private void cbDuplicate_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter is Type _type)
            {
                if (_type == typeof(Room))
                {
                    MustBeInRoom(sender, e);
                }
            }
            e.Handled = true;
        }

        private void cbDuplicate_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is Type _type)
            {
                if (_type == typeof(Room))
                {
                    // must be in a room
                    if (_Map != null)
                    {
                        var _room = CursorRoom;
                        if (_room != null)
                        {
                            var _newRoom = new Room(@"Duplicate", _room, _room, _Map, _room.DeepShadows, !_room.IsPartOfBackground);
                            foreach (var _cLoc in _room.AllCellLocations())
                            {
                                var _cStruc = _room[_cLoc.Z - _room.Z, _cLoc.Y - _room.Y, _cLoc.X - _room.X];
                                _newRoom.SetCellStructure(_cLoc.Z - _room.Z, _cLoc.Y - _room.Y, _cLoc.X - _room.X,
                                    new CellStructure(_cStruc.CellSpace.Template, _cStruc.ParamData));
                            }
                            var _index = _Map.Rooms.IndexOf(_room);
                            if (_index >= 0)
                            {
                                _Map.Rooms.Insert(_index, _newRoom);
                            }
                            else
                            {
                                _Map.Rooms.Add(_newRoom);
                            }

                            _newRoom.ReLink(true);
                        }
                    }
                }
                else
                {
                }
            }
        }
        #endregion

        #region cbCopy
        private Room _CopyRoom = null;
        private ICellLocation _CopyRoomLoc = null;

        private void cbCopy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter is Type _type)
            {
                if (_type == typeof(Room))
                {
                    MustBeInRoom(sender, e);
                }
            }
            e.Handled = true;
        }

        private void cbCopy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is Type _type)
            {
                if (_type == typeof(Room))
                {
                    // must be in a room
                    if (_Map != null)
                    {
                        var _room = CursorRoom;
                        if (_room != null)
                        {
                            _CopyRoom = _room;
                            _CopyRoomLoc = _room.Subtract(_CursorLoc);
                        }
                    }
                }
                else
                {
                }
            }
        }
        #endregion

        #region cbPaste
        private void cbPaste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter is Type _type)
            {
                if (_type == typeof(Room))
                {
                    if (_Map != null)
                    {
                        var _room = CursorRoom;
                        if (_room == null)
                        {
                            e.CanExecute = (_CopyRoom != null);
                        }
                    }
                }
            }
            e.Handled = true;
        }

        private void cbPaste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is Type _type)
            {
                if (_type == typeof(Room))
                {
                    var _newRoom = new Room(@"Duplicate", _CopyRoomLoc.Subtract(_CursorLoc), _CopyRoom, _Map, _CopyRoom.DeepShadows, !_CopyRoom.IsPartOfBackground);
                    foreach (var _cLoc in _CopyRoom.AllCellLocations())
                    {

                        var _cStruc = _CopyRoom[_cLoc.Z - _CopyRoom.Z, _cLoc.Y - _CopyRoom.Y, _cLoc.X - _CopyRoom.X];
                        _newRoom.SetCellStructure(_cLoc.Z - _CopyRoom.Z, _cLoc.Y - _CopyRoom.Y, _cLoc.X - _CopyRoom.X,
                            new CellStructure(_cStruc.CellSpace?.Template, _cStruc.ParamData));
                    }
                    _Map.Rooms.Add(_newRoom);
                    _newRoom.ReLink(true);
                }
            }
        }
        #endregion

        // Rooms

        #region cbBumpRoom
        private void cbBumpRoom_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            MustBeInRoom(sender, e);
            e.Handled = true;
        }

        private void cbBumpRoom_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // must be in a room
            if (_Map != null)
            {
                var _room = CursorRoom;
                if (_room != null)
                {
                    string _mvHdng(int _hdng) => (_hdng - _Heading).ToString();

                    switch (e.Parameter.ToString())
                    {
                        case @"Z-":
                            _room.BindableZ -= 1;
                            cbDown.Command.Execute(null);
                            break;

                        case @"Z+":
                            _room.BindableZ += 1;
                            cbUp.Command.Execute(null);
                            break;

                        case @"Y-":
                            _room.BindableY -= 1;
                            cbMove.Command.Execute(_mvHdng(6));
                            break;

                        case @"Y+":
                            _room.BindableY += 1;
                            cbMove.Command.Execute(_mvHdng(2));
                            break;

                        case @"X-":
                            _room.BindableX -= 1;
                            cbMove.Command.Execute(_mvHdng(4));
                            break;

                        case @"X+":
                            _room.BindableX += 1;
                            cbMove.Command.Execute(_mvHdng(0));
                            break;
                    }
                }
            }
        }
        #endregion

        #region cbFlipRoom
        private void cbFlipRoom_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            MustBeInRoom(sender, e);
            e.Handled = true;
        }

        private void cbFlipRoom_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // must be in a room
            if (_Map != null)
            {
                var _room = CursorRoom;
                if (_room != null)
                {
                    switch (e.Parameter.ToString())
                    {
                        case @"Z":
                            _room.Flip(Axis.Z);
                            break;

                        case @"Y":
                            _room.Flip(Axis.Y);
                            break;

                        case @"X":
                            _room.Flip(Axis.X);
                            break;
                    }
                }
            }
        }
        #endregion

        #region cbSwapRoom
        private void cbSwapRoom_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            MustBeInRoom(sender, e);
            e.Handled = true;
        }

        private void cbSwapRoom_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // must be in a room
            if (_Map != null)
            {
                var _room = CursorRoom;
                if (_room != null)
                {
                    switch (e.Parameter.ToString())
                    {
                        case @"ZY":
                            _room.Swap(Axis.Z);
                            break;

                        case @"YX":
                            _room.Swap(Axis.Y);
                            break;

                        case @"XZ":
                            _room.Swap(Axis.X);
                            break;
                    }
                }
            }
        }
        #endregion

        #region cbSizeRoom
        private void cbSizeRoom_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            MustBeInRoom(sender, e);
            if (e.CanExecute)
            {
                var _room = CursorRoom;
                switch (e.Parameter.ToString())
                {
                    case @"Z-":
                        e.CanExecute = _room.ZHeight > 1;
                        break;

                    case @"Y-":
                        e.CanExecute = _room.YLength > 1;
                        break;

                    case @"X-":
                        e.CanExecute = _room.XLength > 1;
                        break;
                }
            }
            e.Handled = true;
        }

        private void cbSizeRoom_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // must be in a room
            if (_Map != null)
            {
                var _room = CursorRoom;
                if (_room != null)
                {
                    var _relLoc = _room.Subtract(_CursorLoc);
                    switch (e.Parameter.ToString())
                    {
                        case @"Z-":
                            _room.ResizeDown(_room.ZHeight - 1, _room.YLength, _room.XLength, _relLoc.Z, null, null);
                            break;

                        case @"Z+":
                            _room.ResizeUp(_room.ZHeight + 1, _room.YLength, _room.XLength, _relLoc.Z, null, null);
                            break;

                        case @"Y-":
                            _room.ResizeDown(_room.ZHeight, _room.YLength - 1, _room.XLength, null, _relLoc.Y, null);
                            break;

                        case @"Y+":
                            _room.ResizeUp(_room.ZHeight, _room.YLength + 1, _room.XLength, null, _relLoc.Y, null);
                            break;

                        case @"X-":
                            _room.ResizeDown(_room.ZHeight, _room.YLength, _room.XLength - 1, null, null, _relLoc.X);
                            break;

                        case @"X+":
                            _room.ResizeUp(_room.ZHeight, _room.YLength, _room.XLength + 1, null, null, _relLoc.X);
                            break;
                    }
                }
            }
        }
        #endregion

        #region cbSplitRoom
        private void cbSplitRoom_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            MustBeInRoom(sender, e);
            if (e.CanExecute)
            {
                var _room = CursorRoom;
                if (_room != null)
                {
                    var _relLoc = _room.Subtract(_CursorLoc);
                    switch (e.Parameter.ToString())
                    {
                        case @"Z": e.CanExecute = _room.CanSplit(Axis.Z, _relLoc.Z); break;
                        case @"Y": e.CanExecute = _room.CanSplit(Axis.Y, _relLoc.Y); break;
                        case @"X": e.CanExecute = _room.CanSplit(Axis.X, _relLoc.X); break;
                    }
                }
            }
            e.Handled = true;
        }

        private void cbSplitRoom_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // must be in a room
            if (_Map != null)
            {
                var _room = CursorRoom;
                if (_room != null)
                {
                    var _relLoc = _room.Subtract(_CursorLoc);
                    switch (e.Parameter.ToString())
                    {
                        case @"Z": _room.Split(Axis.Z, _relLoc.Z); break;
                        case @"Y": _room.Split(Axis.Y, _relLoc.Y); break;
                        case @"X": _room.Split(Axis.X, _relLoc.X); break;
                    }
                }
            }
        }
        #endregion

        // Cells

        #region cbFlipCell
        private void cbFlipCell_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // NOTE: cannot flip or rotate cells in background regions
            MustBeInRoom(sender, e);
            e.Handled = true;
        }

        private void cbFlipCell_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // must be in a room
            if (_Map != null)
            {
                var _room = CursorRoom;
                if (_room != null)
                {
                    var _relLoc = _room.Subtract(_CursorLoc);
                    var _cell = _room[_relLoc.Z, _relLoc.Y, _relLoc.X];
                    switch (e.Parameter.ToString())
                    {
                        case @"Z":
                            _room.SetCellStructure(_relLoc.Z, _relLoc.Y, _relLoc.X, _cell.FlipAxis(Axis.Z));
                            break;

                        case @"Y":
                            _room.SetCellStructure(_relLoc.Z, _relLoc.Y, _relLoc.X, _cell.FlipAxis(Axis.Y));
                            break;

                        case @"X":
                            _room.SetCellStructure(_relLoc.Z, _relLoc.Y, _relLoc.X, _cell.FlipAxis(Axis.X));
                            break;
                    }

                    // update
                    _room.ReLink();
                }
            }
        }
        #endregion

        #region cbSwapCell
        private void cbSwapCell_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // NOTE: cannot flip or rotate cells in background regions
            MustBeInRoom(sender, e);
            e.Handled = true;
        }

        private void cbSwapCell_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // must be in a room
            if (_Map != null)
            {
                var _room = CursorRoom;
                if (_room != null)
                {
                    var _relLoc = _room.Subtract(_CursorLoc);
                    var _cell = _room[_relLoc.Z, _relLoc.Y, _relLoc.X];
                    switch (e.Parameter.ToString())
                    {
                        case @"ZY":
                            _room.SetCellStructure(_relLoc.Z, _relLoc.Y, _relLoc.X, _cell.SwapAxis(Axis.Z, Axis.Y));
                            break;

                        case @"YX":
                            _room.SetCellStructure(_relLoc.Z, _relLoc.Y, _relLoc.X, _cell.SwapAxis(Axis.Y, Axis.X));
                            break;

                        case @"XZ":
                            _room.SetCellStructure(_relLoc.Z, _relLoc.Y, _relLoc.X, _cell.SwapAxis(Axis.X, Axis.Z));
                            break;
                    }

                    // update
                    _room.ReLink();
                }
            }
        }
        #endregion

        #region Sense Ranges
        private void galBSRange_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (chkBlindSight.IsChecked ?? false)
            {
                RedrawAll();
            }
        }

        private void galDVRange_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (chkDarkVision.IsChecked ?? false)
            {
                RedrawAll();
            }
        }

        private void galRenderRange_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            RedrawAll();
        }
        #endregion

        #region cbBumpLocator
        private void cbBumpLocator_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            MustHaveLocator(sender, e);
            e.Handled = true;
        }

        private void cbBumpLocator_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // must be in a room
            if (_Map != null)
            {
                var _locator = CursorLocator;
                if (_locator != null)
                {
                    var _lLoc = new CellPosition(_locator.GeometricRegion.LowerZ, _locator.GeometricRegion.LowerY, _locator.GeometricRegion.LowerX);
                    var _cubic = new Cubic(_lLoc, _locator.NormalSize);
                    string _mvHdng(int _hdng) => (_hdng - _Heading).ToString();

                    switch (e.Parameter.ToString())
                    {
                        case @"Z-":
                            _locator.Relocate(_cubic.OffsetCubic(AnchorFace.ZLow), _locator.PlanarPresence);
                            cbDown.Command.Execute(null);
                            break;

                        case @"Z+":
                            _locator.Relocate(_cubic.OffsetCubic(AnchorFace.ZHigh), _locator.PlanarPresence);
                            cbUp.Command.Execute(null);
                            break;

                        case @"Y-":
                            _locator.Relocate(_cubic.OffsetCubic(AnchorFace.YLow), _locator.PlanarPresence);
                            cbMove.Command.Execute(_mvHdng(6));
                            break;

                        case @"Y+":
                            _locator.Relocate(_cubic.OffsetCubic(AnchorFace.YHigh), _locator.PlanarPresence);
                            cbMove.Command.Execute(_mvHdng(2));
                            break;

                        case @"X-":
                            _locator.Relocate(_cubic.OffsetCubic(AnchorFace.XLow), _locator.PlanarPresence);
                            cbMove.Command.Execute(_mvHdng(4));
                            break;

                        case @"X+":
                            _locator.Relocate(_cubic.OffsetCubic(AnchorFace.XHigh), _locator.PlanarPresence);
                            cbMove.Command.Execute(_mvHdng(0));
                            break;
                    }
                }
            }
        }
        #endregion

        #region cbTweakLocator
        private void cbTweakLocator_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter is Locator)
            {
                e.CanExecute = true;
            }
            else
            {
                MustHaveLocator(sender, e);
            }

            e.Handled = true;
        }

        private void cbTweakLocator_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // must be in a room
            Locator _locator;
            if (e.Parameter is Locator)
            {
                _locator = e.Parameter as Locator;
            }
            else
            {
                _locator = CursorLocator;
            }

            if (_locator != null)
            {
                // show dialog
                var _oDlg = new ObjectEditorWindow(_locator)
                {
                    Title = @"Tweak Locator",
                    Owner = GetWindow(this),
                    WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner
                };
                _oDlg.ShowDialog();
            }
            e.Handled = true;
        }
        #endregion

        #region cbTweakObjects
        private void cbTweakObjects_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter is Locator)
            {
                e.CanExecute = true;
            }
            else
            {
                MustHaveLocator(sender, e);
            }

            e.Handled = true;
        }

        private void cbTweakObjects_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Locator _loc;
            if (e.Parameter is Locator)
            {
                _loc = e.Parameter as Locator;
            }
            else
            {
                _loc = CursorLocator;
            }

            if (_loc != null)
            {
                var _obj = _loc.ICoreAs<CoreObject>().FirstOrDefault();
                if (_obj is HollowFurnishingLid _lid)
                {
                    var _hflVM = new HollowFurnishingLidVM(_lid, _Map.Resources);
                    var _fDlg = new FurnishingEditorWindow()
                    {
                        DataContext = _hflVM,
                        Owner = GetWindow(this),
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    _fDlg.ShowDialog();
                }
                else if (_obj is HollowFurnishing _hollow)
                {
                    var _hfVM = new HollowFurnishingVM(_hollow, _Map.Resources);
                    var _fDlg = new FurnishingEditorWindow()
                    {
                        DataContext = _hfVM,
                        Owner = GetWindow(this),
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    _fDlg.ShowDialog();
                }
                else if (_obj is FlexibleFlatPanel _flexFlat)
                {
                    var _fDlg = new FurnishingEditorWindow()
                    {
                        DataContext = new FlexibleFlatPanelVM(_flexFlat, _Map.Resources),
                        Owner = GetWindow(this),
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    _fDlg.ShowDialog();
                }
                else if (_obj is Furnishing _furnish)
                {
                    var _fDlg = new FurnishingEditorWindow()
                    {
                        DataContext = _furnish.GetPresentableObjectVM(_Map.Resources, null),
                        Owner = GetWindow(this),
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    _fDlg.ShowDialog();
                }
                else if (_obj is Conveyance _convey)
                {
                    var _cVM = new ConveyanceVM(_convey);
                    var _cDlg = new FurnishingEditorWindow()
                    {
                        DataContext = _cVM,
                        Owner = GetWindow(this),
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    _cDlg.ShowDialog();
                }
                else
                {
                    var _oDlg = new ObjectEditorWindow(_obj.GetPresentableObjectVM(_Map.Resources, null))
                    {
                        Title = @"Tweak Object",
                        Owner = GetWindow(this),
                        WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner
                    };
                    _oDlg.ShowDialog();
                }
            }
            e.Handled = true;
        }
        #endregion

        #region chkCutAway_Checked
        private void chkCutAway_Checked(object sender, RoutedEventArgs e)
        {
            DrawCursors();
            e.Handled = true;
        }
        #endregion

        #region chkCursor_Checked
        private void chkCursor_Checked(object sender, RoutedEventArgs e)
        {
            if (grpCursor != null)
            {
                grpCursor.Visibility = chkCursor.IsChecked ?? false ? Visibility.Visible : Visibility.Hidden;
            }

            if (grpCamera != null)
            {
                grpCamera.Visibility = !(chkCursor.IsChecked ?? false) ? Visibility.Visible : Visibility.Hidden;
            }

            SetCursorLocation();
            DrawCursors();
            e.Handled = true;
        }
        #endregion

        #region cbPickModel
        private void cbPickModel_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var _locator = CursorLocator;
            e.CanExecute = (_locator as ObjectPresenter) != null;
            e.Handled = true;
        }

        private void cbPickModel_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (CursorLocator is ObjectPresenter _op)
            {
                _op.ModelKey = e.Parameter.ToString();
                SyncModelMenu(_op);
                RedrawLocators(FilteredSenses());
            }
            e.Handled = true;
        }
        #endregion

        #region tglPalettes
        PaletteWindow _CellPalette = null;
        PaletteWindow _ObjPalette = null;

        private void tglPalettes_Checked(object sender, RoutedEventArgs e)
        {
            var _tgl = e.Source as RibbonToggleButton;
            if (_tgl.Tag.ToString() == @"CellSpace")
            {
                if (_CellPalette == null)
                {
                    var _cellPal = new CellSpacePalette(_Map, this, this);
                    _CellPalette = new PaletteWindow(_tgl, _cellPal, @"CellSpaces");
                }
                _CellPalette.Show();
            }
            else if (_tgl.Tag.ToString() == @"Object")
            {
                if (_ObjPalette == null)
                {
                    var _objPal = new ObjectPalette(_Map, this);
                    _ObjPalette = new PaletteWindow(_tgl, _objPal, @"Objects");
                }
                _ObjPalette.Show();
            }
        }

        private void tglPalettes_Unchecked(object sender, RoutedEventArgs e)
        {
            var _tgl = e.Source as RibbonToggleButton;
            if (_tgl.Tag.ToString() == @"CellSpace")
            {
                _CellPalette?.Hide();
            }
            else if (_tgl.Tag.ToString() == @"Object")
            {
                _ObjPalette?.Hide();
            }
        }
        #endregion

        #region cbViewportEdit
        private void cbViewportEdit_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _Map != null;
            e.Handled = true;
        }

        private void cbViewportEdit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_Map != null)
            {
                var _viewDlg = new ViewportEdit(_Map.BackgroundViewport)
                {
                    Owner = this
                };
                var _rslt = _viewDlg.ShowDialog();
                if (_rslt ?? false)
                {
                    _Map.BackgroundViewport = _viewDlg.GetCubic();
                    _Map.ShadingZones.RecacheShadingZones();
                    _Map.ShadingZones.ReshadeBackground();
                    _Map.SignalMapChanged(this);
                }
            }
            e.Handled = true;
        }
        #endregion

        #region cbZoomIcon
        private void cbZoomIcon_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter is Locator)
            {
                e.CanExecute = true;
            }
            else
            {
                MustHaveLocator(sender, e);
            }

            e.Handled = true;
        }

        private void cbZoomIcon_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ObjectPresenter _presenter;
            if (e.Parameter is ObjectPresenter)
            {
                _presenter = e.Parameter as ObjectPresenter;
            }
            else
            {
                _presenter = CursorLocator as ObjectPresenter;
            }

            if (_presenter != null)
            {
                if (ZoomedIcon == _presenter.ICore.ID)
                {
                    ZoomedIcon = Guid.Empty;
                }
                else
                {
                    ZoomedIcon = _presenter.ICore.ID;
                }
            }
            var _senses = FilteredSenses();
            RedrawLocators(_senses);
        }
        #endregion

        #region IZoomIcons Members

        public double ZoomLevel
        {
            get
            {
                if (_Zoom <= 0d)
                {
                    _Zoom = 1d;
                }

                return _Zoom;
            }
            set
            {
                // boundary
                if (_Zoom < 0.3d)
                {
                    _Zoom = 0.3d;
                }

                if (_Zoom > 1d)
                {
                    _Zoom = 1d;
                }

                _Zoom = value;
            }
        }

        public double UnZoomLevel
        {
            get
            {
                // default
                if (_UnZoom <= 0.1d)
                {
                    _UnZoom = 0.3d;
                }

                return _UnZoom;
            }
            set
            {
                // boundary
                if (_UnZoom < 0.1d)
                {
                    _UnZoom = 0.1d;
                }

                if (_UnZoom > 1d)
                {
                    _UnZoom = 1d;
                }

                _UnZoom = value;
            }
        }

        public Guid ZoomedIcon
        {
            get
            {
                return _ZoomIcon;
            }
            set
            {
                _ZoomIcon = value;
            }
        }

        #endregion

        #region IPresentationInputBinder Members

        public IEnumerable<InputBinding> GetBindings(Presentable presentable)
        {
            yield return new MouseBinding(TweakLocator, new MouseGesture(MouseAction.LeftClick, ModifierKeys.Control))
            {
                CommandParameter = presentable.Presenter,
                CommandTarget = this
            };
            yield return new MouseBinding(TweakObjects, new MouseGesture(MouseAction.LeftClick))
            {
                CommandParameter = presentable.Presenter,
                CommandTarget = this
            };
            if (presentable.Presentations.Any(_p => _p is IconPresentation))
            {
                yield return new MouseBinding(ZoomIcon, new MouseGesture(MouseAction.RightClick))
                {
                    CommandParameter = presentable.Presenter,
                    CommandTarget = this
                };
            }
            yield break;
        }

        #endregion

        // apply "templates" to creatures

        #region cbSkeletonize
        private void cbSkeletonize_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ObjectPresenter _presenter;
            if (e.Parameter is ObjectPresenter)
            {
                _presenter = e.Parameter as ObjectPresenter;
            }
            else
            {
                _presenter = CursorLocator as ObjectPresenter;
            }

            // make skeleton
            var _skeleton = new Skeleton(_presenter.Chief as Creature);
            var _replace = new Creature(_presenter.Chief.Name, _skeleton.DefaultAbilities());
            _skeleton.BindTo(_replace);
            _replace.Devotion = new Devotion(@"Death");

            // this is the new chief
            _ = new ObjectPresenter(_replace, _presenter.MapContext, _presenter.ModelKey, _presenter.NormalSize, _presenter.GeometricRegion);
            _presenter.MapContext.Remove(_presenter);
        }

        private void cbSkeletonize_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!(e.Parameter is Locator _locator))
            {
                _locator = CursorLocator as Locator;
            }
            if (_locator != null)
            {
                if (_locator.Chief is Creature)
                {
                    var _zombie = new Skeleton(_locator.Chief as Creature);
                    e.CanExecute = _zombie.CanGenerate;
                }
            }
            e.Handled = true;
        }
        #endregion

        #region cbZombify
        private void cbZombify_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ObjectPresenter _presenter;
            if (e.Parameter is ObjectPresenter)
            {
                _presenter = e.Parameter as ObjectPresenter;
            }
            else
            {
                _presenter = CursorLocator as ObjectPresenter;
            }

            // make zombie
            var _zombie = new Zombie(_presenter.Chief as Creature);
            var _replace = new Creature(_presenter.Chief.Name, _zombie.DefaultAbilities());
            _zombie.BindTo(_replace);
            _replace.Devotion = new Devotion(@"Death");

            // this is the new chief
            _ = new ObjectPresenter(_replace, _presenter.MapContext, _presenter.ModelKey, _presenter.NormalSize, _presenter.GeometricRegion);
            _presenter.MapContext.Remove(_presenter);
        }

        private void cbZombify_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!(e.Parameter is Locator _locator))
            {
                _locator = CursorLocator as Locator;
            }
            if (_locator != null)
            {
                if (_locator.Chief is Creature)
                {
                    var _zombie = new Zombie(_locator.Chief as Creature);
                    e.CanExecute = _zombie.CanGenerate;
                }
            }
            e.Handled = true;
        }
        #endregion

        #region cbGhoulify
        private void cbGhoulify_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ObjectPresenter _presenter;
            if (e.Parameter is ObjectPresenter)
            {
                _presenter = e.Parameter as ObjectPresenter;
            }
            else
            {
                _presenter = CursorLocator as ObjectPresenter;
            }

            // make ghoul
            var _ghoul = new Ghoul(_presenter.Chief as Creature);
            var _replace = new Creature(_presenter.Chief.Name, _ghoul.DefaultAbilities());
            _ghoul.BindTo(_replace);
            _replace.Devotion = new Devotion(@"Death");

            // this is the new chief
            _ = new ObjectPresenter(_replace, _presenter.MapContext, _presenter.ModelKey, _presenter.NormalSize, _presenter.GeometricRegion);
            _presenter.MapContext.Remove(_presenter);
        }

        private void cbGhoulify_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!(e.Parameter is Locator _locator))
            {
                _locator = CursorLocator as Locator;
            }
            if (_locator != null)
            {
                if (_locator.Chief is Creature)
                {
                    var _ghoul = new Ghoul(_locator.Chief as Creature);
                    e.CanExecute = _ghoul.CanGenerate;
                }
            }
            e.Handled = true;
        }
        #endregion

        #region cbCelestial
        private void cbCelestial_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!(e.Parameter is Locator _locator))
            {
                _locator = CursorLocator as Locator;
            }
            if (_locator != null)
            {
                if (_locator.Chief is Creature _critter)
                {
                    var _celestial = new Celestial();
                    e.CanExecute = _celestial.CanAnchor(_critter);
                }
            }
            e.Handled = true;
        }

        private void cbCelestial_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ObjectPresenter _presenter;
            if (e.Parameter is ObjectPresenter)
            {
                _presenter = e.Parameter as ObjectPresenter;
            }
            else
            {
                _presenter = CursorLocator as ObjectPresenter;
            }

            // make celestial
            var _celestial = new Celestial();
            _presenter.Chief.AddAdjunct(_celestial);
        }
        #endregion

        #region cbFiendish
        private void cbFiendish_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!(e.Parameter is Locator _locator))
            {
                _locator = CursorLocator as Locator;
            }
            if (_locator != null)
            {
                if (_locator.Chief is Creature _critter)
                {
                    var _fiendish = new Fiendish();
                    e.CanExecute = _fiendish.CanAnchor(_critter);
                }
            }
            e.Handled = true;
        }

        private void cbFiendish_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ObjectPresenter _presenter;
            if (e.Parameter is ObjectPresenter)
            {
                _presenter = e.Parameter as ObjectPresenter;
            }
            else
            {
                _presenter = CursorLocator as ObjectPresenter;
            }

            // make celestial
            var _fiendish = new Fiendish();
            _presenter.Chief.AddAdjunct(_fiendish);
        }
        #endregion

        // twist and turn objects

        #region cbTurnObject
        private void cbTurnObject_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            MustHaveLocator(sender, e);
            if (e.CanExecute)
            {
                var _loc = CursorLocator;
                e.CanExecute = (_loc.Chief != null) || (_loc.ICore is Furnishing) || (_loc.ICore is Conveyance);
            }
            e.Handled = true;
        }

        private void cbTurnObject_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _loc = CursorLocator;
            if (_loc.Chief is Creature _chief)
            {
                _chief.Heading += Convert.ToInt32(e.Parameter);
                RedrawLocators(FilteredSenses());
            }
            else if (_loc.ICore is Furnishing _furnish)
            {
                var _orient = _furnish.Orientation;
                var _gravity = _orient.GravityFace;
                var _hdng = _orient.Heading;
                var _hFace = _gravity.GetHeadingFaces(_orient.Heading * 2).ToAnchorFaces().FirstOrDefault();
                var _param = Convert.ToInt32(e.Parameter);

                void _doTurn()
                {
                    // {in|de}crease (ortho-)heading
                    _hdng += _param;
                    _orient.SetOrientation(null, null, _hdng);
                }

                // heading face
                var _right = _gravity.RightFace(_hdng * 2)
                    ?? _gravity.GetHeadingFaces(6).ToAnchorFaces().FirstOrDefault();
                var _left = _gravity.LeftFace(_hdng * 2)
                    ?? _gravity.GetHeadingFaces(2).ToAnchorFaces().FirstOrDefault();
                if (_param == 1)
                {
                    // left turn
                    if (_orient.IsFaceSnapped(_right) && !_orient.IsFaceSnapped(_left))
                    {
                        // wrong corner for direction
                        if (_orient.IsHeadingTwisted)
                        {
                            // untwist only to flatten out object
                            _orient.SetOrientation(null, _orient.Twist + 1, null);
                        }
                        else
                        {
                            // snapped to right, but not left, change snap
                            _orient.SetAxisSnap(_right.GetAxis(), _right.IsLowFace());
                        }

                    }
                    else
                    {
                        // correct corner
                        if (_orient.IsHeadingTwisted)
                        {
                            _hdng += _param;
                            _orient.SetOrientation(null, _orient.Twist - 1, _hdng);
                            _orient.SetAxisSnap(_hFace.GetAxis(), _hFace.IsLowFace());
                        }
                        else
                        {
                            _doTurn();
                        }
                    }
                }
                else if (_param == -1)
                {
                    // right turn
                    if (_orient.IsFaceSnapped(_left) && !_orient.IsFaceSnapped(_right))
                    {
                        // wrong corner for direction
                        if (_orient.IsHeadingTwisted)
                        {
                            // untwist only to flatten out object
                            _orient.SetOrientation(null, _orient.Twist - 1, null);
                        }
                        else
                        {
                            // snapped to left, but not right, change snap
                            _orient.SetAxisSnap(_left.GetAxis(), _left.IsLowFace());
                        }
                    }
                    else
                    {
                        if (_orient.IsHeadingTwisted)
                        {
                            _hdng += _param;
                            _orient.SetOrientation(null, _orient.Twist + 1, _hdng);
                            _orient.SetAxisSnap(_hFace.GetAxis(), _hFace.IsLowFace());
                        }
                        else
                        {
                            _doTurn();
                        }
                    }
                }
                else
                {
                    _doTurn();
                }
                RedrawLocators(FilteredSenses());
            }
            else if (_loc.ICore is Conveyance _convey)
            {
                var _hdng = _convey.Orientation.Heading;
                var _param = Convert.ToInt32(e.Parameter);

                // {in|de}crease (ortho-)heading
                _hdng += _param * 1;
                _convey.Orientation.SetOrientation(null, null, _hdng);
                RedrawLocators(FilteredSenses());
            }
            e.Handled = true;
        }
        #endregion

        #region cbTwistObject
        private void cbTwistObject_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            MustHaveLocator(sender, e);
            e.CanExecute &= (CursorLocator?.ICore is Furnishing);
            e.Handled = true;
        }

        private void cbTwistObject_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _loc = CursorLocator;
            if (_loc.ICore is Furnishing)
            {
                var _furnish = _loc.ICore as Furnishing;
                var _twist = _furnish.Orientation.Twist;
                _twist += Convert.ToInt32(e.Parameter);
                _furnish.Orientation.SetOrientation(null, _twist, null);
                RedrawLocators(FilteredSenses());
            }
        }
        #endregion

        #region cbTiltObject
        private void cbTiltObject_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            MustHaveLocator(sender, e);
            var _change = Convert.ToInt32(e.Parameter);
            var _furnish = (CursorLocator?.ICore as Furnishing);
            e.CanExecute &= (_furnish != null);
            e.Handled = true;
        }

        private void cbTiltObject_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _loc = CursorLocator;
            if (_loc.ICore is Furnishing)
            {
                var _furnish = _loc.ICore as Furnishing;
                var _upright = _furnish.Orientation.Upright;

                // double step incline/decline if on diagonal heading
                _upright += Convert.ToInt32(e.Parameter) * 2;
                if (_upright > Verticality.OnSideBottomOut)
                {
                    _upright = Verticality.Upright;
                }

                if (_upright < Verticality.Upright)
                {
                    _upright = Verticality.OnSideBottomOut;
                }

                _furnish.Orientation.SetOrientation(_upright, null, null);
                RedrawLocators(FilteredSenses());
            }
        }
        #endregion

        // portal stuff

        #region cbOpenFace | cbCloseFace
        private void cbPortal_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            MustHaveLocator(sender, e);
            e.CanExecute &= (CursorLocator?.ICore is PortalBase);
        }

        private void cbSlidingPortalFace_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            MustHaveLocator(sender, e);
            e.CanExecute &= (CursorLocator?.ICore is SlidingPortal);
        }

        private void cbCornerPortalFace_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            MustHaveLocator(sender, e);
            e.CanExecute &= (CursorLocator?.ICore is CornerPivotPortal);
        }

        private void cbOpenFace_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _loc = CursorLocator;
            if (_loc.ICore is CornerPivotPortal _portal)
            {
                switch (e.Parameter.ToString())
                {
                    case @"Z-":
                        _portal.AnchorOpen = AnchorFace.ZLow;
                        break;

                    case @"Z+":
                        _portal.AnchorOpen = AnchorFace.ZHigh;
                        break;

                    case @"Y-":
                        _portal.AnchorOpen = AnchorFace.YLow;
                        break;

                    case @"Y+":
                        _portal.AnchorOpen = AnchorFace.YHigh;
                        break;

                    case @"X-":
                        _portal.AnchorOpen = AnchorFace.XLow;
                        break;

                    case @"X+":
                        _portal.AnchorOpen = AnchorFace.XHigh;
                        break;
                }
            }
            else if (_loc.ICore is SlidingPortal _slide)
            {
                switch (e.Parameter.ToString())
                {
                    case @"Z-":
                        _slide.AnchorFace = AnchorFace.ZLow;
                        break;

                    case @"Z+":
                        _slide.AnchorFace = AnchorFace.ZHigh;
                        break;

                    case @"Y-":
                        _slide.AnchorFace = AnchorFace.YLow;
                        break;

                    case @"Y+":
                        _slide.AnchorFace = AnchorFace.YHigh;
                        break;

                    case @"X-":
                        _slide.AnchorFace = AnchorFace.XLow;
                        break;

                    case @"X+":
                        _slide.AnchorFace = AnchorFace.XHigh;
                        break;
                }
            }

            RedrawLocators(FilteredSenses());
        }

        private void cbCloseFace_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _loc = CursorLocator;
            if (_loc.ICore is CornerPivotPortal)
            {
                var _portal = _loc.ICore as CornerPivotPortal;
                switch (e.Parameter.ToString())
                {
                    case @"Z-":
                        _portal.AnchorClose = AnchorFace.ZLow;
                        break;

                    case @"Z+":
                        _portal.AnchorClose = AnchorFace.ZHigh;
                        break;

                    case @"Y-":
                        _portal.AnchorClose = AnchorFace.YLow;
                        break;

                    case @"Y+":
                        _portal.AnchorClose = AnchorFace.YHigh;
                        break;

                    case @"X-":
                        _portal.AnchorClose = AnchorFace.XLow;
                        break;

                    case @"X+":
                        _portal.AnchorClose = AnchorFace.XHigh;
                        break;
                }

                RedrawLocators(FilteredSenses());
            }
        }
        #endregion

        #region cbSlideAxis
        private void cbSlideAxis_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _loc = CursorLocator;
            if (_loc.ICore is SlidingPortal _slide)
            {
                switch (e.Parameter.ToString())
                {
                    case @"Z":
                        _slide.SlidingAxis = Axis.Z;
                        break;

                    case @"Y":
                        _slide.SlidingAxis = Axis.Y;
                        break;

                    case @"X":
                        _slide.SlidingAxis = Axis.X;
                        break;
                }
            }
        }
        #endregion

        #region cbPortalOpenState
        private void cbPortalOpenState_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _loc = CursorLocator;
            if (_loc.ICore is PortalBase _portal)
            {
                _portal.OpenState = (e.Parameter.ToString()) switch
                {
                    @"Close" => _portal.GetOpenStatus(null, _portal, 0),
                    _ => _portal.GetOpenStatus(null, _portal, 1),
                };
            }
        }
        #endregion

        // stuff

        #region cbDrawObject
        private void cbDrawObject_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cbDrawObject_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is string)
            {
                var _str = e.Parameter.ToString();
                if (_str.Length > 0)
                {
                    if (_ObjPalette != null)
                    {
                        var _op = _ObjPalette.gridPlaceHolder.Children[0] as ObjectPalette;
                        DrawObject.Execute(_op.CommandParameter(Convert.ToInt32(_str)), this);
                    }
                }
            }
            else
            {
                if ((chkCursor.IsChecked ?? false) && (e.Parameter is ContentControl _ccObject))
                {
                    var _room = CursorRoom;
                    if ((_room != null) && (_ccObject.Content != null))
                    {
                        var _rCursor = _room.Subtract(_CursorLoc);
                        var _furnish = (_ccObject.Content as Furnishing);
                        if (_furnish?.Clone() is Furnishing _clone)
                        {
                            var _size = _clone.Orientation.SnappableSize;
                            var _cube = new Cubic(_CursorLoc, _size.ZHeight, _size.YLength, _size.XLength);
                            var _key = (_furnish.GetLocated()?.Locator as ObjectPresenter)?.ModelKey;
                            _ = new ObjectPresenter(_clone, _Map.MapContext, _key, _size, _cube);
                        }
                    }
                }
            }
        }
        #endregion

        #region cbReindex
        private void cbReindex_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cbReindex_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _Map.MapContext.RebuildIndex();
            e.Handled = true;
        }
        #endregion
    }
}