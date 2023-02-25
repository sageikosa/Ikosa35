using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Proxy.VisualizationSvc;
using System.Windows.Media.Animation;
using System.Windows;
using Uzi.Visualize;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Client.UI
{
    public abstract class PerspectiveViewPoint : BaseViewPoint
    {
        protected PerspectiveViewPoint()
            : base()
        {
            _Camera = new PerspectiveCamera(new Point3D(7.5, 7.5, 10), new Vector3D(1, 1, 0), new Vector3D(0, 0, 1), 90)
            {
                NearPlaneDistance = 0.125,
                FarPlaneDistance = 300
            };
        }

        protected PerspectiveCamera _Camera;

        public override Camera ViewPointCamera { get { return _Camera; } }

        public override bool IsSelfVisible { get { return false; } }

        public override Point3D ViewPosition
            => Position;

        protected virtual Point3D Position
        {
            get
            {
                return new Point3D(
                    (SensorHost.X * 5d) + SensorHost.Offset.X,
                    (SensorHost.Y * 5d) + SensorHost.Offset.Y,
                    (SensorHost.Z * 5d) + SensorHost.Offset.Z);
            }
        }

        #region private void SetPosition(bool animate)
        private void SetPosition(bool animate)
        {
            var _pt = Position;

            // TODO: make animation time dependent on critter's speed
            if (animate)
            {
                var _animate = new Point3DAnimation(_Camera.Position, _pt, new Duration(TimeSpan.FromMilliseconds(100)))
                {
                    AccelerationRatio = 0.3,
                    DecelerationRatio = 0.5,
                };
                _Camera.BeginAnimation(PerspectiveCamera.PositionProperty, _animate);
            }
            else
            {
                _Camera.Position = _pt;
            }
        }
        #endregion

        #region protected void SetLook(bool animate)
        protected void SetLook(bool animate)
        {
            var _lookVector = new Vector3D();
            var _upDirection = new Vector3D();
            var _incline = _SensorHost.Incline * (Math.Abs(_SensorHost.Incline) <= 1 ? 1 : 20);
            switch (SensorHost.GravityAnchorFace)
            {
                case AnchorFace.XHigh:
                    _upDirection = new Vector3D(-1, 0, 0);
                    switch (SensorHost.Heading)
                    {
                        case 0:
                            _lookVector = new Vector3D(-_incline, 0, 1);
                            break;
                        case 1:
                            _lookVector = new Vector3D(-_incline, 1, 1);
                            break;
                        case 2:
                            _lookVector = new Vector3D(-_incline, 1, 0);
                            break;
                        case 3:
                            _lookVector = new Vector3D(-_incline, 1, -1);
                            break;
                        case 4:
                            _lookVector = new Vector3D(-_incline, 0, -1);
                            break;
                        case 5:
                            _lookVector = new Vector3D(-_incline, -1, -1);
                            break;
                        case 6:
                            _lookVector = new Vector3D(-_incline, -1, 0);
                            break;
                        case 7:
                            _lookVector = new Vector3D(-_incline, -1, 1);
                            break;
                    }
                    break;

                case AnchorFace.XLow:
                    _upDirection = new Vector3D(1, 0, 0);
                    switch (SensorHost.Heading)
                    {
                        case 0:
                            _lookVector = new Vector3D(_incline, 0, -1);
                            break;
                        case 1:
                            _lookVector = new Vector3D(_incline, 1, -1);
                            break;
                        case 2:
                            _lookVector = new Vector3D(_incline, 1, 0);
                            break;
                        case 3:
                            _lookVector = new Vector3D(_incline, 1, 1);
                            break;
                        case 4:
                            _lookVector = new Vector3D(_incline, 0, 1);
                            break;
                        case 5:
                            _lookVector = new Vector3D(_incline, -1, 1);
                            break;
                        case 6:
                            _lookVector = new Vector3D(_incline, -1, 0);
                            break;
                        case 7:
                            _lookVector = new Vector3D(_incline, -1, -1);
                            break;
                    }
                    break;

                case AnchorFace.YHigh:
                    _upDirection = new Vector3D(0, -1, 0);
                    switch (SensorHost.Heading)
                    {
                        case 0:
                            _lookVector = new Vector3D(1, -_incline, 0);
                            break;
                        case 1:
                            _lookVector = new Vector3D(1, -_incline, 1);
                            break;
                        case 2:
                            _lookVector = new Vector3D(0, -_incline, 1);
                            break;
                        case 3:
                            _lookVector = new Vector3D(-1, -_incline, 1);
                            break;
                        case 4:
                            _lookVector = new Vector3D(-1, -_incline, 0);
                            break;
                        case 5:
                            _lookVector = new Vector3D(-1, -_incline, -1);
                            break;
                        case 6:
                            _lookVector = new Vector3D(0, -_incline, -1);
                            break;
                        case 7:
                            _lookVector = new Vector3D(1, -_incline, -1);
                            break;
                    }
                    break;

                case AnchorFace.YLow:
                    _upDirection = new Vector3D(0, 1, 0);
                    switch (SensorHost.Heading)
                    {
                        case 0:
                            _lookVector = new Vector3D(1, _incline, 0);
                            break;
                        case 1:
                            _lookVector = new Vector3D(1, _incline, -1);
                            break;
                        case 2:
                            _lookVector = new Vector3D(0, _incline, -1);
                            break;
                        case 3:
                            _lookVector = new Vector3D(-1, _incline, -1);
                            break;
                        case 4:
                            _lookVector = new Vector3D(-1, _incline, 0);
                            break;
                        case 5:
                            _lookVector = new Vector3D(-1, _incline, 1);
                            break;
                        case 6:
                            _lookVector = new Vector3D(0, _incline, 1);
                            break;
                        case 7:
                            _lookVector = new Vector3D(1, _incline, 1);
                            break;
                    }
                    break;

                case AnchorFace.ZHigh:
                    _upDirection = new Vector3D(0, 0, -1);
                    switch (SensorHost.Heading)
                    {
                        case 0:
                            _lookVector = new Vector3D(1, 0, -_incline);
                            break;
                        case 1:
                            _lookVector = new Vector3D(1, -1, -_incline);
                            break;
                        case 2:
                            _lookVector = new Vector3D(0, -1, -_incline);
                            break;
                        case 3:
                            _lookVector = new Vector3D(-1, -1, -_incline);
                            break;
                        case 4:
                            _lookVector = new Vector3D(-1, 0, -_incline);
                            break;
                        case 5:
                            _lookVector = new Vector3D(-1, 1, -_incline);
                            break;
                        case 6:
                            _lookVector = new Vector3D(0, 1, -_incline);
                            break;
                        case 7:
                            _lookVector = new Vector3D(1, 1, _incline);
                            break;
                    }
                    break;

                case AnchorFace.ZLow:
                default:
                    _upDirection = new Vector3D(0, 0, 1);
                    switch (SensorHost.Heading)
                    {
                        case 0:
                            _lookVector = new Vector3D(1, 0, _incline);
                            break;
                        case 1:
                            _lookVector = new Vector3D(1, 1, _incline);
                            break;
                        case 2:
                            _lookVector = new Vector3D(0, 1, _incline);
                            break;
                        case 3:
                            _lookVector = new Vector3D(-1, 1, _incline);
                            break;
                        case 4:
                            _lookVector = new Vector3D(-1, 0, _incline);
                            break;
                        case 5:
                            _lookVector = new Vector3D(-1, -1, _incline);
                            break;
                        case 6:
                            _lookVector = new Vector3D(0, -1, _incline);
                            break;
                        case 7:
                            _lookVector = new Vector3D(1, -1, _incline);
                            break;
                    }
                    break;
            }
            if (_upDirection != _Camera.UpDirection)
                ChangeUpDirection(animate, _upDirection);
            if (_lookVector != _Camera.LookDirection)
                ChangeLookVector(animate, _lookVector);
        }

        private void ChangeUpDirection(bool animate, Vector3D upDirection)
        {
            if (animate)
            {
                var _animate = new Vector3DAnimation(_Camera.UpDirection, upDirection,
                    new Duration(TimeSpan.FromMilliseconds(333)), FillBehavior.HoldEnd)
                {
                    AccelerationRatio = 0.3,
                    DecelerationRatio = 0.5,
                };
                _Camera.BeginAnimation(PerspectiveCamera.UpDirectionProperty, _animate);
            }
            else
                _Camera.UpDirection = upDirection;
        }

        private void ChangeLookVector(bool animate, Vector3D lookVector)
        {
            if (animate)
            {
                var _animate = new Vector3DAnimation(_Camera.LookDirection, lookVector,
                    new Duration(TimeSpan.FromMilliseconds(167)), FillBehavior.HoldEnd)
                {
                    AccelerationRatio = 0.3,
                    DecelerationRatio = 0.5,
                };
                _Camera.BeginAnimation(PerspectiveCamera.LookDirectionProperty, _animate);
            }
            else
                _Camera.LookDirection = lookVector;
        }
        #endregion

        #region public override SensorHostInfo SensorHost
        public override SensorHostInfo SensorHost
        {
            get { return base.SensorHost; }
            set
            {
                if (value == null)
                    return;
                if (SensorHost != null)
                {
                    var _reorient = SensorHost.GravityFace != value.GravityFace;
                    var _headingChange = SensorHost.Heading != value.Heading;
                    var _inclineChange = SensorHost.Incline != value.Incline;
                    var _posChange = DoesPositionChange(value);

                    base.SensorHost = value;

                    if (_headingChange || _inclineChange || _reorient)
                        SetLook(true);
                    if (_posChange)
                        SetPosition(true);
                }
                else
                {
                    base.SensorHost = value;
                    SetLook(false);
                    SetPosition(false);
                }
            }
        }
        #endregion

        protected virtual bool DoesPositionChange(SensorHostInfo value)
        {
            return (SensorHost.Z != value.Z) || (SensorHost.Y != value.Y) || (SensorHost.X != value.X)
                || (SensorHost.Offset.Z != value.Offset.Z) || (SensorHost.Offset.Y != value.Offset.Y) || (SensorHost.Offset.X != value.Offset.X);
        }
    }
}
