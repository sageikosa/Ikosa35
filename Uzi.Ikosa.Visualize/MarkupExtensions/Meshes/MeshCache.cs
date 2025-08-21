using System;

namespace Uzi.Visualize
{
    public class MeshCache
    {
        [ThreadStatic]
        public static ICacheMeshes Current = null;
    }
}
