using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    public class ResourceBrushesResolver : IResolveMaterial
    {
        public ResourceBrushesResolver(IEnumerable<IResolveMaterial> resolvers)
        {
            if (resolvers != null)
                _Resolvers = resolvers.ToList();
            else
                _Resolvers = new List<IResolveMaterial>();
        }

        private List<IResolveMaterial> _Resolvers;
        public IEnumerable<IResolveMaterial> Resolvers { get { return _Resolvers.Select(_r => _r); } }

        #region IResolveMaterial Members

        public Material GetMaterial(object key, VisualEffect effect)
        {
            foreach (var _resource in _Resolvers)
            {
                var _material = _resource.GetMaterial(key, effect);
                if (_material != null)
                    return _material;
            }
            return null;
        }

        public IResolveMaterial IResolveMaterialParent { get { return null; } }

        public IEnumerable<BrushDefinitionListItem> ResolvableBrushes
        {
            get { return _Resolvers.SelectMany(_r => _r.ResolvableBrushes); }
        }

        #endregion
    }
}
