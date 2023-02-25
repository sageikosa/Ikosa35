using System;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class SolidCellMaterial : CellMaterial
    {
        public SolidCellMaterial(string name, Material solidMaterial, LocalMap localMap)
            : base(name, localMap)
        {
            _SolidMaterial = solidMaterial;
            BaseGrip = 25;
            DangleGrip = 25;
            LedgeGrip = 10;
            Balance = 0;
        }

        private bool _CanBurrow = false;
        private Material _SolidMaterial = null;

        public Material SolidMaterial { get { return _SolidMaterial; } }

        public bool CanBurrow { get { return _CanBurrow; } set { _CanBurrow = value; } }

        public override bool BlocksEffect { get { return true; } }
    }
}