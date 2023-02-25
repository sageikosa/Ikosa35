using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uzi.Visualize
{
    public class MeshCache
    {
        [ThreadStatic]
        public static ICacheMeshes Current = null;
    }
}
