using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    public static class Vector3DHelper
    {
        /// <summary>Calculates a signed angle between two vectors when using a reference axis</summary>
        public static double AxisAngleBetween(this Vector3D axis, Vector3D start, Vector3D end)
        {
            // cross [start] to [axis] to get [thumb] vector (should be "planar" with [start] and [end])
            var _thumb = Vector3D.CrossProduct(start, axis);

            // if anglebetween([thumb], [end]) > 90, then [end] is on the other side of the [axis]...[start] plane from [thumb]
            // ... being on same side of [thumb] is negative
            var _sign = (Vector3D.AngleBetween(_thumb, end) > 90) ? 1 : -1;

            // so whatever absolute angle is calculated, multiply by sign
            return Vector3D.AngleBetween(start, end) * _sign;
        }
    }
}
