using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class MovementLocatorTarget
    {
        #region data
        private Locator _Locator;
        private Vector3D _Offset;
        private IGeometricRegion _TargetRegion;
        private AnchorFace _BaseFace;
        #endregion

        public Locator Locator
        {
            get { return _Locator; }
            set { _Locator = value; }
        }

        /// <summary>OUTPUT</summary>
        public Vector3D Offset
        {
            get { return _Offset; }
            set { _Offset = value; }
        }

        /// <summary>OUTPUT</summary>
        public IGeometricRegion TargetRegion
        {
            get { return _TargetRegion; }
            set { _TargetRegion = value; }
        }

        public AnchorFace BaseFace
        {
            get { return _BaseFace; }
            set { _BaseFace = value; }
        }
    }
}
