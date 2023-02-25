using System.Windows.Media.Media3D;
using Uzi.Visualize;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Tactical
{
    public interface IParamCellSpace : IDrawCellSpace, ICellSpaceTacticalMovement
    {
        string Name { get; }
        string GetDescription(uint param);
        string GetParamText(uint param);

        LocalMap Map { get; }
        uint Index { get; }
        bool IsTemplate { get; }

        /// <summary>Represents the shareable cell space (especially if this instance has unique properties</summary>
        CellSpace Template { get; }

        /// <summary>Used when the CellSpace is part of a CompositeCellSpace</summary>
        CellSpace Parent { get; }

        bool BlocksDetect(uint param, int z, int y, int x, Point3D entryPt, Point3D exitPt);

        bool BlocksPath(uint param, int z, int y, int x, Point3D pt1, Point3D pt2);

        uint FlipAxis(uint paramsIn, Axis flipAxis);
        uint SwapAxis(uint paramsIn, Axis axis1, Axis axis2);

        CellSpaceInfo ToCellSpaceInfo();

        bool SuppliesBreathableAir(uint param);

        bool SuppliesBreathableWater(uint param);

        /// <summary>maximum structure points for a given parameter</summary>
        int MaxStructurePoints(uint param);
    }
}
