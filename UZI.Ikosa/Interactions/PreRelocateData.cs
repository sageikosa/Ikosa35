using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Uzi.Core;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class PreRelocateData : InteractData
    {
        public PreRelocateData(CoreActor actor, Locator locator, IGeometricRegion region,
            MovementBase activeMovement, Vector3D offset, AnchorFaceList crossings, AnchorFace baseFace) 
            : base(actor)
        {
            _Locator = locator;
            _Region = region;
            _Movement = activeMovement;
            _Offset = offset;
            _Crossings = crossings;
            _BaseFace = baseFace;
        }

        #region Data
        private Locator _Locator;
        private IGeometricRegion _Region;
        private MovementBase _Movement;
        private Vector3D _Offset;
        private AnchorFaceList _Crossings;
        private AnchorFace _BaseFace;
        #endregion

        public AnchorFace BaseFace => _BaseFace;
        public AnchorFaceList Direction => _Crossings;
        public Locator Locator => _Locator;
        public IGeometricRegion Region => _Region;
        public MovementBase Movement => _Movement;
        public Vector3D OffsetVector => _Offset;
    }
}
