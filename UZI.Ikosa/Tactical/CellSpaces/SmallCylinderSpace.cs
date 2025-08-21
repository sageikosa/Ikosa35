using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Uzi.Visualize;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class SmallCylinderSpace : WedgeCellSpace
    {
        #region ctor()
        public SmallCylinderSpace(SolidCellMaterial solid, TileSet solidTiling, CellMaterial fillMaterial, TileSet fillTiling)
            : base(solid, solidTiling, fillMaterial, fillTiling, 2.5d, 2.5d, true)
        {
        }
        #endregion

        protected override bool OnCanCellMaterialChange(CellMaterial material)
            => material is SolidCellMaterial;

        protected override bool OnCanPlusMaterialChange(CellMaterial material)
            => !(material is SolidCellMaterial);

        protected override bool OnCanOffset1Change(double offset)
            => false;

        protected override bool OnCanOffset2Change(double offset)
            => false;

        public override bool? OccludesFace(uint param, AnchorFace outwardFace)
            => false;

        public override bool IsShadeable(uint param)
            => true; // always shadeable based on its structure

        public override void AddInnerStructures(uint param, BuildableGroup addToGroup, int z, int y, int x, VisualEffect effect)
        {
            CylinderSpaceFaces.AddSmallCylinder(param, this, addToGroup, effect);
        }

        public override void AddOuterSurface(uint param, BuildableGroup buildable, int z, int y, int x, AnchorFace face, VisualEffect effect, Vector3D bump, Cubic currentGroup)
        {
            if (!IsPlusInvisible)
            {
                CellSpaceFaces.AddPlusOuterSurface(param, this, buildable, z, y, x, face, effect, bump);
            }

            CylinderSpaceFaces.AddSmallCylinderCap(param, this, buildable, z, y, x, face, effect, bump);
        }

        public override string GetDescription(uint param)
            => $@"SmCyl:{Name} ({CellMaterialName};{TilingName}),({PlusMaterialName};{PlusTilingName})";

        public override string GetParamText(uint param)
        {
            var _param = new WedgeParams(param);
            return $@"Axis={_param.Axis}, Style={_param.Style}, Segs={_param.SegmentCount}, PF={_param.PrimarySnap}, SF={_param.SecondarySnap}";
        }

        public override CellSpaceInfo ToCellSpaceInfo()
            => new SmallCylinderSpaceInfo(this);
    }
}
