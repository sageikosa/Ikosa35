using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class CellEdge : ICellEdge
    {
        public CellEdge()
        {
        }

        #region data
        private CellMaterial _Material;
        private TileSet _Tiling;
        private double _Width;
        #endregion

        public string EdgeMaterial
            => Material?.Name;

        public string EdgeTiling
            => Tiling?.Name;

        public CellMaterial Material
        {
            get { return _Material; }
            set { _Material = value; }
        }

        public TileSet Tiling
        {
            get { return _Tiling; }
            set { _Tiling = value; }
        }

        public double Width
        {
            get { return _Width; }
            set { _Width = value; }
        }

        public BuildableMeshKey GetBuildableMeshKey(AnchorFace face, VisualEffect effect)
            => new BuildableMeshKey
            {
                Effect = effect,
                BrushKey = (Tiling != null) ? Tiling.BrushCollectionKey : string.Empty,
                BrushIndex = (Tiling != null) ? Tiling.GetAnchorFaceMaterialIndex(face) : 0
            };

        #region public BuildableMaterial GetOrthoFaceMaterial(Axis axis, bool isPlus, VisualEffect effect)
        public BuildableMaterial GetOrthoFaceMaterial(Axis axis, bool isPlus, VisualEffect effect)
        {
            if (Tiling == null)
            {
                return new BuildableMaterial { Material = null, IsAlpha = false };
            }

            switch (axis)
            {
                case Axis.Z:
                    if (isPlus)
                    {
                        return new BuildableMaterial { Material = Tiling.ZPlusMaterial(effect), IsAlpha = Tiling.GetAnchorFaceAlpha(AnchorFace.ZHigh) };
                    }

                    return new BuildableMaterial { Material = Tiling.ZMinusMaterial(effect), IsAlpha = Tiling.GetAnchorFaceAlpha(AnchorFace.ZLow) };
                case Axis.Y:
                    if (isPlus)
                    {
                        return new BuildableMaterial { Material = Tiling.YPlusMaterial(effect), IsAlpha = Tiling.GetAnchorFaceAlpha(AnchorFace.YHigh) };
                    }

                    return new BuildableMaterial { Material = Tiling.YMinusMaterial(effect), IsAlpha = Tiling.GetAnchorFaceAlpha(AnchorFace.YLow) };
                default:
                    if (isPlus)
                    {
                        return new BuildableMaterial { Material = Tiling.XPlusMaterial(effect), IsAlpha = Tiling.GetAnchorFaceAlpha(AnchorFace.XHigh) };
                    }

                    return new BuildableMaterial { Material = Tiling.XMinusMaterial(effect), IsAlpha = Tiling.GetAnchorFaceAlpha(AnchorFace.XLow) };
            }
        }
        #endregion

        public BuildableMaterial GetOtherFaceMaterial(int index, VisualEffect effect)
        {
            if (Tiling == null)
            {
                return new BuildableMaterial { Material = null, IsAlpha = false };
            }

            return new BuildableMaterial { Material = Tiling.WedgeMaterial(effect), IsAlpha = Tiling.GetWedgeAlpha() };
        }
    }
}
