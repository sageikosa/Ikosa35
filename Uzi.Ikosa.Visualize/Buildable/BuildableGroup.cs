using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    public struct BuildableGroup
    {
        public Model3DGroup Opaque;
        public Model3DGroup Alpha;
        public BuildableContext Context;
    }
}
