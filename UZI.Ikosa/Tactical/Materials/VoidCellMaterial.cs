using System;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class VoidCellMaterial : CellMaterial
    {
        public VoidCellMaterial(LocalMap localMap)
            : base(@"Void", localMap)
        {
        }

        public override bool BlocksEffect { get { return false; } }
    }
}