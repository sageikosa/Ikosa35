using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    public interface ICacheMeshes
    {
        bool HasKey(int key);
        MeshGeometry3D GetMesh(int key);
        void SetMesh(int key, MeshGeometry3D mesh);
        void FlushCache();
    }
}
