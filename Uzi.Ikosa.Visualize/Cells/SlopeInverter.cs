namespace Uzi.Visualize
{
    internal class SlopeInverter : IPlusCellSpace
    {
        internal SlopeInverter(IPlusCellSpace baseSlope)
        {
            _BaseSlope = baseSlope;
        }

        #region private data
        private IPlusCellSpace _BaseSlope;
        #endregion

        #region ISliverSpace Members

        public bool IsPlusGas => _BaseSlope.IsGas;
        public bool IsPlusLiquid => _BaseSlope.IsLiquid;
        public bool IsPlusInvisible => _BaseSlope.IsInvisible;
        public bool IsPlusSolid => _BaseSlope.IsSolid;

        public BuildableMaterial GetPlusOrthoFaceMaterial(Axis axis, bool isPlus, VisualEffect effect)
            => _BaseSlope.GetOrthoFaceMaterial(axis, isPlus, effect);

        public BuildableMaterial GetPlusOtherFaceMaterial(int index, VisualEffect effect)
            => _BaseSlope.GetOtherFaceMaterial(index, effect);

        public BuildableMeshKey GetPlusBuildableMeshKey(AnchorFace face, VisualEffect effect)
            => _BaseSlope.GetBuildableMeshKey(face, effect);

        #endregion

        #region ICellSpace Members

        public bool IsGas => _BaseSlope.IsPlusGas;
        public bool IsLiquid => _BaseSlope.IsPlusLiquid;
        public bool IsInvisible => _BaseSlope.IsPlusInvisible;

        public BuildableMaterial GetOrthoFaceMaterial(Axis axis, bool isPlus, VisualEffect effect)
            => _BaseSlope.GetPlusOrthoFaceMaterial(axis, isPlus, effect);

        public BuildableMaterial GetOtherFaceMaterial(int index, VisualEffect effect)
            => _BaseSlope.GetPlusOtherFaceMaterial(index, effect);

        public bool? OccludesFace(uint param, AnchorFace outwardFace)
            => _BaseSlope.OccludesFace(param, outwardFace);

        public bool? NeighborOccludes(int z, int y, int x, AnchorFace neighborFace, IGeometricRegion currentRegion)
            => _BaseSlope.NeighborOccludes(z, y, x, neighborFace, currentRegion);

        public bool? ShowCubicFace(uint param, AnchorFace outwardFace)
            => _BaseSlope.ShowCubicFace(param, outwardFace);

        public bool ShowDirectionalFace(uint param, AnchorFace outwardFace)
            => _BaseSlope.ShowDirectionalFace(param, outwardFace);

        public BuildableMeshKey GetBuildableMeshKey(AnchorFace face, VisualEffect effect)
            => _BaseSlope.GetPlusBuildableMeshKey(face, effect);

        public uint Index => _BaseSlope.Index;
        public string Name => _BaseSlope.Name;
        public string CellMaterialName => _BaseSlope.PlusMaterialName;
        public string TilingName => _BaseSlope.PlusTilingName;
        public bool IsSolid => _BaseSlope.IsPlusSolid;

        #endregion

        #region IPlusCellSpace Members

        public string PlusMaterialName => _BaseSlope.CellMaterialName;
        public string PlusTilingName => _BaseSlope.TilingName;

        #endregion
    }
}
