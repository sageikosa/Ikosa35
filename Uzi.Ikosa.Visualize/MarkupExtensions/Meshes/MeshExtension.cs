using System;
using System.Windows.Markup;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    public abstract class MeshExtension : MarkupExtension
    {
        protected abstract MeshGeometry3D GenerateMesh();

        public int CacheKey { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var _cache = MeshCache.Current;
            if (_cache == null)
            {
                // no cache; generate directly
                return GenerateMesh();
            }
            else
            {
                if (_cache.HasKey(CacheKey))
                {
                    // found in cache; use it
                    return _cache.GetMesh(CacheKey);
                }
                else
                {
                    // cache exists, but key not found; seed value
                    var _mesh = GenerateMesh();
                    _cache.SetMesh(CacheKey, _mesh);
                    return _mesh;
                }
            }
        }
    }
}
