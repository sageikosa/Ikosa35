using Uzi.Ikosa.Proxy.VisualizationSvc;
using System.Windows.Media.Media3D;
using Uzi.Visualize;

namespace Uzi.Ikosa.Proxy
{
    public struct CellStructureInfo
    {
        public CellSpaceViewModel CellSpace;
        public uint ParamData;

        #region public void AddOuterSurface(BuildableGroup group, int z, int y, int x, AnchorFace face, VisualEffect effect, Transform3D bump)
        public void AddOuterSurface(BuildableGroup group, int z, int y, int x, AnchorFace face, VisualEffect effect, Vector3D bump, IGeometricRegion currentRegion)
        {
            if (CellSpace != null)
            {
                // should show?
                var _show = CellSpace.ShowCubicFace(ParamData, face);
                if (_show ?? true)
                {
                    // yes or maybe show
                    var _occluded = CellSpace.NeighborOccludes(z, y, x, face, currentRegion);
                    if (!_show.HasValue)
                    {
                        // maybe show
                        if (_occluded.HasValue && !_occluded.Value)
                        {
                            // neighbor definitely not occluding, so show
                            CellSpace.AddOuterSurface(ParamData, group, z, y, x, face, effect, bump, currentRegion);
                        }
                    }
                    else if (!(_occluded ?? false))
                    {
                        // show, and neighbor is not strongly occluding
                        CellSpace.AddOuterSurface(ParamData, group, z, y, x, face, effect, bump, currentRegion);
                    }
                }
            }
        }
        #endregion

        public void AddExteriorSurface(BuildableGroup group, int z, int y, int x, AnchorFace face, VisualEffect effect, IGeometricRegion currentGroup)
        {
            // should show?
            if (CellSpace.ShowDirectionalFace(ParamData, face) && (CellSpace.NeighborOccludes(z, y, x, face, currentGroup) ?? true))
            {
                // show, and neighbor is not strongly occluding
                CellSpace.AddOuterSurface(ParamData, group, z, y, x, face, effect, new Vector3D(), currentGroup);
            }
        }

        /// <summary>Generates Model3DGroup for any part of the cell that is not flush to the surface</summary>
        public void AddInnerStructures(BuildableGroup group, int z, int y, int x, VisualEffect effect)
        {
            if (CellSpace != null)
                CellSpace.AddInnerStructures(ParamData, group, z, y, x, effect);
        }

        public bool? OccludesFace(AnchorFace outwardFace)
        {
            return CellSpace.OccludesFace(ParamData, outwardFace);
        }

        #region public bool OutwardVisible(int z, int y, int x)
        public bool OutwardVisible(int z, int y, int x)
        {
            if (CellSpace != null)
                return CellSpace.OutwardVisible(ParamData, z, y, x);
            return false;
        }
        #endregion

        #region public bool InwardVisible(ushort param, int z, int y, int x)
        public bool InwardVisible(ushort param, int z, int y, int x)
        {
            if (CellSpace != null)
                return CellSpace.InwardVisible(ParamData, z, y, x);
            return false;
        }
        #endregion

    }
}
