using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Visualize;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Proxy.ViewModel;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Client.UI
{
    public class GameBoardViewPoint : BaseViewPoint
    {
        public GameBoardViewPoint()
            : base()
        {
            _Camera = new OrthographicCamera(new Point3D(7.5, 7.5, 10), new Vector3D(1, 1, 0), new Vector3D(0, 0, 1), 120)
            {
                NearPlaneDistance = 0.125,
                FarPlaneDistance = 300
            };
            _GBState = new GameBoardViewPointState();
        }

        #region data
        private OrthographicCamera _Camera;
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
                var _gravity = SensorHost.GravityAnchorFace;
                var _revGravity = _gravity.ReverseFace();
                var _lookHeading = _gravity.GetHeadingFaces(SensorHost.Heading).GetAnchorOffset().Vector3D();
                _lookHeading.Normalize();
                var _headingVector = _gravity.GetHeadingFaces(BoardHeading).GetAnchorOffset().Vector3D();
                _headingVector.Normalize();
                _Camera.Width = BoardWidth;
                var _displace = GazeOffset ? (0.25 * BoardWidth) + (BoardWidth / 260) * 20 - 1 : 0d;
                if (Overhead)
                {
                    _Camera.FarPlaneDistance = BoardWidth * 2;
                    _Camera.UpDirection = _headingVector;
                    _Camera.Position = SensorHost.AimCell.GetPoint() + (_revGravity.GetNormalVector() * BoardWidth) + (_lookHeading * _displace);
                    _Camera.LookDirection = _gravity.GetNormalVector();
                }
                else
                {
                    var _look = _gravity.GetNormalVector() + _headingVector;
                    _look.Normalize();
                    var _side = _gravity.GetHeadingFaces(BoardHeading + 2).GetAnchorOffset().Vector3D();
                    _side.Normalize();
                    var _up = Vector3D.CrossProduct(_look, _side);
                    _up.Normalize();

                    _Camera.FarPlaneDistance = BoardWidth * 2;
                    _Camera.UpDirection = _up;
                    _Camera.LookDirection = _look;
                    _Camera.Position = SensorHost.AimCell.GetPoint() + (_look * -1d * BoardWidth) + (_lookHeading * _displace);
                }
            }
        }
        #endregion

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
