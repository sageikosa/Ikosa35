namespace Uzi.Ikosa.Proxy.VisualizationSvc
{
    public partial class _CubicInfo //: IGeometricRegion
    {
        //public bool ContainsCell(ICellLocation location)
        //{
        //    return this.IsCellInBounds(location);
        //}

        //public long ZHeight { get { return ZTop - Z + 1; } }
        //public long YLength { get { return YTop - Y + 1; } }
        //public long XLength { get { return XTop - X + 1; } }

        //#region IGeometricSize Members

        //public double ZExtent { get { return (double)ZHeight; } }
        //public double YExtent { get { return (double)YLength; } }
        //public double XExtent { get { return (double)XLength; } }

        //#endregion

        //#region IGeometricRegion Members

        //public IEnumerable<ICellLocation> AllCellLocations()
        //{
        //    for (int _z = Z; _z <= UpperZ; _z++)
        //        for (int _y = Y; _y <= UpperY; _y++)
        //            for (int _x = X; _x <= UpperX; _x++)
        //                yield return new CellPosition { Z = _z, Y = _y, X = _x };
        //    yield break;
        //}

        //public int LowerZ { get { return Z; } }
        //public int LowerY { get { return Y; } }
        //public int LowerX { get { return X; } }

        //public int UpperZ { get { return ZTop; } }
        //public int UpperY { get { return YTop; } }
        //public int UpperX { get { return XTop; } }

        //public Point3D GetPoint()
        //{
        //    return new Point3D
        //    {
        //        Z = Z * 5d + ZHeight * 2.5d,
        //        Y = Y * 5d + YLength * 2.5d,
        //        X = X * 5d + XLength * 2.5d
        //    };
        //}

        //public IGeometricRegion Move(ICellLocation offset)
        //{
        //    return new CubicInfo
        //    {
        //        Z = Z + offset.Z,
        //        Y = Y + offset.Y,
        //        X = X + offset.X,
        //        ZTop = ZTop + offset.Z,
        //        YTop = YTop + offset.Y,
        //        XTop = XTop + offset.X
        //    };
        //}

        //public bool IsCellAtSurface(ICellLocation location, AnchorFace surfaceFace)
        //{
        //    if (surfaceFace == AnchorFace.ZLow) return location.Z == LowerZ;
        //    if (surfaceFace == AnchorFace.YLow) return location.Y == LowerY;
        //    if (surfaceFace == AnchorFace.XLow) return location.X == LowerX;
        //    if (surfaceFace == AnchorFace.ZHigh) return location.Z == UpperZ;
        //    if (surfaceFace == AnchorFace.YHigh) return location.Y == UpperY;
        //    if (surfaceFace == AnchorFace.XHigh) return location.X == UpperX;
        //    return false;
        //}

        //#endregion
    }
}
