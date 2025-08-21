using System;
using System.Runtime.Serialization;
using System.Windows.Media.Media3D;
using Uzi.Visualize.Contracts;

namespace Uzi.Visualize
{
    [Flags]
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public enum LocationAimMode
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Intersection = 1,
        [EnumMember]
        Cell = 2,
        [EnumMember]
        Any = 3
    }

    public static class LocationAimModeHelper
    {
        public static Point3D GetPoint3D(this LocationAimMode self, ICellLocation location)
            => ((self == LocationAimMode.Cell)
            ? location?.GetPoint()
            : location?.Point3D()) ?? new Point3D();
    }
}
