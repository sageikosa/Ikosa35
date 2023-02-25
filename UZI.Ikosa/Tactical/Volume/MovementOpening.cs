using System.Windows.Media.Media3D;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    public struct MovementOpening
    {
        public MovementOpening(AnchorFace opensTowards, double amount, double blockage)
        {
            _Face = opensTowards;
            _Amount = amount;
            _Blockage = blockage;
        }

        #region data
        private AnchorFace _Face;
        private double _Amount;
        private double _Blockage;
        #endregion

        /// <summary>Face that the cell opens towards</summary>
        public AnchorFace Face => _Face;

        /// <summary>Amount of space from face to blocking area</summary>
        public double Amount => _Amount;

        /// <summary>Coverage of the blocking area (0 to 1)</summary>
        public double Blockage => _Blockage;

        public Vector3D OffsetVector3D
            => Face.GetNormalVector() * (5 - Amount);
    }
}
