using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    public interface ICellLocation 
    {
        int Z { get; }
        int Y { get; }
        int X { get; }
        CellPosition ToCellPosition();
    }

    public class CellLocationEquality : IEqualityComparer<ICellLocation>
    {
        #region IEqualityComparer<CellLocation> Members

        public bool Equals(ICellLocation x, ICellLocation y)
        {
            return (x.Z == y.Z) && (x.Y == y.Y) && (x.X == y.X);
        }

        public int GetHashCode(ICellLocation obj)
        {
            return obj.Point3D().GetHashCode();
        }

        #endregion
    }

    public static class ICellLocationHelper
    {
        public static string ToDebugString(this ICellLocation self)
            => $@"ZYX({self?.Z ?? double.NaN}, {self?.Y ?? double.NaN}, {self?.X ?? double.NaN})";

        public static bool IsCellEqual(this ICellLocation self, ICellLocation other)
            => (self.Z == other.Z) && (self.Y == other.Y) && (self.X == other.X);
    }
}
