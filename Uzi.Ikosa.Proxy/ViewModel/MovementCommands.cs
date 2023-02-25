using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Visualize;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class MovementCommands : ViewModelBase
    {
        #region ctor()
        public MovementCommands(ObservableActor actor)
        {
            _ObservableActor = actor;
            _FreeYaw = 4;
            _MaxYaw = 4;
            _YawGap = 0d;
            var _state = actor.GetLocalState();
            _MoveStart = _state.MoveStart;
            _UseDouble = _state.UseDouble;
            _Move = new RelayCommand<MoveParameters>(Move_Execute, Move_CanExecute);
            _Shift = new RelayCommand<MoveParameters>(ShiftPosition_Executed, ShiftPosition_CanExecute);
            _MultiStep = new RelayCommand(MultiStep_Executed, MultiStep_CanExecute);
            _ShiftFacing = new RelayCommand(ShiftFacing_Executed, ShiftFacing_CanExecute);
            _Tumble = new RelayCommand(Tumble_Executed, Tumble_CanExecute);
            _Overrun = new RelayCommand(Overrun_Executed, Overrun_CanExecute);
            _DropProne = new RelayCommand(DropProne_Executed, DropProne_CanExecute);
            _StandUp = new RelayCommand(StandUp_Executed, StandUp_CanExecute);
            _Crawl = new RelayCommand(Crawl_Executed, Crawl_CanExecute);
            _PrevMove = new RelayCommand(PrevMove_Executed, PrevMove_CanExecute);
            _NextMove = new RelayCommand(NextMove_Executed, NextMove_CanExecute);
            _PrevStart = new RelayCommand(PrevStart_Executed, PrevStart_CanExecute);
            _NextStart = new RelayCommand(NextStart_Executed, NextStart_CanExecute);
            _Double = new RelayCommand(DoubleCmd_Executed, DoubleCmd_CanExecute);
            _JumpDown = new RelayCommand(JumpDown_Executed, JumpDown_CanExecute);
            _HopUp = new RelayCommand(HopUp_Executed, HopUp_CanExecute);
        }
        #endregion

        #region data
        private readonly ObservableActor _ObservableActor;
        private MovementInfo _Movement;
        private MoveStart _MoveStart;
        private bool _UseDouble;
        private int _FreeYaw;
        private int _MaxYaw;
        private double _YawGap;
        private readonly RelayCommand<MoveParameters> _Move;
        private readonly RelayCommand<MoveParameters> _Shift;
        private readonly RelayCommand _MultiStep;
        private readonly RelayCommand _ShiftFacing;
        private readonly RelayCommand _Tumble;
        private readonly RelayCommand _Overrun;
        private readonly RelayCommand _DropProne;
        private readonly RelayCommand _StandUp;
        private readonly RelayCommand _Crawl;
        private readonly RelayCommand _PrevMove;
        private readonly RelayCommand _NextMove;
        private readonly RelayCommand _PrevStart;
        private readonly RelayCommand _NextStart;
        private readonly RelayCommand _Double;
        private readonly RelayCommand _JumpDown;
        private readonly RelayCommand _HopUp;
        #endregion

        private List<MovementInfo> Movements
            => ObservableActor.Actor.CreatureModel?.Movements;

        public ObservableActor ObservableActor => _ObservableActor;
        public RelayCommand<MoveParameters> MoveCommand => _Move;
        public RelayCommand<MoveParameters> ShiftCommand => _Shift;
        public RelayCommand MultiStepMoveCommand => _MultiStep;
        public RelayCommand ShiftFacingCommand => _ShiftFacing;
        public RelayCommand TumbleCommand => _Tumble;
        public RelayCommand OverrunCommand => _Overrun;
        public RelayCommand DropProneCommand => _DropProne;
        public RelayCommand StandUpCommand => _StandUp;
        public RelayCommand CrawlCommand => _Crawl;
        public RelayCommand PrevMoveCommand => _PrevMove;
        public RelayCommand NextMoveCommand => _NextMove;
        public RelayCommand PrevStartCommand => _PrevStart;
        public RelayCommand NextStartCommand => _NextStart;
        public RelayCommand UseDoubleCommand => _Double;
        public RelayCommand JumpDownCommand => _JumpDown;
        public RelayCommand HopUpCommand => _HopUp;

        #region private void DoMoveAction(ActionInfo action, int headingOffSet, int upDown, bool? doubleMove)
        private void DoMoveAction(ActionInfo action, int headingOffSet, int upDown, bool? doubleMove)
        {
            var _targets = new List<AimTargetInfo>
            {
                new HeadingTargetInfo
                {
                    Key = action.AimingModes.OfType<MovementAimInfo>().First().Key,
                    Heading = ControlHeadingOffset(headingOffSet),
                    UpDownAdjust = upDown
                }
            };
            if (doubleMove.HasValue)
            {
                _targets.Add(new OptionTargetInfo
                {
                    Key = @"Double",
                    OptionKey = (doubleMove ?? false) ? @"True" : @"False"
                });
            }

            // start movement
            var _activity = new ActivityInfo
            {
                ActionID = action.ID,
                ActionKey = action.Key,
                ActorID = ObservableActor.Actor.CreatureLoginInfo.ID,
                Targets = _targets.ToArray()
            };
            ObservableActor.ClearQueuedLocations();
            ObservableActor.Actor.PerformAction(_activity);
        }
        #endregion

        #region private void DoMultiStepAction(ActionInfo action, bool? doubleMove)
        private void DoMultiStepAction(ActionInfo action, bool? doubleMove)
        {
            var _targets = new List<AimTargetInfo>();

            var _tCell = ObservableActor.LocaleViewModel.TargetCell;
            var _sCell = ObservableActor.LocaleViewModel.Sensors.AimCell;
            var _diff = _sCell.Subtract(_tCell);

            _targets.Add(new MultiStepDestinationInfo
            {
                Key = action.AimingModes.OfType<MovementAimInfo>().First().Key,
                XSteps = _diff.X,
                YSteps = _diff.Y,
                ZSteps = _diff.Z
            });

            if (doubleMove.HasValue)
            {
                _targets.Add(new OptionTargetInfo
                {
                    Key = @"Double",
                    OptionKey = (doubleMove ?? false) ? @"True" : @"False"
                });
            }

            // start movement
            var _activity = new ActivityInfo
            {
                ActionID = action.ID,
                ActionKey = action.Key,
                ActorID = ObservableActor.Actor.CreatureLoginInfo.ID,
                Targets = _targets.ToArray()
            };
            ObservableActor.Actor.PerformAction(_activity);
        }
        #endregion

        public void EnsureMovement()
        {
            if (!Movements.Any(_m => _m.ID == SelectedMovement.ID))
            {
                SelectedMovement = Movements.FirstOrDefault();
            }
        }

        private int ControlHeadingOffset(int heading)
            => (ControlHeading + heading) % 8;

        public int ControlHeading
            => ObservableActor.LocaleViewModel.ForwardHeading;

        public int ControlIncline
            => ObservableActor.LocaleViewModel.Sensors.Incline;

        #region public MovementInfo SelectedMovement { get; set; }
        public MovementInfo SelectedMovement
        {
            get
            {
                if (_Movement == null)
                {
                    var _id = ObservableActor.GetLocalState().MovementInfoID;
                    _Movement = Movements?.FirstOrDefault(_m => _m.ID == _id) ?? Movements?.FirstOrDefault();
                }
                return _Movement;
            }
            set
            {
                _Movement = Movements.FirstOrDefault(_mv => _mv.ID == value?.ID);
                if (_Movement is FlightMovementInfo)
                {
                    var _flight = _Movement as FlightMovementInfo;
                    FreeYaw = _flight.FreeYaw;
                    MaxYaw = _flight.MaxYaw;
                    YawGap = _flight.YawGap;
                }
                else
                {
                    FreeYaw = 4;
                    MaxYaw = 4;
                    YawGap = 0d;
                }
                ObservableActor.UpdateLocalState((state) =>
                    {
                        state.MovementInfoID = _Movement.ID;
                        return state;
                    });
                DoPropertyChanged(nameof(SelectedMovement));
            }
        }
        #endregion

        #region public MoveStart SelectedMovementStart { get; set; }
        public MoveStart SelectedMovementStart
        {
            get => _MoveStart;
            set
            {
                _MoveStart = value;
                ObservableActor.UpdateLocalState((state) =>
                    {
                        state.MoveStart = _MoveStart;
                        return state;
                    });
                DoPropertyChanged(nameof(SelectedMovementStart));
            }
        }
        #endregion

        #region public bool UseDoubleMove { get; set; }
        public bool UseDoubleMove
        {
            get => _UseDouble;
            set
            {
                _UseDouble = value;
                ObservableActor.UpdateLocalState((state) =>
                {
                    state.UseDouble = _UseDouble;
                    return state;
                });
                DoPropertyChanged(nameof(UseDoubleMove));
            }
        }
        #endregion

        #region public int FreeYaw { get; set; }
        public int FreeYaw
        {
            get { return _FreeYaw; }
            set
            {
                _FreeYaw = value;
                DoPropertyChanged(@"FreeYaw");
            }
        }
        #endregion

        #region public int MaxYaw { get; set; }
        public int MaxYaw
        {
            get { return _MaxYaw; }
            set
            {
                _MaxYaw = value;
                DoPropertyChanged(@"MaxYaw");
            }
        }
        #endregion

        #region public double YawGap { get; set; }
        public double YawGap
        {
            get { return _YawGap; }
            set
            {
                _YawGap = value;
                DoPropertyChanged(@"YawGap");
            }
        }
        #endregion

        #region Move Command

        private bool Move_CanExecute(MoveParameters parameters)
        {
            if (ObservableActor.IsMoveEnabled)
            {
                if (ObservableActor.Actor.CanTakeTurn)
                {
                    var _move = SelectedMovement;
                    if (_move != null)
                    {
                        // can start?
                        var _start = SelectedMovementStart;
                        var _key = $@"Movement.Start.{_start}";
                        if (ObservableActor.Actor.Actions.Any(_act => _act.Key.Equals(_key, StringComparison.OrdinalIgnoreCase)
                            && _act.ID == _move.ID))
                            return true;

                        // can continue?
                        _key = @"Movement.Continue";
                        if (ObservableActor.Actor.Actions.Any(_act => _act.Key.Equals(_key, StringComparison.OrdinalIgnoreCase)
                            && _act.ID == _move.ID))
                            return true;
                    }
                }
            }
            else
            {
                // "aiming" movement always possible
                return true;
            }
            return false;
        }

        private void Move_Execute(MoveParameters parameters)
        {
            if (ObservableActor.IsMoveEnabled)
            {
                var _move = SelectedMovement;

                var _start = SelectedMovementStart;
                var _key = $@"Movement.Start.{_start}";
                var _action = ObservableActor.Actor.Actions.FirstOrDefault(_act => _act.Key.Equals(_key, StringComparison.OrdinalIgnoreCase)
                    && _act.ID == _move.ID);
                if (_action != null)
                {
                    DoMoveAction(_action, parameters.Heading, parameters.UpDownAdjust, UseDoubleMove);
                }
                else
                {
                    // continue movement
                    _key = @"Movement.Continue";
                    _action = ObservableActor.Actor.Actions.FirstOrDefault(_act => _act.Key.Equals(_key, StringComparison.OrdinalIgnoreCase)
                        && _act.ID == _move.ID);
                    if (_action != null)
                    {
                        DoMoveAction(_action, parameters.Heading, parameters.UpDownAdjust, UseDoubleMove);
                    }
                    else
                    {
                        // really want to refresh
                        ObservableActor.Actor.UpdateActions();
                    }
                }
            }
            else
            {
                switch (ObservableActor.AimPointActivation)
                {
                    case AimPointActivation.TargetIntersection:
                    case AimPointActivation.TargetCell:
                        ObservableActor.LocaleViewModel.MoveTargetPointer(parameters.Heading, parameters.UpDownAdjust);
                        break;

                    case AimPointActivation.SetExtent:
                        var _lat = parameters.UpDownAdjust * 45d;
                        var _long = parameters.Heading * 45d;
                        ObservableActor.LocaleViewModel.SetSensorHostAimExtent(_long, _lat);
                        ObservableActor.AimPointActivation = AimPointActivation.Off;
                        ObservableActor.IsMoveEnabled = true;
                        break;

                    case AimPointActivation.AdjustPoint:
                        var _shorts = MovementShorts(parameters.Heading, parameters.UpDownAdjust);
                        ObservableActor.LocaleViewModel.AimCameraAdjust(_shorts.Item1, _shorts.Item2, _shorts.Item3);
                        break;
                }
            }
        }

        #endregion

        #region private Tuple<short, short, short> MovementShorts(RoutedMoveCommand movementCommand)
        /// <summary>Get Tuple&lt;z, y, x%gt; to be used for adjustments</summary>
        private Tuple<short, short, short> MovementShorts(int heading, int upDown)
        {
            var _faces = new List<AnchorFace>();

            #region relative heading adjustment
            // NOTE: initial heading of eight means straight up or down
            if (heading < 8)
            {
                // "true" heading adjustment based on direction moved and camera heading
                var _sensors = ObservableActor.LocaleViewModel.Sensors;
                var _travelHead = (_sensors.Heading + heading) % 8;
                switch (_travelHead)
                {
                    case 0:
                        AddForwardFace(_sensors, _faces);
                        break;

                    case 1:
                        AddForwardFace(_sensors, _faces);
                        AddLeftFace(_sensors, _faces);
                        break;

                    case 2:
                        AddLeftFace(_sensors, _faces);
                        break;

                    case 3:
                        AddBackwardFace(_sensors, _faces);
                        AddLeftFace(_sensors, _faces);
                        break;

                    case 4:
                        AddBackwardFace(_sensors, _faces);
                        break;

                    case 5:
                        AddBackwardFace(_sensors, _faces);
                        AddRightFace(_sensors, _faces);
                        break;

                    case 6:
                        AddRightFace(_sensors, _faces);
                        break;

                    case 7:
                        AddForwardFace(_sensors, _faces);
                        AddRightFace(_sensors, _faces);
                        break;

                    default:
                        // no heading adjustment (up or down only)
                        break;
                }
            }
            #endregion

            #region up-down face adjustment
            // up down face adjustment by gravity
            if (upDown != 0)
            {
                switch (ObservableActor.LocaleViewModel.Sensors.GravityAnchorFace)
                {
                    case AnchorFace.ZLow:
                        _faces.Add(upDown > 0 ? AnchorFace.ZHigh : AnchorFace.ZLow);
                        break;
                    case AnchorFace.ZHigh:
                        _faces.Add(upDown < 0 ? AnchorFace.ZHigh : AnchorFace.ZLow);
                        break;
                    case AnchorFace.YLow:
                        _faces.Add(upDown > 0 ? AnchorFace.YHigh : AnchorFace.YLow);
                        break;
                    case AnchorFace.YHigh:
                        _faces.Add(upDown < 0 ? AnchorFace.YHigh : AnchorFace.YLow);
                        break;
                    case AnchorFace.XLow:
                        _faces.Add(upDown > 0 ? AnchorFace.XHigh : AnchorFace.XLow);
                        break;
                    case AnchorFace.XHigh:
                        _faces.Add(upDown < 0 ? AnchorFace.XHigh : AnchorFace.XLow);
                        break;
                }
            }
            #endregion

            return new Tuple<short, short, short>(
                _faces.Contains(AnchorFace.ZHigh) ? (short)1 : _faces.Contains(AnchorFace.ZLow) ? (short)-1 : (short)0,
                _faces.Contains(AnchorFace.YHigh) ? (short)1 : _faces.Contains(AnchorFace.YLow) ? (short)-1 : (short)0,
                _faces.Contains(AnchorFace.XHigh) ? (short)1 : _faces.Contains(AnchorFace.XLow) ? (short)-1 : (short)0
                );
        }

        #region private void AddRightFace(SensorHostInfo sensors, List<AnchorFace> faces)
        private void AddRightFace(SensorHostInfo sensors, List<AnchorFace> faces)
        {
            switch (sensors.GravityAnchorFace)
            {
                case AnchorFace.ZLow:
                    faces.Add(AnchorFace.YLow);
                    break;
                case AnchorFace.ZHigh:
                    faces.Add(AnchorFace.YHigh);
                    break;
                case AnchorFace.YLow:
                    faces.Add(AnchorFace.ZHigh);
                    break;
                case AnchorFace.YHigh:
                    faces.Add(AnchorFace.ZLow);
                    break;
                case AnchorFace.XLow:
                case AnchorFace.XHigh:
                    faces.Add(AnchorFace.YLow);
                    break;
            }
        }
        #endregion

        #region private void AddLeftFace(SensorHostInfo sensors, List<AnchorFace> faces)
        private void AddLeftFace(SensorHostInfo sensors, List<AnchorFace> faces)
        {
            switch (sensors.GravityAnchorFace)
            {
                case AnchorFace.ZLow:
                    faces.Add(AnchorFace.YHigh);
                    break;
                case AnchorFace.ZHigh:
                    faces.Add(AnchorFace.YLow);
                    break;
                case AnchorFace.YLow:
                    faces.Add(AnchorFace.ZLow);
                    break;
                case AnchorFace.YHigh:
                    faces.Add(AnchorFace.ZHigh);
                    break;
                case AnchorFace.XLow:
                case AnchorFace.XHigh:
                    faces.Add(AnchorFace.YHigh);
                    break;
            }
        }
        #endregion

        #region private void AddBackwardFace(SensorHostInfo sensors, List<AnchorFace> faces)
        private void AddBackwardFace(SensorHostInfo sensors, List<AnchorFace> faces)
        {
            switch (sensors.GravityAnchorFace)
            {
                case AnchorFace.ZLow:
                case AnchorFace.ZHigh:
                case AnchorFace.YLow:
                case AnchorFace.YHigh:
                    faces.Add(AnchorFace.XLow);
                    break;
                case AnchorFace.XLow:
                    faces.Add(AnchorFace.ZHigh);
                    break;
                case AnchorFace.XHigh:
                    faces.Add(AnchorFace.ZLow);
                    break;
            }
        }
        #endregion

        #region private void AddForwardFace(SensorHostInfo sensors, List<AnchorFace> faces)
        private void AddForwardFace(SensorHostInfo sensors, List<AnchorFace> faces)
        {
            // forward
            switch (sensors.GravityAnchorFace)
            {
                case AnchorFace.ZLow:
                case AnchorFace.ZHigh:
                case AnchorFace.YLow:
                case AnchorFace.YHigh:
                    faces.Add(AnchorFace.XHigh);
                    break;
                case AnchorFace.XLow:
                    faces.Add(AnchorFace.ZLow);
                    break;
                case AnchorFace.XHigh:
                    faces.Add(AnchorFace.ZHigh);
                    break;
            }
        }
        #endregion

        #endregion

        #region Jump
        private bool Jump_CanExecute()
        {
            return false;
            // TODO: if jump key is in action list
        }

        private void Jump_Executed()
        {
        }
        #endregion

        #region Tumble
        private bool Tumble_CanExecute()
            => ObservableActor.Actor.CanTakeTurn
            && ObservableActor.Actor.Actions.Any(_a => _a.Key.Equals(@"Movement.Tumble", StringComparison.OrdinalIgnoreCase));

        private void Tumble_Executed()
        {
            var _act = ObservableActor.Actor.Actions
                .FirstOrDefault(_a => _a.Key.Equals(@"Movement.Tumble", StringComparison.OrdinalIgnoreCase));
            if (_act != null)
            {
                DoMoveAction(_act, 0, 0, UseDoubleMove);
            }
            else
            {
                // really want to refresh
                ObservableActor.Actor.UpdateActions();
            }
        }
        #endregion

        #region Overrun
        private bool Overrun_CanExecute()
        {
            var _move = SelectedMovement;
            if (ObservableActor.Actor.CanTakeTurn
                && ObservableActor.Actor.Actions.Any(_act =>
                    _act.Key.Equals(@"Movement.Overrun", StringComparison.OrdinalIgnoreCase)
                    && _act.ID == _move.ID))
            {
                switch (ObservableActor.AimPointActivation)
                {
                    case AimPointActivation.TargetCell:
                        var _tCell = ObservableActor.LocaleViewModel.TargetCell;
                        var _sCell = ObservableActor.LocaleViewModel.Sensors.AimCell;
                        var _diff = _sCell.Subtract(_tCell);
                        if (_diff.X == 0 && _diff.Y == 0 && _diff.Z == 0)
                            return false;
                        return true;

                    case AimPointActivation.TargetIntersection:
                        return false;

                    default:
                        return true;
                }
            }
            return false;
        }

        private void Overrun_Executed()
        {
            var _move = SelectedMovement;
            var _action = ObservableActor.Actor.Actions.FirstOrDefault(_act =>
                _act.Key.Equals(@"Movement.Overrun", StringComparison.OrdinalIgnoreCase)
                && _act.ID == _move.ID);
            if (_action != null)
            {
                switch (ObservableActor.AimPointActivation)
                {
                    case AimPointActivation.TargetCell:
                        DoMultiStepAction(_action, false);
                        break;

                    case AimPointActivation.TargetIntersection:
                        break;

                    default:
                        DoMoveAction(_action, 0, Convert.ToInt32(ControlIncline), false);
                        break;
                }
            }
            else
            {
                // really want to refresh
                ObservableActor.Actor.UpdateActions();
            }
        }
        #endregion

        #region Drop Prone
        private bool DropProne_CanExecute()
        {
            return ObservableActor.Actor.CanTakeTurn &&
                ObservableActor.Actor.Actions.Any(_a => _a.Key.Equals(@"DropProne", StringComparison.OrdinalIgnoreCase));
        }

        private void DropProne_Executed()
        {
            var _act = ObservableActor.Actor.Actions.FirstOrDefault(_a => _a.Key.Equals(@"DropProne", StringComparison.OrdinalIgnoreCase));
            if (_act != null)
            {
                // start movement
                var _activity = new ActivityInfo
                {
                    ActionID = _act.ID,
                    ActionKey = _act.Key,
                    ActorID = ObservableActor.Actor.CreatureLoginInfo.ID
                };
                ObservableActor.Actor.PerformAction(_activity);
            }
            else
            {
                ObservableActor.Actor.UpdateActions();
            }
        }
        #endregion

        #region Stand Up
        private bool StandUp_CanExecute()
        {
            return ObservableActor.Actor.CanTakeTurn &&
                ObservableActor.Actor.Actions.Any(_a => _a.Key.Equals(@"Movement.StandUp", StringComparison.OrdinalIgnoreCase));
        }

        private void StandUp_Executed()
        {
            var _act = ObservableActor.Actor.Actions.FirstOrDefault(_a => _a.Key.Equals(@"Movement.StandUp", StringComparison.OrdinalIgnoreCase));
            if (_act != null)
            {
                // start movement
                var _activity = new ActivityInfo
                {
                    ActionID = _act.ID,
                    ActionKey = _act.Key,
                    ActorID = ObservableActor.Actor.CreatureLoginInfo.ID
                };
                ObservableActor.Actor.PerformAction(_activity);
            }
            else
            {
                // really want to refresh
                ObservableActor.Actor.UpdateActions();
            }
        }
        #endregion

        #region Crawl
        private bool Crawl_CanExecute()
        {
            return ObservableActor.Actor.CanTakeTurn &&
                ObservableActor.Actor.Actions.Any(_a => _a.Key.Equals(@"Movement.Crawl", StringComparison.OrdinalIgnoreCase));
        }

        private void Crawl_Executed()
        {
            var _act = ObservableActor.Actor.Actions.FirstOrDefault(_a => _a.Key.Equals(@"Movement.Crawl", StringComparison.OrdinalIgnoreCase));
            if (_act != null)
            {
                DoMoveAction(_act, 0, 0, UseDoubleMove);
            }
            else
            {
                // really want to refresh
                ObservableActor.Actor.UpdateActions();
            }
        }
        #endregion

        #region Multi-Step Movement

        private bool MultiStep_CanExecute()
        {
            // make sure regular movement commands are NOT enabled!
            if (!ObservableActor.IsMoveEnabled)
            {
                if (ObservableActor.Actor.CanTakeTurn)
                {
                    switch (ObservableActor.AimPointActivation)
                    {
                        case AimPointActivation.TargetCell:
                        case AimPointActivation.TargetIntersection:
                            var _tCell = ObservableActor.LocaleViewModel.TargetCell;
                            var _sCell = ObservableActor.LocaleViewModel.Sensors.AimCell;
                            var _diff = _sCell.Subtract(_tCell);
                            if (_diff.X == 0 && _diff.Y == 0 && _diff.Z == 0)
                                return false;

                            // multi-step movement only when aiming a cell
                            var _move = SelectedMovement;
                            if (_move != null)
                            {
                                // can start?
                                var _start = SelectedMovementStart;
                                var _key = $@"Movement.Start.{_start}";
                                if (ObservableActor.Actor.Actions.Any(_act => _act.Key.Equals(_key, StringComparison.OrdinalIgnoreCase)
                                    && _act.ID == _move.ID))
                                    return true;

                                // can continue?
                                _key = @"Movement.Continue";
                                if (ObservableActor.Actor.Actions.Any(_act => _act.Key.Equals(_key, StringComparison.OrdinalIgnoreCase)
                                    && _act.ID == _move.ID))
                                    return true;
                            }
                            break;
                    }
                }
            }
            return false;
        }

        private void MultiStep_Executed()
        {
            var _move = SelectedMovement;

            var _start = SelectedMovementStart;
            var _key = $@"Movement.Start.{_start}";
            var _action = ObservableActor.Actor.Actions
                .FirstOrDefault(_act =>
                    _act.Key.Equals(_key, StringComparison.OrdinalIgnoreCase)
                    && _act.ID == _move.ID);
            if (_action != null)
            {
                DoMultiStepAction(_action, UseDoubleMove);
            }
            else
            {
                // continue movement
                _key = @"Movement.Continue";
                _action = ObservableActor.Actor.Actions
                    .FirstOrDefault(_act =>
                        _act.Key.Equals(_key, StringComparison.OrdinalIgnoreCase)
                        && _act.ID == _move.ID);
                if (_action != null)
                {
                    DoMultiStepAction(_action, UseDoubleMove);
                }
                else
                {
                    // really want to refresh
                    ObservableActor.Actor.UpdateActions();
                }
            }
        }

        #endregion

        #region Shift Position
        private bool ShiftPosition_CanExecute(MoveParameters parameters)
            => ShiftFacing_CanExecute();

        private void ShiftPosition_Executed(MoveParameters parameters)
        {
            if (ObservableActor.IsMoveEnabled)
            {
                var _act = ObservableActor.Actor.Actions.FirstOrDefault(_a => _a.Key.Equals(@"Movement.ShiftPosition", StringComparison.OrdinalIgnoreCase));
                if (_act != null)
                {
                    DoMoveAction(_act, parameters.Heading, parameters.UpDownAdjust, UseDoubleMove);
                }
                else
                {
                    // really want to refresh
                    ObservableActor.Actor.UpdateActions();
                }
            }
        }

        private bool ShiftFacing_CanExecute()
            => ObservableActor.Actor.CanTakeTurn
            && ObservableActor.IsMoveEnabled
            && ObservableActor.Actor.Actions.Any(_a => _a.Key.Equals(@"Movement.ShiftPosition", StringComparison.OrdinalIgnoreCase));

        private void ShiftFacing_Executed()
        {
            if (ObservableActor.IsMoveEnabled)
            {
                var _act = ObservableActor.Actor.Actions.FirstOrDefault(_a => _a.Key.Equals(@"Movement.ShiftPosition", StringComparison.OrdinalIgnoreCase));
                if (_act != null)
                {
                    DoMoveAction(_act,
                        ObservableActor.LocaleViewModel.Sensors.Heading - ControlHeading,
                        Convert.ToInt32(ControlIncline),
                        UseDoubleMove);
                }
                else
                {
                    // really want to refresh
                    ObservableActor.Actor.UpdateActions();
                }
            }
        }
        #endregion

        #region JumpDown
        private bool JumpDown_CanExecute()
            => ObservableActor.Actor.CanTakeTurn &&
            ObservableActor.Actor.Actions.Any(_a => _a.Key.Equals(@"Movement.JumpDown", StringComparison.OrdinalIgnoreCase));

        private void JumpDown_Executed()
        {
            var _act = ObservableActor.Actor.Actions.FirstOrDefault(_a => _a.Key.Equals(@"Movement.JumpDown", StringComparison.OrdinalIgnoreCase));
            if (_act != null)
            {
                DoMoveAction(_act, 0, 0, UseDoubleMove);
            }
            else
            {
                // really want to refresh
                ObservableActor.Actor.UpdateActions();
            }
        }
        #endregion

        #region HopUp
        private bool HopUp_CanExecute()
            => ObservableActor.Actor.CanTakeTurn &&
            ObservableActor.Actor.Actions.Any(_a => _a.Key.Equals(@"Movement.HopUp", StringComparison.OrdinalIgnoreCase));

        private void HopUp_Executed()
        {
            var _act = ObservableActor.Actor.Actions.FirstOrDefault(_a => _a.Key.Equals(@"Movement.HopUp", StringComparison.OrdinalIgnoreCase));
            if (_act != null)
            {
                DoMoveAction(_act, 0, 0, UseDoubleMove);
            }
            else
            {
                // really want to refresh
                ObservableActor.Actor.UpdateActions();
            }
        }
        #endregion

        #region PrevMove
        private bool PrevMove_CanExecute()
        {
            return Movements.Any();
        }

        private void PrevMove_Executed()
        {
            if (Movements.Count > 1)
            {
                if (Movements.Any(_m => _m.ID == SelectedMovement.ID))
                {
                    if (SelectedMovement?.ID == Movements[0].ID)
                        // if first, go to last
                        SelectedMovement = Movements.Last();
                    else
                        // go through all movements before the selected one, and take the last one
                        SelectedMovement = Movements.TakeWhile(_m => _m.ID != SelectedMovement?.ID).Last();
                }
                else
                {
                    SelectedMovement = Movements.First();
                }
            }
            else if (Movements.Count == 1)
            {
                SelectedMovement = Movements.First();
            }
        }
        #endregion

        #region NextMove
        private bool NextMove_CanExecute()
        {
            return Movements.Any();
        }

        private void NextMove_Executed()
        {
            if (Movements.Count > 1)
            {
                if (Movements.Any(_m => _m.ID == SelectedMovement.ID))
                {
                    if (SelectedMovement?.ID == Movements.Last()?.ID)
                        SelectedMovement = Movements[0];
                    else
                        SelectedMovement = Movements.SkipWhile(_m => _m.ID != SelectedMovement?.ID).Skip(1).First();
                }
                else
                {
                    SelectedMovement = Movements.First();
                }

            }
            else if (Movements.Count == 1)
            {
                SelectedMovement = Movements.First();
            }
        }
        #endregion

        #region PrevStart
        private bool PrevStart_CanExecute()
        {
            return SelectedMovementStart != MoveStart.Standard;
        }

        private void PrevStart_Executed()
        {
            SelectedMovementStart = SelectedMovementStart - 1;
        }
        #endregion

        #region NextStart
        private bool NextStart_CanExecute()
        {
            return SelectedMovementStart != MoveStart.Withdraw;
        }

        private void NextStart_Executed()
        {
            SelectedMovementStart = SelectedMovementStart + 1;
        }
        #endregion

        #region DoubleCmd
        private bool DoubleCmd_CanExecute()
        {
            return true;
        }

        private void DoubleCmd_Executed()
        {
            UseDoubleMove = !UseDoubleMove;
        }
        #endregion
    }
}
