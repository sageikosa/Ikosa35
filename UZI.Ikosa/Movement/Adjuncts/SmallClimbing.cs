using System;
using Uzi.Core;
using Uzi.Visualize;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class SmallClimbing : Adjunct
    {
        public SmallClimbing()
            : base(typeof(SmallClimbing))
        {
        }

        #region private data
        private CellSnap _Snap;
        #endregion

        public CellSnap CellSnap { get { return _Snap; } set { _Snap = value; } }

        public override object Clone()
        {
            return new SmallClimbing();
        }
    }
}
