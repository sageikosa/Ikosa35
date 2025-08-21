using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    public static class Transform3DExtend
    {
        /// <summary>
        /// Transforms an entire mesh of points and normals
        /// </summary>
        /// <param name="transform">transformation to use</param>
        /// <param name="mesh">source mesh to be cloned and transformed</param>
        /// <returns></returns>
        public static MeshGeometry3D Transform(this Transform3D transform, MeshGeometry3D mesh)
        {
            var _clone = mesh.Clone();
            for (var _px = 0; _px < _clone.Positions.Count; _px++)
            {
                _clone.Positions[_px] = transform.Transform(_clone.Positions[_px]);
            }

            for (var _nx = 0; _nx < _clone.Normals.Count; _nx++)
            {
                _clone.Normals[_nx] = transform.Transform(_clone.Normals[_nx]);
            }

            return _clone;
        }
    }
}
