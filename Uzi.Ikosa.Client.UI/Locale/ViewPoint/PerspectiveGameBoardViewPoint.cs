using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Proxy.ViewModel;
using Uzi.Visualize;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Client.UI
{
    public class PerspectiveGameBoardViewPoint : BaseViewPoint
    {
        public PerspectiveGameBoardViewPoint()
            : base()
        {
            _Camera = new PerspectiveCamera(new Point3D(7.5, 7.5, 10), new Vector3D(1, 1, 0), new Vector3D(0, 0, 1), 105)
            {
                NearPlaneDistance = 0.125,
                FarPlaneDistance = 300
            };
            _GBState = new GameBoardViewPointState();
        }

        #region data
        private ProjectionCamera _Camera;
        private GameBoardViewPointState _GBState;
        #endregion

        #region public override uint ViewPointState { get; set; }
        public override uint ViewPointState
        {
            get => base.ViewPointState;
            set
            {
                var _old = _GBState;
                base.ViewPointState = value;
                _GBState = new GameBoardViewPointState(value);
                if (_old.Value != _GBState.Value)
                {
                    SyncCamera();
                }
            }
        }
        #endregion

        #region private void SyncCamera()
        private void SyncCamera()
        {
            if (SensorHost != null)
            {
                if (!_GBState.Gaze)
                {
                    void _ensureOrtho(Vector3D upDirection, Vector3D lookDirection, Point3D position)
                    {
                        if (!(_Camera is OrthographicCamera))
                        {
                            _Camera = new OrthographicCamera(position, lookDirection, upDirection, BoardWidth)
                            {
                                NearPlaneDistance = 0.125,
                                FarPlaneDistance = BoardWidth * 2
                            };
                            DoPropertyChanged(nameof(ViewPointCamera));
                        }
                        else
                        {
                            _Camera.FarPlaneDistance = BoardWidth * 2;
                            AnimateCamera(BoardWidth, upDirection, lookDirection, position);
                        }
                    }
                    var _gravity = SensorHost.GravityAnchorFace;
                    var _revGravity = _gravity.ReverseFace();
                    var _lookHeading = _gravity.GetHeadingFaces(SensorHost.Heading).GetAnchorOffset().Vector3D();
                    _lookHeading.Normalize();
                    var _headingVector = _gravity.GetHeadingFaces(BoardHeading).GetAnchorOffset().Vector3D();
                    _headingVector.Normalize();
                    var _displace = GazeOffset ? (0.25 * BoardWidth) + (BoardWidth / 260) * 20 - 1 : 0d;
                    if (Overhead)
                    {
                        _ensureOrtho(_headingVector, _gravity.GetNormalVector(), SensorHost.AimCell.GetPoint() + (_revGravity.GetNormalVector() * BoardWidth) + (_lookHeading * _displace));
                    }
                    else
                    {
                        var _look = _gravity.GetNormalVector() + _headingVector;
                        _look.Normalize();
                        var _side = _gravity.GetHeadingFaces(BoardHeading + 2).GetAnchorOffset().Vector3D();
                        _side.Normalize();
                        var _up = _gravity.ReverseFace().GetNormalVector();//  Vector3D.CrossProduct(_look, _side);
                                                                           //  var _up = Vector3D.CrossProduct(_look, _side);
                                                                           //  _up.Normalize();

                        _ensureOrtho(_up, _look, SensorHost.AimCell.GetPoint() + (_look * -1d * BoardWidth) + (_lookHeading * _displace));
                    }
                }
                else
                {
                    void _ensurePerspective(double boardWidth, Vector3D upDirection, Vector3D lookDirection, Point3D position)
                    {
                        if (!(_Camera is PerspectiveCamera))
                        {
                            _Camera = new PerspectiveCamera(position, lookDirection, upDirection, 105)
                            {
                                NearPlaneDistance = 0.125,
                                FarPlaneDistance = BoardWidth * 2
                            };
                            DoPropertyChanged(nameof(ViewPointCamera));
                        }
                        else
                        {
                            _Camera.FarPlaneDistance = BoardWidth * 2;
                            AnimateCamera(boardWidth, upDirection, lookDirection, position);
                        }
                    }

                    var _gravity = SensorHost.GravityAnchorFace;
                    var _revGravity = _gravity.ReverseFace();
                    var _lookHeading = _gravity.GetHeadingFaces(SensorHost.Heading).GetAnchorOffset().Vector3D();
                    _lookHeading.Normalize();
                    var _headingVector = _gravity.GetHeadingFaces(BoardHeading).GetAnchorOffset().Vector3D();
                    _headingVector.Normalize();
                    if (Overhead)
                    {
                        _ensurePerspective(BoardWidth, _headingVector, _gravity.GetNormalVector(), SensorHost.AimCell.GetPoint() + (_revGravity.GetNormalVector() * BoardWidth / 2));
                    }
                    else
                    {
                        var _look = _gravity.GetNormalVector() + _headingVector;
                        _look.Normalize();
                        var _side = _gravity.GetHeadingFaces(BoardHeading + 2).GetAnchorOffset().Vector3D();
                        _side.Normalize();
                        var _up = _gravity.ReverseFace().GetNormalVector();
                        //  var _up = Vector3D.CrossProduct(_look, _side);
                        //  _up.Normalize();
                        _ensurePerspective(BoardWidth * 2, _up, _look, SensorHost.AimCell.GetPoint() + (_look * -1d * BoardWidth / 2));
                    }
                }
            }
        }
        #endregion

        private void AnimateCamera(double boardWidth, Vector3D upDirection, Vector3D lookDirection, Point3D position)
        {
            var _ud = new Vector3DAnimation(_Camera.UpDirection, upDirection, new Duration(TimeSpan.FromMilliseconds(100)), FillBehavior.HoldEnd)
            {
                AccelerationRatio = 0.3,
                DecelerationRatio = 0.5,
            };
            var _ld = new Vector3DAnimation(_Camera.LookDirection, lookDirection, new Duration(TimeSpan.FromMilliseconds(100)), FillBehavior.HoldEnd)
            {
                AccelerationRatio = 0.3,
                DecelerationRatio = 0.5,
            };
            var _p = new Point3DAnimation(_Camera.Position, position, new Duration(TimeSpan.FromMilliseconds(100)), FillBehavior.HoldEnd)
            {
                AccelerationRatio = 0.3,
                DecelerationRatio = 0.5,
            };
            if (_Camera is OrthographicCamera _ortho)
            {
                var _w = new DoubleAnimation(_ortho.Width, boardWidth, new Duration(TimeSpan.FromMilliseconds(100)), FillBehavior.HoldEnd)
                {
                    AccelerationRatio = 0.3,
                    DecelerationRatio = 0.5,
                };
                _Camera.BeginAnimation(OrthographicCamera.WidthProperty, _w);
            }
            _Camera.BeginAnimation(ProjectionCamera.UpDirectionProperty, _ud);
            _Camera.BeginAnimation(ProjectionCamera.LookDirectionProperty, _ld);
            _Camera.BeginAnimation(ProjectionCamera.PositionProperty, _p);
        }

        public int BoardHeading => _GBState.Heading;
        public bool Overhead => _GBState.Above;
        public bool GazeOffset => _GBState.Gaze;
        public double BoardWidth => _GBState.Width;

        public override Camera ViewPointCamera => _Camera;

        public override Point3D ViewPosition
            => _Camera.Position;

        #region public override SensorHostInfo SensorHost { get; set; }
        public override SensorHostInfo SensorHost
        {
            get => base.SensorHost;
            set
            {
                if (value == null) return;

                if (SensorHost != null)
                {
                    var _reorient = SensorHost.GravityFace != value.GravityFace;
                    var _posChange = DoesPositionChange(value);
                    var _headingChange = SensorHost.Heading != value.Heading;

                    base.SensorHost = value;

                    if ((_reorient) || (_posChange) || _headingChange)
                    {
                        SyncCamera();
                    }
                }
                else
                {
                    base.SensorHost = value;
                    SyncCamera();
                }

            }
        }
        #endregion

        protected bool DoesPositionChange(SensorHostInfo value)
            => (SensorHost.Z != value.Z)
            || (SensorHost.Y != value.Y)
            || (SensorHost.X != value.X);
    }
}
