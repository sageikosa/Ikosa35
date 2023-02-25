using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Visualize;
using Uzi.Ikosa.Contracts;
using Uzi.Visualize.Contracts.Tactical;
using Uzi.Visualize.Contracts;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class LocaleViewModel : ViewModelBase, IZoomIcons, IPresentationInputBinder
    {
        #region construction
        public LocaleViewModel(ObservableActor actor, LocalMapInfo map, SensorHostInfo sensors, List<ShadingInfo> shadings,
            List<PresentationInfo> presentations, List<TransientVisualizerInfo> transients, List<SoundAwarenessInfo> sounds,
            List<ExtraInfoInfo> extras, ICellLocation targetCell, Guid zoomedIcon)
        {
            // parameters
            _ObservableActor = actor;
            _Map = map;
            _Sensors = sensors;
            _Shadings = shadings;
            _Presentations = presentations;
            _Transients = transients;
            _Sounds = sounds;
            _Extra = extras;
            _TargetCell = targetCell;
            _ZoomIcon = zoomedIcon;

            // overlay control
            _Overlay = false;
            _SoundShow = false;
            _ExtrasShow = false;

            // preserve last ViewPoint if possible
            var _state = actor.GetLocalState();
            _ViewPoint = _state.ViewPointType;
            _GameBoardState = _state.GameBoardState;
            _ThirdPersonState = _state.ThirdPersonState;

            // commands
            _SubSelectCmd = new RelayCommand<PresentationInfo>(SubSelect_Executed, Select_CanExecute);
            _PickAwarenessCmd = new RelayCommand<AwarenessInfo>(PickAwareness_Executed, Awareness_CanExecute);
            _SelectCmd = new RelayCommand<PresentationInfo>(Select_Executed, Select_CanExecute);
            _ContextSelectCmd = new RelayCommand<PresentationInfo>(ContextSelect_Execute, Select_CanExecute);
            _AlterSelectionCmd = new RelayCommand<PresentationInfo>(AlterSelection_Execute, Select_CanExecute);
            _ZoomIconCmd = new RelayCommand<PresentationInfo>(ZoomIcon_Execute, ZoomIcon_CanExecute);
            _GrabCmd = new RelayCommand<PresentationInfo>(Grab_Execute, Grab_CanExecute);

            _LookTurnCmd = new RelayCommand<string>(LookTurn_Executed, LookTurn_CanExecute);
            _GazeCmd = new RelayCommand(Gaze_Executed, Gaze_CanExecute);
            _ViewFieldChangeCmd = new RelayCommand<string>(ViewFieldChange_Executed, ViewFieldChange_CanExecute);
            _ToggleSounds = new RelayCommand(() => ShowSounds = !ShowSounds);
            _ToggleMarkers = new RelayCommand(() => ShowExtraMarkers = !ShowExtraMarkers);
            _GBInit = false;
        }
        #endregion

        #region data
        private readonly LocalMapInfo _Map;
        private SensorHostInfo _Sensors;
        private bool _GBInit;
        private ViewPointType _ViewPoint;
        private uint _GameBoardState;
        private uint _ThirdPersonState;
        private readonly List<ShadingInfo> _Shadings;
        private readonly List<PresentationInfo> _Presentations;
        private readonly List<TransientVisualizerInfo> _Transients;
        private readonly List<SoundAwarenessInfo> _Sounds;
        private readonly List<ExtraInfoInfo> _Extra;
        private readonly ObservableActor _ObservableActor;
        private ICellLocation _TargetCell = new CellPosition();
        private bool _Overlay;
        private bool _SoundShow;
        private bool _ExtrasShow;

        private double _Zoom = 1.0d;
        private double _UnZoom = 0.3d;
        private Guid _ZoomIcon = Guid.Empty;
        private readonly RelayCommand<PresentationInfo> _SubSelectCmd;
        private readonly RelayCommand<AwarenessInfo> _PickAwarenessCmd;
        private readonly RelayCommand<PresentationInfo> _SelectCmd;
        private readonly RelayCommand<PresentationInfo> _ContextSelectCmd;
        private readonly RelayCommand<PresentationInfo> _AlterSelectionCmd;
        private readonly RelayCommand<PresentationInfo> _ZoomIconCmd;
        private readonly RelayCommand<PresentationInfo> _GrabCmd;

        private readonly RelayCommand<string> _LookTurnCmd;
        private readonly RelayCommand _GazeCmd;
        private readonly RelayCommand<string> _ViewFieldChangeCmd;
        private readonly RelayCommand _ToggleSounds;
        private readonly RelayCommand _ToggleMarkers;
        #endregion

        public ObservableActor ObservableActor => _ObservableActor;
        public LocalMapInfo Map => _Map;
        public IEnumerable<ShadingInfo> Shadings => _Shadings;
        public IEnumerable<PresentationInfo> Presentations => _Presentations;
        public IEnumerable<TransientVisualizerInfo> TransientVisualizers => _Transients;
        public IEnumerable<SoundAwarenessInfo> Sounds => _Sounds;
        public IEnumerable<ExtraInfoInfo> ExtraInfos => _Extra;
        public ICellLocation TargetCell => _TargetCell;
        public ViewPointType ViewPointType => _ViewPoint;

        #region public SensorHostInfo Sensors { get; private set; }
        public SensorHostInfo Sensors
        {
            get => _Sensors;
            private set
            {
                _Sensors = value;
                DoPropertyChanged(nameof(Sensors));
                DoPropertyChanged(nameof(ForwardHeading));
                DoPropertyChanged(nameof(ForwardIncline));
            }
        }
        #endregion

        public uint ViewPointState
            => (_ViewPoint == ViewPointType.GameBoard)
            ? _GameBoardState
            : _ThirdPersonState;

        public RelayCommand<PresentationInfo> SubSelect => _SubSelectCmd;
        public RelayCommand<AwarenessInfo> PickAwareness => _PickAwarenessCmd;
        public RelayCommand<PresentationInfo> Select => _SelectCmd;
        public RelayCommand<PresentationInfo> ContextSelect => _ContextSelectCmd;
        public RelayCommand<PresentationInfo> AlterSelection => _AlterSelectionCmd;
        public RelayCommand<PresentationInfo> ZoomIcon => _ZoomIconCmd;
        public RelayCommand<PresentationInfo> Grab => _GrabCmd;
        public RelayCommand ToggleSounds => _ToggleSounds;
        public RelayCommand ToggleMarkers => _ToggleMarkers;

        public RelayCommand<string> LookTurn => _LookTurnCmd;
        public RelayCommand GazeCommand => _GazeCmd;
        public RelayCommand<string> ViewFieldChange => _ViewFieldChangeCmd;

        #region public bool ShowOverlay { get; set; }
        public bool ShowOverlay
        {
            get => _Overlay;
            set
            {
                _Overlay = value;
                DoPropertyChanged(nameof(ShowOverlay));
            }
        }
        #endregion

        #region public bool ShowSounds { get; set; }
        public bool ShowSounds
        {
            get => _SoundShow;
            set
            {
                _SoundShow = value;
                DoPropertyChanged(nameof(ShowSounds));
            }
        }
        #endregion

        #region public bool ShowExtraMarkers { get; set; }
        public bool ShowExtraMarkers
        {
            get => _ExtrasShow;
            set
            {
                _ExtrasShow = value;
                DoPropertyChanged(nameof(ShowExtraMarkers));
            }
        }
        #endregion

        public void RefreshPresentations()
            => DoPropertyChanged(nameof(SelectedPresentations));

        public int ForwardHeading
            => ViewPointType switch
            {
                ViewPointType.GameBoard => new GameBoardViewPointState(ViewPointState).Heading,
                _ => Sensors.Heading
            };

        public int ForwardIncline
            => ViewPointType switch
            {
                ViewPointType.GameBoard => new GameBoardViewPointState(ViewPointState).Above ? 2 : 1,
                _ => Sensors.Incline
            };

        private ViewPointType _SaveView = ViewPointType.Undefined;
        public void ToggleAimPoint()
        {
            if (_ViewPoint != ViewPointType.AimPoint)
            {
                _SaveView = _ViewPoint;
                _ViewPoint = ViewPointType.AimPoint;
                DoPropertyChanged(nameof(ViewPointType));
                DoPropertyChanged(nameof(ViewPointState));
            }
            else
            {
                _ViewPoint = _SaveView == ViewPointType.Undefined ? ViewPointType.ThirdPerson : _SaveView;
                DoPropertyChanged(nameof(ViewPointType));
                DoPropertyChanged(nameof(ViewPointState));
            }
        }

        #region public void SetSensorHostThirdCamera(int heading, int incline)
        public void SetSensorHostThirdCamera(int heading, int incline)
        {
            var _heading = heading;
            var _incline = 1;
            switch (incline)
            {
                case -2:
                    _incline = 1;
                    switch (heading)
                    {
                        case 2: case 3: _heading = 1; break;
                        case 8: case 4: _heading = 0; break;
                        case 6: case 5: _heading = 7; break;
                    }
                    break;

                case -1:
                    _incline = 1;
                    switch (heading)
                    {
                        case 1: case 3: _heading = 2; break;
                        case 0: case 4: _heading = 8; _incline = 2; break;
                        case 7: case 5: _heading = 6; break;
                    }
                    break;

                case 1:
                    _incline = 0;
                    switch (heading)
                    {
                        case 1: case 2: _heading = 3; break;
                        case 0: case 8: _heading = 4; break;
                        case 6: case 7: _heading = 5; break;
                    }
                    break;
                case 2:
                    _incline = -1;
                    switch (heading)
                    {
                        case 1: case 2: _heading = 3; break;
                        case 0: case 8: _heading = 4; break;
                        case 6: case 7: _heading = 5; break;
                    }
                    break;
                default:
                    switch (heading)
                    {
                        case 1: case 2: _heading = 3; break;
                        case 0: case 8: _heading = 4; break;
                        case 6: case 7: _heading = 5; break;
                    }
                    break;
            }
            Sensors = ObservableActor.Proxies.ViewProxy.Service
                .SetSensorHostThirdCamera(ObservableActor.Actor.CreatureLoginInfo.ID.ToString(), _Sensors.ID, _heading, _incline);
        }
        #endregion

        #region public void AimCameraAdjust(short zOff, short yOff, short xOff)
        public void AimCameraAdjust(short zOff, short yOff, short xOff)
        {
            Sensors = ObservableActor.Proxies.ViewProxy.Service
                .AdjustSensorHostAiming(ObservableActor.Actor.CreatureLoginInfo.ID.ToString(), _Sensors.ID, zOff, yOff, xOff);
        }
        #endregion

        #region public void SetSensorHostAimExtent(double longitude, double latitude)
        public void SetSensorHostAimExtent(double longitude, double latitude)
        {
            Sensors = ObservableActor.Proxies.ViewProxy.Service
                .SetSensorHostAimExtent(ObservableActor.Actor.CreatureLoginInfo.ID.ToString(), _Sensors.ID, longitude, latitude);
        }
        #endregion

        #region public void SetSensorHostHeading(int heading, int incline)
        public void SetSensorHostHeading(int heading, int incline)
        {
            var _rel = _Sensors.ThirdCameraRelativeHeading;
            Sensors = ObservableActor.Proxies.ViewProxy.Service
                .SetSensorHostHeading(ObservableActor.Actor.CreatureLoginInfo.ID.ToString(), _Sensors.ID, heading, incline);
            SetSensorHostThirdCamera(_rel, incline);
        }
        #endregion

        /// <summary>get presentations for selected awarenesses</summary>
        public IEnumerable<PresentationInfo> SelectedPresentations
            => ((Presentations != null) && (ObservableActor.Awarenesses != null))
            ? (from _a in ObservableActor.Awarenesses
               where _a.HasAnySelected
               let _p = Presentations.FirstOrDefault(_p => _p.PresentingIDs.Contains(_a.ID))
               where _p != null
               select _p).ToList()
            : new List<PresentationInfo>();

        #region public void ResetTargetCell()
        public void ResetTargetCell()
        {
            _TargetCell = new CellPosition(Sensors.AimCell);
            DoPropertyChanged(nameof(TargetCell));
        }
        #endregion

        #region public void MoveTargetPointer(int heading, int upDownAdjust)
        public void MoveTargetPointer(int heading, int upDownAdjust)
        {
            // get movement faces for the adjustment
            var _moveFaces = AnchorFaceHelper.MovementFaces(Sensors.GravityAnchorFace, (heading + ForwardHeading) % 8, upDownAdjust).ToArray();

            // get cell offset
            var _offset = _moveFaces.GetAnchorOffset();

            // calc new cell
            var _newCell = new CellPosition(TargetCell.Z + _offset.Z, TargetCell.Y + _offset.Y, _TargetCell.X + _offset.X);

            // set new cell
            _TargetCell = _newCell;
            DoPropertyChanged(@"TargetCell");
        }
        #endregion

        #region IZoomIcons Members

        public double ZoomLevel
        {
            get
            {
                if (_Zoom <= 0d)
                    _Zoom = 1d;
                return _Zoom;
            }
            set
            {
                // boundary
                if (_Zoom < 0.3d)
                    _Zoom = 0.3d;
                if (_Zoom > 1d)
                    _Zoom = 1d;
                _Zoom = value;
            }
        }

        public double UnZoomLevel
        {
            get
            {
                // default
                if (_UnZoom <= 0.1d)
                    _UnZoom = 0.3d;
                return _UnZoom;
            }
            set
            {
                // boundary
                if (_UnZoom < 0.1d)
                    _UnZoom = 0.1d;
                if (_UnZoom > 1d)
                    _UnZoom = 1d;
                _UnZoom = value;
            }
        }

        public Guid ZoomedIcon
        {
            get => _ZoomIcon;
            set => _ZoomIcon = value;
        }

        #endregion

        #region Select
        private bool Select_CanExecute(PresentationInfo present)
            => (present != null)
            && ObservableActor.Awarenesses.Any(_a => _a.ID == present.PresentingIDs.FirstOrDefault());

        private void Select_Executed(PresentationInfo present)
        {
            if (present != null)
            {
                // main object
                var _aware = ObservableActor.Awarenesses.FirstOrDefault(_a => present.PresentingIDs.First() == _a.ID);

                // zoomed icon
                if ((present is IconPresentationInfo) && (_aware != null))
                {
                    if (ZoomedIcon == _aware.ID)
                        ZoomedIcon = Guid.Empty;
                    else
                        ZoomedIcon = _aware.ID;
                }
                ObservableActor.SelectAwareness(_aware.ID);
            }
        }
        #endregion

        #region Alter Selection
        private void AlterSelection_Execute(PresentationInfo present)
        {
            if (present != null)
            {
                if (ObservableActor.SelectedAwarenesses.Any(_sa => present.PresentingIDs.Contains(_sa.ID)))
                {
                    // remove from selection
                    ObservableActor.RemoveAwarenesses(ObservableActor.Awarenesses
                        .Where(_a => present.PresentingIDs.Contains(_a.ID))
                        .Select(_sa => _sa.ID));
                }
                else
                {
                    // add to selection
                    ObservableActor.AddAwarenesses(ObservableActor.Awarenesses
                        .Where(_a => present.PresentingIDs.Contains(_a.ID))
                        .Select(_sa => _sa.ID));
                }
                ObservableActor.SetContextMenu();
            }
        }
        #endregion

        #region Context Select
        private void ContextSelect_Execute(PresentationInfo present)
        {
            if (present != null)
            {
                // new selection
                if (ObservableActor.Awarenesses.Any(_a => present.PresentingIDs.Contains(_a.ID)))
                {
                    ObservableActor.SelectAwareness(Guid.Empty);
                    ObservableActor.AddAwarenesses(ObservableActor.Awarenesses
                        .Where(_a => present.PresentingIDs.Contains(_a.ID))
                        .Select(_sa => _sa.ID));
                    ObservableActor.SetContextMenu();
                }
                else
                {
                    ObservableActor.SetContextMenu(true);
                }
            }
            else
            {
                ObservableActor.SetContextMenu(true);
            }
        }
        #endregion

        #region ZoomIcon
        private bool ZoomIcon_CanExecute(PresentationInfo present)
            => (present != null)
            && ObservableActor.Awarenesses.Any(_a => _a.ID == present.PresentingIDs.FirstOrDefault());

        private void ZoomIcon_Execute(PresentationInfo present)
        {
            if (present != null)
            {
                // main object
                var _aware = ObservableActor.Awarenesses.FirstOrDefault(_a => present.PresentingIDs.First() == _a.ID);

                // zoomed icon
                if (ZoomedIcon == _aware.ID)
                    ZoomedIcon = Guid.Empty;
                else
                    ZoomedIcon = _aware.ID;

                // refresh items
                RefreshPresentations();
            }
        }
        #endregion

        #region Grab
        private bool Grab_CanExecute(PresentationInfo present)
            => (present != null)
            && ObservableActor.Actor.Actions
            .Any(_a => _a.Key == @"Furnishing.Grab" && _a.Provider.ID == present.PresentingIDs.FirstOrDefault());

        private void Grab_Execute(PresentationInfo present)
        {
            if (present != null)
            {
                // main action
                var _action = ObservableActor.Actor.Actions
                    .FirstOrDefault(_a => _a.Key == @"Furnishing.Grab" && _a.Provider.ID == present.PresentingIDs.FirstOrDefault());
                if (_action != null)
                    ObservableActor.Actor.DoAction.Execute(_action);
            }
        }
        #endregion

        #region PickAwareness
        private bool Awareness_CanExecute(AwarenessInfo awareness)
            => true;

        private void PickAwareness_Executed(AwarenessInfo awareness)
        {
            if (awareness != null)
            {
                ObservableActor.SelectAwareness(awareness.ID);
            }
        }
        #endregion

        #region private void AddSubSelectMenus(ItemCollection menu, AwarenessInfo aware)
        private void AddSubSelectMenus(ItemCollection menu, AwarenessInfo aware)
        {
            foreach (var _a in aware.Items)
            {
                if (_a.Items.Any())
                {
                    var _subMenu = new MenuItem { Header = _a.Info };
                    menu.Add(_subMenu);
                    _subMenu.Items.Add(new MenuItem
                    {
                        Header = @"Pick",
                        Command = PickAwareness,
                        CommandParameter = _a,
                        IsChecked = _a.IsInSelection
                    });
                    _subMenu.Items.Add(new Separator());
                    AddSubSelectMenus(_subMenu.Items, _a);
                }
                else
                {
                    menu.Add(new MenuItem
                    {
                        Header = _a.Info,
                        Command = PickAwareness,
                        CommandParameter = _a,
                        IsChecked = _a.IsInSelection
                    });
                }
            }
        }
        #endregion

        #region private void SubSelect_Executed(PresentationInfo present)
        private void SubSelect_Executed(PresentationInfo present)
        {
            if (present != null)
            {
                var _aware = ObservableActor.Awarenesses.FirstOrDefault(_a => present.PresentingIDs.First() == _a.ID);
                var _ctx = new ContextMenu()
                {
                    Resources = ObservableActor.Resources
                };
                var _root = new MenuItem
                {
                    Header = _aware.Info,
                    Command = PickAwareness,
                    CommandParameter = _aware,
                    IsChecked = _aware.IsInSelection
                };
                _ctx.Items.Add(_root);

                if (_aware.Items.Any())
                {
                    _ctx.Items.Add(new Separator());
                    AddSubSelectMenus(_ctx.Items, _aware);
                }

                _ctx.IsOpen = true;
            }
        }
        #endregion

        #region IPresentationInputBinder Members

        public IEnumerable<InputBinding> GetBindings(Presentable presentable)
        {
            if (presentable.Presenter is PresentationInfo _info)
            {
                yield return new MouseBinding(Select, new MouseGesture(MouseAction.LeftClick))
                {
                    CommandParameter = _info
                };
                yield return new MouseBinding(AlterSelection, new MouseGesture(MouseAction.LeftClick, ModifierKeys.Shift))
                {
                    CommandParameter = _info
                };
                yield return new MouseBinding(ContextSelect, new MouseGesture(MouseAction.RightClick))
                {
                    CommandParameter = _info
                };
                if (presentable.Presentations.Any(_p => _p is IconPresentationInfo))
                {
                    yield return new MouseBinding(ZoomIcon, new MouseGesture(MouseAction.RightClick, ModifierKeys.Shift))
                    {
                        CommandParameter = _info
                    };
                }
                yield return new MouseBinding(SubSelect, new MouseGesture(MouseAction.LeftClick, ModifierKeys.Control))
                {
                    CommandParameter = _info
                };
                yield return new MouseBinding(Grab, new MouseGesture(MouseAction.RightClick, ModifierKeys.Control))
                {
                    CommandParameter = _info
                };

                /*
                yield return new MouseBinding(Awareness.AlterSubSelection, new MouseGesture(MouseAction.LeftClick, ModifierKeys.Control | ModifierKeys.Shift))
                {
                    CommandParameter = presentable.Presenter,
                    CommandTarget = Awareness
                };
                 */
            }
            yield break;
        }

        #endregion

        public CellPosition AdjacentFacingCell
            => CellPosition.GetAdjacentCellPosition(
                Sensors.AimCell,
                AnchorFaceHelper.MovementFaces(Sensors.GravityAnchorFace, Sensors.Heading, Sensors.Incline).ToArray());

        #region Look Turns
        private bool LookTurn_CanExecute(string direction)
            => true;

        private void SetHeading(int heading)
        {
            SetSensorHostHeading(heading, Sensors.Incline);
        }

        private void SetIncline(int incline)
        {
            // notify
            SetSensorHostHeading(Sensors.Heading, incline);
        }

        private void LookTurn_Executed(string direction)
        {
            switch (direction)
            {
                case @"Left":
                    SetHeading((Sensors.Heading + 1) % 8);
                    break;

                case @"Right":
                    SetHeading((Sensors.Heading + 7) % 8);
                    break;

                case @"Down":
                    {
                        var _incline = Sensors.Incline + 1;
                        if (_incline > 2)
                            _incline = 2;
                        SetIncline(_incline);
                    }
                    break;

                case @"Up":
                    {
                        var _incline = Sensors.Incline - 1;
                        if (_incline < -2)
                            _incline = -2;
                        SetIncline(_incline);
                    }
                    break;

                case @"BoardLeft":
                    if (ViewPointType == ViewPointType.GameBoard)
                    {
                        var _state = new GameBoardViewPointState(ViewPointState);
                        _state.Heading = (_state.Heading + 7) % 8;
                        _GameBoardState = _state.Value;
                        StoreViewPointState();
                        DoPropertyChanged(nameof(ViewPointState));
                        DoPropertyChanged(nameof(ForwardHeading));
                    }
                    else if (ViewPointType == ViewPointType.ThirdPerson)
                    {
                        var _heading = Sensors.ThirdCameraRelativeHeading;
                        switch (_heading)
                        {
                            case 4:
                            case 5:
                                _heading--;
                                break;
                        }
                        SetSensorHostThirdCamera(_heading, ObservableActor.LocaleViewModel.Sensors.Incline);
                    }
                    break;

                case @"BoardRight":
                    if (ViewPointType == ViewPointType.GameBoard)
                    {
                        var _state = new GameBoardViewPointState(ViewPointState);
                        _state.Heading = (_state.Heading + 1) % 8;
                        _GameBoardState = _state.Value;
                        StoreViewPointState();
                        DoPropertyChanged(nameof(ViewPointState));
                        DoPropertyChanged(nameof(ForwardHeading));
                    }
                    else if (ViewPointType == ViewPointType.ThirdPerson)
                    {
                        var _heading = Sensors.ThirdCameraRelativeHeading;
                        switch (_heading)
                        {
                            case 3:
                            case 4:
                                _heading++;
                                break;
                        }
                        SetSensorHostThirdCamera(_heading, ObservableActor.LocaleViewModel.Sensors.Incline);
                    }
                    break;

                case @"BoardUp":
                    switch (ViewPointType)
                    {
                        case ViewPointType.FirstPerson:
                        case ViewPointType.ThirdPerson:
                            ToggleViewPoint();
                            break;

                        case ViewPointType.GameBoard:
                            var _state = new GameBoardViewPointState(ViewPointState)
                            {
                                Above = true
                            };
                            _GameBoardState = _state.Value;
                            StoreViewPointState();
                            DoPropertyChanged(nameof(ViewPointState));
                            break;

                        case ViewPointType.AimPoint:
                        case ViewPointType.Undefined:
                        default:
                            break;
                    }
                    break;

                case @"BoardDown":
                    if (ViewPointType == ViewPointType.GameBoard)
                    {
                        var _state = new GameBoardViewPointState(ViewPointState);
                        if (!_state.Above)
                        {
                            ToggleViewPoint();
                        }
                        else
                        {
                            _state.Above = false;
                            _GameBoardState = _state.Value;
                            StoreViewPointState();
                            DoPropertyChanged(nameof(ViewPointState));
                        }
                    }
                    break;

                default:
                    break;
            }
        }
        #endregion

        #region Gaze
        private bool Gaze_CanExecute()
            => (_ViewPoint == ViewPointType.GameBoard);

        private void Gaze_Executed()
        {
            switch (_ViewPoint)
            {
                case ViewPointType.GameBoard:
                    var _state = new GameBoardViewPointState(ViewPointState);
                    _state.Gaze = !_state.Gaze;
                    _GameBoardState = _state.Value;
                    StoreViewPointState();
                    DoPropertyChanged(nameof(ViewPointState));
                    break;
            }
        }
        #endregion

        #region ViewFieldChange
        private bool ViewFieldChange_CanExecute(string mode)
            => (_ViewPoint == ViewPointType.ThirdPerson)
            || (_ViewPoint == ViewPointType.FirstPerson)
            || (_ViewPoint == ViewPointType.GameBoard);

        private void ViewFieldChange_Executed(string mode)
        {
            switch (_ViewPoint)
            {
                case ViewPointType.FirstPerson:
                case ViewPointType.ThirdPerson:
                    {
                        var _state = new ThirdPersonViewPointState(ViewPointState);
                        switch (mode)
                        {
                            case @"Prev": _state.FieldOfView -= 5d; break;
                            case @"Next": _state.FieldOfView += 5d; break;
                            case @"Home": _state.FieldOfView = 115d; break;
                        }
                        _ThirdPersonState = _state.Value;
                    }
                    break;

                case ViewPointType.GameBoard:
                    {
                        var _state = new GameBoardViewPointState(ViewPointState);
                        switch (mode)
                        {
                            case @"Prev": _state.WidthCells = (int)(_state.WidthCells * 0.9m); break;
                            case @"Next": _state.WidthCells = (int)(_state.WidthCells * 1.1m); break;
                            case @"Home": _state.WidthCells = 10; break;
                        }
                        _GameBoardState = _state.Value;
                    }
                    break;
            }

            // store new state and indicate updated
            StoreViewPointState();
            DoPropertyChanged(nameof(ViewPointState));
        }
        #endregion

        #region ToggleViewPoint
        private void ToggleViewPoint()
        {
            switch (_ViewPoint)
            {
                case ViewPointType.FirstPerson:
                case ViewPointType.ThirdPerson:
                    // game board
                    var _state = new GameBoardViewPointState(_GameBoardState);
                    if (!_GBInit)
                    {
                        _state.Heading = ForwardHeading;
                        _GBInit = true;
                    }

                    // cycle to undefined
                    _ViewPoint = ViewPointType.Undefined;
                    DoPropertyChanged(nameof(ViewPointType));

                    // setup for new stuff
                    _GameBoardState = _state.Value;

                    // now the view point
                    _ViewPoint = ViewPointType.GameBoard;
                    DoPropertyChanged(nameof(ForwardHeading));
                    break;

                case ViewPointType.GameBoard:
                default:
                    _ViewPoint = _Sensors.ID.Equals(ObservableActor.ActorID.ToString())
                        ? ViewPointType.ThirdPerson
                        : ViewPointType.FirstPerson;
                    DoPropertyChanged(nameof(ForwardHeading));
                    break;
            }
            StoreViewPointState();
            DoPropertyChanged(nameof(ViewPointType));
            DoPropertyChanged(nameof(ViewPointState));
        }
        #endregion

        #region private void StoreViewPointState()
        public void StoreViewPointState()
        {
            // save for later ...
            ObservableActor.UpdateLocalState((state) =>
            {
                state.GameBoardState = _GameBoardState;
                state.ThirdPersonState = _ThirdPersonState;
                state.ViewPointType = _ViewPoint;
                return state;
            });
        }
        #endregion

        #region public void IntersectToCell()
        public void IntersectToCell()
        {
            var _newX = TargetCell.X;
            var _newY = TargetCell.Y;
            var _newZ = TargetCell.Z;
            switch (Sensors.GravityAnchorFace)
            {
                case AnchorFace.XHigh:
                case AnchorFace.XLow:
                    if (TargetCell.X > (Sensors.AimCellX ?? Sensors.UpperX))
                    {
                        // past the cell/sensors
                        _newX--;
                    }
                    if (TargetCell.Z > (Sensors.AimCellZ ?? Sensors.UpperZ))
                    {
                        // past the cell/sensors
                        _newZ--;
                    }
                    break;

                case AnchorFace.YHigh:
                case AnchorFace.YLow:
                    if (TargetCell.Y > (Sensors.AimCellY ?? Sensors.UpperY))
                    {
                        // past the cell/sensors
                        _newY--;
                    }
                    if (TargetCell.Z > (Sensors.AimCellZ ?? Sensors.UpperZ))
                    {
                        // past the cell/sensors
                        _newZ--;
                    }
                    break;

                case AnchorFace.ZHigh:
                case AnchorFace.ZLow:
                default:
                    if (TargetCell.X > (Sensors.AimCellX ?? Sensors.UpperX))
                    {
                        // past the cell/sensors
                        _newX--;
                    }
                    if (TargetCell.Y > (Sensors.AimCellY ?? Sensors.UpperY))
                    {
                        // past the cell/sensors
                        _newY--;
                    }
                    break;
            }

            // calc new cell
            var _newCell = new CellPosition(_newZ, _newY, _newX);

            // set new cell
            _TargetCell = _newCell;
            DoPropertyChanged(nameof(TargetCell));
        }
        #endregion

        #region public void CellToIntersect()
        public void CellToIntersect()
        {
            var _newX = TargetCell.X;
            var _newY = TargetCell.Y;
            var _newZ = TargetCell.Z;
            switch (Sensors.GravityAnchorFace)
            {
                case AnchorFace.XHigh:
                    #region xHigh
                    if (TargetCell.Y > (Sensors.AimCellY ?? Sensors.UpperY))
                        // past the cell/sensors
                        _newY++;
                    else if (TargetCell.Y >= (Sensors.AimCellY ?? Sensors.LowerY))
                        // inside the cell/sensors
                        if (TargetCell.Z < (Sensors.AimCellZ ?? Sensors.LowerZ))
                            // past the cell/sensors
                            _newY++;
                        else if (TargetCell.Z <= (Sensors.AimCellZ ?? Sensors.UpperZ))
                            // inside the cell/sensors
                            switch (Sensors.Heading)
                            {
                                case 7:
                                case 0:
                                case 1:
                                case 2:
                                    _newY++;
                                    break;
                            }

                    if (TargetCell.Z > (Sensors.AimCellZ ?? Sensors.UpperZ))
                        // past the cell/sensors
                        _newZ++;
                    else if (TargetCell.Z >= (Sensors.AimCellZ ?? Sensors.LowerZ))
                        // inside the cell/sensors
                        if (TargetCell.Y > (Sensors.AimCellY ?? Sensors.UpperY))
                            // past the cell/sensors
                            _newZ++;
                        else if (TargetCell.Y >= (Sensors.AimCellY ?? Sensors.LowerY))
                            // inside the cell/sensors
                            switch (Sensors.Heading)
                            {
                                case 5:
                                case 6:
                                case 7:
                                case 0:
                                    _newZ++;
                                    break;
                            }
                    #endregion
                    break;

                case AnchorFace.XLow:
                    #region xLow
                    if (TargetCell.Y > (Sensors.AimCellY ?? Sensors.UpperY))
                        // past the cell/sensors
                        _newY++;
                    else if (TargetCell.Y >= (Sensors.AimCellY ?? Sensors.LowerY))
                        // inside the cell/sensors
                        if (TargetCell.Z > (Sensors.AimCellZ ?? Sensors.UpperZ))
                            // past the cell/sensors
                            _newY++;
                        else if (TargetCell.Z >= (Sensors.AimCellZ ?? Sensors.LowerZ))
                            // inside the cell/sensors
                            switch (Sensors.Heading)
                            {
                                case 7:
                                case 0:
                                case 1:
                                case 2:
                                    _newY++;
                                    break;
                            }

                    if (TargetCell.Z > (Sensors.AimCellZ ?? Sensors.UpperZ))
                        // past the cell/sensors
                        _newZ++;
                    else if (TargetCell.Z >= (Sensors.AimCellZ ?? Sensors.LowerZ))
                        // inside the cell/sensors
                        if (TargetCell.Y < (Sensors.AimCellY ?? Sensors.LowerY))
                            // past the cell/sensors
                            _newZ++;
                        else if (TargetCell.Y <= (Sensors.AimCellY ?? Sensors.UpperY))
                            // inside the cell/sensors
                            switch (Sensors.Heading)
                            {
                                case 1:
                                case 2:
                                case 3:
                                case 4:
                                    _newZ++;
                                    break;
                            }
                    #endregion
                    break;

                case AnchorFace.YHigh:
                    #region yHigh
                    if (TargetCell.Z > (Sensors.AimCellZ ?? Sensors.UpperZ))
                        // past the cell/sensors
                        _newZ++;
                    else if (TargetCell.Z >= (Sensors.AimCellZ ?? Sensors.LowerZ))
                        // inside the cell/sensors
                        if (TargetCell.X < (Sensors.AimCellX ?? Sensors.LowerX))
                            // past the cell/sensors
                            _newZ++;
                        else if (TargetCell.X <= (Sensors.AimCellX ?? Sensors.UpperX))
                            // inside the cell/sensors
                            switch (Sensors.Heading)
                            {
                                case 7:
                                case 0:
                                case 1:
                                case 2:
                                    _newZ++;
                                    break;
                            }

                    if (TargetCell.X > (Sensors.AimCellX ?? Sensors.UpperX))
                        // past the cell/sensors
                        _newX++;
                    else if (TargetCell.X >= (Sensors.AimCellX ?? Sensors.LowerX))
                        // inside the cell/sensors
                        if (TargetCell.Z > (Sensors.AimCellZ ?? Sensors.UpperZ))
                            // past the cell/sensors
                            _newX++;
                        else if (TargetCell.Z >= (Sensors.AimCellZ ?? Sensors.LowerZ))
                            // inside the cell/sensors
                            switch (Sensors.Heading)
                            {
                                case 5:
                                case 6:
                                case 7:
                                case 0:
                                    _newX++;
                                    break;
                            }
                    #endregion
                    break;

                case AnchorFace.YLow:
                    #region yLow
                    if (TargetCell.Z > (Sensors.AimCellZ ?? Sensors.UpperZ))
                        // past the cell/sensors
                        _newZ++;
                    else if (TargetCell.Z >= (Sensors.AimCellZ ?? Sensors.LowerZ))
                        // inside the cell/sensors
                        if (TargetCell.X > (Sensors.AimCellX ?? Sensors.UpperX))
                            // past the cell/sensors
                            _newZ++;
                        else if (TargetCell.X >= (Sensors.AimCellX ?? Sensors.LowerX))
                            // inside the cell/sensors
                            switch (Sensors.Heading)
                            {
                                case 7:
                                case 0:
                                case 1:
                                case 2:
                                    _newZ++;
                                    break;
                            }

                    if (TargetCell.X > (Sensors.AimCellX ?? Sensors.UpperX))
                        // past the cell/sensors
                        _newX++;
                    else if (TargetCell.X >= (Sensors.AimCellX ?? Sensors.LowerX))
                        // inside the cell/sensors
                        if (TargetCell.Z < (Sensors.AimCellZ ?? Sensors.LowerZ))
                            // past the cell/sensors
                            _newX++;
                        else if (TargetCell.Z <= (Sensors.AimCellZ ?? Sensors.UpperZ))
                            // inside the cell/sensors
                            switch (Sensors.Heading)
                            {
                                case 1:
                                case 2:
                                case 3:
                                case 4:
                                    _newX++;
                                    break;
                            }
                    #endregion
                    break;

                case AnchorFace.ZHigh:
                    #region zHigh
                    if (TargetCell.X > (Sensors.AimCellX ?? Sensors.UpperX))
                        // past the cell/sensors
                        _newX++;
                    else if (TargetCell.X >= (Sensors.AimCellX ?? Sensors.LowerX))
                        // inside the cell/sensors
                        if (TargetCell.Y < (Sensors.AimCellY ?? Sensors.LowerY))
                            // past the cell/sensors
                            _newX++;
                        else if (TargetCell.Y <= (Sensors.AimCellY ?? Sensors.UpperY))
                            // inside the cell/sensors
                            switch (Sensors.Heading)
                            {
                                case 7:
                                case 0:
                                case 1:
                                case 2:
                                    _newX++;
                                    break;
                            }

                    if (TargetCell.Y > (Sensors.AimCellY ?? Sensors.UpperY))
                        // past the cell/sensors
                        _newY++;
                    else if (TargetCell.Y >= (Sensors.AimCellY ?? Sensors.LowerY))
                        // inside the cell/sensors
                        if (TargetCell.X > (Sensors.AimCellX ?? Sensors.UpperX))
                            // past the cell/sensors
                            _newY++;
                        else if (TargetCell.X >= (Sensors.AimCellX ?? Sensors.LowerX))
                            // inside the cell/sensors
                            switch (Sensors.Heading)
                            {
                                case 5:
                                case 6:
                                case 7:
                                case 0:
                                    _newY++;
                                    break;
                            }
                    #endregion
                    break;

                case AnchorFace.ZLow:
                default:
                    #region zLow
                    if (TargetCell.X > (Sensors.AimCellX ?? Sensors.UpperX))
                        // past the cell/sensors
                        _newX++;
                    else if (TargetCell.X >= (Sensors.AimCellX ?? Sensors.LowerX))
                        // inside the cell/sensors
                        if (TargetCell.Y > (Sensors.AimCellY ?? Sensors.UpperY))
                            // past the cell/sensors
                            _newX++;
                        else if (TargetCell.Y >= (Sensors.AimCellY ?? Sensors.LowerY))
                            // inside the cell/sensors
                            switch (Sensors.Heading)
                            {
                                case 7:
                                case 0:
                                case 1:
                                case 2:
                                    _newX++;
                                    break;
                            }

                    if (TargetCell.Y > (Sensors.AimCellY ?? Sensors.UpperY))
                        // past the cell/sensors
                        _newY++;
                    else if (TargetCell.Y >= (Sensors.AimCellY ?? Sensors.LowerY))
                        // inside the cell/sensors
                        if (TargetCell.X < (Sensors.AimCellX ?? Sensors.LowerX))
                            // past the cell/sensors
                            _newY++;
                        else if (TargetCell.X <= (Sensors.AimCellX ?? Sensors.UpperX))
                            // inside the cell/sensors
                            switch (Sensors.Heading)
                            {
                                case 1:
                                case 2:
                                case 3:
                                case 4:
                                    _newY++;
                                    break;
                            }
                    #endregion
                    break;
            }

            // calc new cell
            var _newCell = new CellPosition(_newZ, _newY, _newX);

            // set new cell
            _TargetCell = _newCell;
            DoPropertyChanged(nameof(TargetCell));
        }
        #endregion
    }
}
