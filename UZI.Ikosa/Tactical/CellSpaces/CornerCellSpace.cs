using System;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class CornerCellSpace : WedgeCellSpace
    {
        #region Construction
        public CornerCellSpace(CellMaterial minusMaterial, TileSet minusTiles, CellMaterial plusMaterial, TileSet plusTiles,
            double primaryOffset, double secondardyOffset)
            :base(minusMaterial, minusTiles, plusMaterial, plusTiles, primaryOffset, secondardyOffset, true)
        {
        }
        #endregion

        public override string GetDescription(uint param)
            => $@"Cor:{Name} ({CellMaterialName};{TilingName}),({PlusMaterialName};{PlusTilingName}) [{Offset1};{Offset2}]";
    }
}
