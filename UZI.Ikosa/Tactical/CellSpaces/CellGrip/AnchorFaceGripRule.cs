using System;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    /// <summary>
    /// Grip rule for (Uniform) CellSpaces
    /// </summary>
    [Serializable]
    public class AnchorFaceGripRule : GripRule
    {
        private AnchorFaceList _AppliesTo;

        /// <summary>cellSpace doesn't rotate, so grip rules are anchorFace-specific</summary>
        public AnchorFaceList AppliesTo { get { return _AppliesTo; } set { _AppliesTo = value; } }
    }
}
