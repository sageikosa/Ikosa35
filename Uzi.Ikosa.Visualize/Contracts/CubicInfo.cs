using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class CubicInfo : CellInfo, IGeometricRegion
    {
        public CubicInfo()
        {
        }

        public void SetCubicInfo(IGeometricRegion cubic)
        {
            Z = cubic.LowerZ;
            Y = cubic.LowerY;
            X = cubic.LowerX;
            ZTop = cubic.UpperZ;
            YTop = cubic.UpperY;
            XTop = cubic.UpperX;
        }

        [DataMember]
        public int ZTop { get; set; }
        [DataMember]
        public int YTop { get; set; }
        [DataMember]
        public int XTop { get; set; }

        public bool ContainsCell(int z, int y, int x)
            => (z >= Z) && (y >= Y) && (x >= X)
                && (z <= ZTop) && (y <= YTop) && (x <= XTop);

        public bool ContainsCell(ICellLocation location)
            => ContainsCell(location.Z, location.Y, location.X);

        /// <summary>True if the cubic represents a single cell</summary>
        public bool IsCell
            => ZHeight == 1 && YLength == 1 && XLength == 1;

        public long ZHeight => ZTop - Z + 1;
        public long YLength => YTop - Y + 1;
        public long XLength => XTop - X + 1;

        #region IGeometricSize Members

        public double ZExtent => ZHeight;
        public double YExtent => YLength;
        public double XExtent => XLength;

        #endregion

        #region IGeometricRegion Members

        public IEnumerable<ICellLocation> AllCellLocations()
        {
            for (int _z = Z; _z <= UpperZ; _z++)
            {
                for (int _y = Y; _y <= UpperY; _y++)
                {
                    for (int _x = X; _x <= UpperX; _x++)
                    {
                        yield return new CellPosition { Z = _z, Y = _y, X = _x };
                    }
                }
            }

            yield break;
        }

        public int LowerZ => Z;
        public int LowerY => Y;
        public int LowerX => X;

        public int UpperZ => ZTop;
        public int UpperY => YTop;
        public int UpperX => XTop;

        public Point3D GetPoint3D()
            => new Point3D
            {
                Z = Z * 5d + ZHeight * 2.5d,
                Y = Y * 5d + YLength * 2.5d,
                X = X * 5d + XLength * 2.5d
            };

        public IGeometricRegion Move(ICellLocation offset)
            => new CubicInfo
            {
                Z = Z + offset.Z,
                Y = Y + offset.Y,
                X = X + offset.X,
                ZTop = ZTop + offset.Z,
                YTop = YTop + offset.Y,
                XTop = XTop + offset.X
            };

        public bool IsCellAtSurface(ICellLocation location, AnchorFace surfaceFace)
        {
            switch (surfaceFace)
            {
                case AnchorFace.ZLow: return location.Z == LowerZ;
                case AnchorFace.YLow: return location.Y == LowerY;
                case AnchorFace.XLow: return location.X == LowerX;
                case AnchorFace.ZHigh: return location.Z == UpperZ;
                case AnchorFace.YHigh: return location.Y == UpperY;
                case AnchorFace.XHigh: return location.X == UpperX;
                default: return false;
            }
        }

        #endregion
    }
}
