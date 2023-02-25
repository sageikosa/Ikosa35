using System;

namespace Uzi.Ikosa.Tactical
{
    /// <summary>
    /// Used to mark recessive cells materials between two local-maps where they overlap
    /// </summary>
    [Serializable]
    public class UndefinedCellMaterial : CellMaterial
    {
        public UndefinedCellMaterial(LocalMap localMap)
            : base(@"Undefined", localMap)
        {
        }

        public override bool BlocksEffect { get { return false; } }
    }
}
