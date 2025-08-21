using Newtonsoft.Json;
using System;
using System.Windows.Media.Media3D;
using Uzi.Visualize;

namespace Uzi.Core
{
    [Serializable]
    /// <summary>
    /// Represents the intersection of cell bounaries.  Useful for expressing coordinates that must be on a vertex.
    /// </summary>
    public class Intersection : ICellLocation, IGeometryAnchorSupplier, IEquatable<Intersection>
    {
        #region construction
        public Intersection(int z, int y, int x)
        {
            _Z = z;
            _Y = y;
            _X = x;
        }

        public Intersection(ICellLocation cellLocation)
        {
            _Z = cellLocation.Z;
            _Y = cellLocation.Y;
            _X = cellLocation.X;
        }

        public Intersection(ICellLocation cellLocation, int zOffset, int yOffset, int xOffset)
        {
            _Z = cellLocation.Z + zOffset;
            _Y = cellLocation.Y + yOffset;
            _X = cellLocation.X + xOffset;
        }

        /// <summary>Creates a cell intersection closest to the point</summary>
        public Intersection(Point3D point)
        {
            _Z = (int)Math.Round(point.Z / 5);
            _Y = (int)Math.Round(point.Y / 5);
            _X = (int)Math.Round(point.X / 5);
        }
        #endregion

        #region private data
        private int _Z;
        private int _Y;
        private int _X;
        #endregion

        public int Z => _Z;
        public int Y => _Y;
        public int X => _X;

        public CellPosition ToCellPosition() => new CellPosition(this);

        // IGeometryAnchorSupplier Members
        public ICellLocation Location
            => this;

        public LocationAimMode LocationAimMode 
            => LocationAimMode.Intersection;

        #region IControlChange<ICellLocation> Members

        public void AddChangeMonitor(Uzi.Core.IMonitorChange<ICellLocation> monitor)
        {
            // this doesn't change
        }

        public void RemoveChangeMonitor(Uzi.Core.IMonitorChange<ICellLocation> monitor)
        {
            // this doesn't change
        }

        #endregion

        #region INotifyPropertyChanged Members
        [field:NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        #endregion

        public override bool Equals(object obj)
        {
            var _i = obj as Intersection;
            if (_i != null)
            {
                return Equals(_i);
            }

            return false;
        }

        public static bool operator ==(Intersection i1, Intersection i2)
        {
            return i1.Equals(i2);
        }

        public static bool operator !=(Intersection i1, Intersection i2)
        {
            return !i1.Equals(i2);
        }

        public override int GetHashCode()
        {
            return this.Point3D().GetHashCode();
        }

        #region IEquatable<Intersection> Members

        public bool Equals(Intersection other)
        {
            return (Z == other.Z) && (Y == other.Y) && (X == other.X);
        }

        #endregion
    }
}
