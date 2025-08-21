using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    [Serializable]
    public enum Axis : byte
    {
        /// <summary>X-axis (0)</summary>
        X = 0,
        /// <summary>Y-axis (1)</summary>
        Y = 1,
        /// <summary>Z-axis (2)</summary>
        Z = 2
    }

    public static class AxisHelper
    {
        public static AnchorFace GetLowFace(this Axis self)
        {
            switch (self)
            {
                case Axis.Z:
                    return AnchorFace.ZLow;
                case Axis.Y:
                    return AnchorFace.YLow;
                case Axis.X:
                default:
                    return AnchorFace.XLow;
            }
        }

        public static AnchorFace GetHighFace(this Axis self)
        {
            switch (self)
            {
                case Axis.Z:
                    return AnchorFace.ZHigh;
                case Axis.Y:
                    return AnchorFace.YHigh;
                case Axis.X:
                default:
                    return AnchorFace.XHigh;
            }
        }

        public static Vector3D AxisVector(this Axis self)
        {
            switch (self)
            {
                case Axis.Z:
                    return new Vector3D(0, 0, 1);
                case Axis.Y:
                    return new Vector3D(0, 1, 0);
                case Axis.X:
                default:
                    return new Vector3D(1, 0, 0);
            }
        }

        public static IEnumerable<Axis> GetAll()
        {
            yield return Axis.Z;
            yield return Axis.Y;
            yield return Axis.X;
            yield break;
        }
    }
}
