using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Proxy.ViewModel;
using Uzi.Ikosa.Proxy.VisualizationSvc;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Client.UI
{
    public class ThirdPersonViewPoint : PerspectiveViewPoint
    {
        public ThirdPersonViewPoint()
            : base()
        {
            _Camera.FieldOfView = 115;
            _TPState = new ThirdPersonViewPointState();
        }

        #region data
        private ThirdPersonViewPointState _TPState;
        #endregion

        #region protected override bool DoesPositionChange(SensorHostInfo value)
        protected override bool DoesPositionChange(SensorHostInfo value)
        {
            return (value.ThirdCameraPoint.X != SensorHost.ThirdCameraPoint.X)
                || (value.ThirdCameraPoint.Y != SensorHost.ThirdCameraPoint.Y)
                || (value.ThirdCameraPoint.Z != SensorHost.ThirdCameraPoint.Z);
        }
        #endregion

        #region protected override Point3D Position
        protected override Point3D Position
        {
            get
            {
                // NOTE: AimPoint co-ordinates are not offsets
                var _base = base.Position;
                var _offset = (new Point3D(SensorHost.ThirdCameraPoint.X, SensorHost.ThirdCameraPoint.Y, SensorHost.ThirdCameraPoint.Z) - _base) * 0.90d;
                return _base + _offset;
            }
        }
        #endregion

        public override bool IsSelfVisible => true;

        public override uint ViewPointState
        {
            get => base.ViewPointState;
            set
            {
                var _old = _TPState;
                base.ViewPointState = value;
                _TPState = new ThirdPersonViewPointState(value);
                if (_old.Value != _TPState.Value)
                {
                    _Camera.FieldOfView = _TPState.FieldOfView;
                }
            }
        }

        public double FieldOfView => _TPState.FieldOfView;
    }
}
