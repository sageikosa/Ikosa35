using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Visualize
{
    internal class SliverInverter : IPlusCellSpace
    {
        public SliverInverter(IPlusCellSpace baseSpace)
        {
            _BaseSpace = baseSpace;
        }

        private IPlusCellSpace _BaseSpace;

        #region IPlusCellSpace Members

        public string PlusMaterialName => _BaseSpace.CellMaterialName;
        public string PlusTilingName => _BaseSpace.TilingName;
        public bool IsPlusGas => _BaseSpace.IsGas;
        public bool IsPlusLiquid => _BaseSpace.IsLiquid;
        public bool IsPlusInvisible => _BaseSpace.IsInvisible;
        public bool IsPlusSolid => _BaseSpace.IsSolid;

        public BuildableMaterial GetPlusOrthoFaceMaterial(Axis axis, bool isPlus, VisualEffect effect)
            => _BaseSpace.GetOrthoFaceMaterial(axis, isPlus, effect);

        public BuildableMaterial GetPlusOtherFaceMaterial(int index, VisualEffect effect)
            => _BaseSpace.GetOtherFaceMaterial(index, effect);

        public BuildableMeshKey GetPlusBuildableMeshKey(AnchorFace face, VisualEffect effect)
            => _BaseSpace.GetBuildableMeshKey(face, effect);

        #endregion

        #region ICellSpace Members

        public uint Index => _BaseSpace.Index;
        public string Name => _BaseSpace.Name;
        public string CellMaterialName => _BaseSpace.PlusMaterialName;
        public string TilingName => _BaseSpace.PlusTilingName;
        public bool IsSolid => _BaseSpace.IsPlusSolid;
        public bool IsGas => _BaseSpace.IsPlusGas;
        public bool IsLiquid => _BaseSpace.IsPlusLiquid;
        public bool IsInvisible => _BaseSpace.IsPlusInvisible;

        public BuildableMaterial GetOrthoFaceMaterial(Axis axis, bool isPlus, VisualEffect effect)
            => _BaseSpace.GetPlusOrthoFaceMaterial(axis, isPlus, effect);

        public BuildableMaterial GetOtherFaceMaterial(int index, VisualEffect effect)
            => _BaseSpace.GetPlusOtherFaceMaterial(index, effect);

        public bool? OccludesFace(uint param, AnchorFace outwardFace)
            => _BaseSpace.OccludesFace(param, outwardFace);

        public bool? NeighborOccludes(int z, int y, int x, AnchorFace neighborFace, IGeometricRegion region)
            => _BaseSpace.NeighborOccludes(z, y, x, neighborFace, region);

        public bool? ShowCubicFace(uint param, AnchorFace outwardFace)
            => _BaseSpace.ShowCubicFace(param, outwardFace);

        public BuildableMeshKey GetBuildableMeshKey(AnchorFace face, VisualEffect effect)
            => _BaseSpace.GetPlusBuildableMeshKey(face, effect);

        public bool ShowDirectionalFace(uint param, AnchorFace outwardFace)
            => _BaseSpace.ShowDirectionalFace(param, outwardFace);

        #endregion
    }
}