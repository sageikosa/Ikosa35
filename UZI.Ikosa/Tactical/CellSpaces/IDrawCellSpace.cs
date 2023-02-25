using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    public interface IDrawCellSpace
    {
        /// <summary>Cell is shadeable purely on its own structure</summary>
        bool IsShadeable(uint param);

        void AddOuterSurface(uint param, BuildableGroup buildable, int z, int y, int x, AnchorFace face, VisualEffect effect, Vector3D bump, Cubic currentGroup);

        /// <summary>Generates Model3DGroup for any part of the cell that is not flush to the surface</summary>
        void AddInnerStructures(uint param, BuildableGroup addToGroup, int z, int y, int x, VisualEffect effect);

        BuildableGroup GenerateModel(uint param, int z, int y, int x, Cubic currentGroup);

        /// <summary>Returns the best brush for a SensorHost</summary>
        Brush InnerBrush(uint param, VisualEffect effect);

        /// <summary>Item1 = collection key, Item 2 - brush key</summary>
        (string collectionKey, string brushKey) InnerBrushKeys(uint param, Point3D point);

        bool? NeighborOccludes(int z, int y, int x, AnchorFace neighborFace, IGeometricRegion currentGroup);

        bool? ShowCubicFace(uint param, AnchorFace outwardFace);

        bool ShowDirectionalFace(uint param, AnchorFace outwardFace);

        bool? OccludesFace(uint param, AnchorFace outwardFace);

    }
}
