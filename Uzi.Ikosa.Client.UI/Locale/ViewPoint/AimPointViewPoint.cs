using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Proxy.VisualizationSvc;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Client.UI
{
    public class AimPointViewPoint : PerspectiveViewPoint
    {
        public AimPointViewPoint()
            : base()
        {
            _Camera.FieldOfView = 75;
        }

        #region protected override bool DoesPositionChange(SensorHostInfo value)
        protected override bool DoesPositionChange(SensorHostInfo value)
        {
            return (value.AimPoint.X != SensorHost.AimPoint.X)
                || (value.AimPoint.Y != SensorHost.AimPoint.Y)
                || (value.AimPoint.Z != SensorHost.AimPoint.Z);
        }
        #endregion

        #region protected override Point3D Position
        protected override Point3D Position
        {
            get
            {
                // NOTE: AimPoint co-ordinates are not offsets
                var _base = base.Position;
                var _offset = (SensorHost.AimPoint3D - _base) * 0.95d;
                return _base + _offset;
            }
        }
        #endregion

        public override bool IsSelfVisible { get { return true; } }
    }
}