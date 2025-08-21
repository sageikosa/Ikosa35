using System.Collections.Generic;

namespace Uzi.Visualize
{
    public class BuildableMeshKeyComparer : IEqualityComparer<BuildableMeshKey>
    {
        #region IEqualityComparer<BuildableMeshKey> Members

        public bool Equals(BuildableMeshKey x, BuildableMeshKey y)
        {
            return x.BrushIndex == y.BrushIndex
                && x.Effect == y.Effect
                && (x.BrushKey?.Equals(y.BrushKey) ?? ((x.BrushKey == null) && (y.BrushKey == null)));
        }

        public int GetHashCode(BuildableMeshKey obj)
        {
            return (obj.BrushKey?.GetHashCode() ?? 0) + (37 * obj.BrushIndex) + (1371 * (int)obj.Effect);
        }

        #endregion
    }
}
