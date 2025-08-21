using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    public class ResourceModel3DResolver : IResolveModel3D
    {
        public ResourceModel3DResolver(IEnumerable<IResolveModel3D> resolvers)
        {
            if (resolvers != null)
            {
                _Resolvers = resolvers.ToList();
            }
            else
            {
                _Resolvers = [];
            }
        }

        private List<IResolveModel3D> _Resolvers;
        public IEnumerable<IResolveModel3D> Resolvers { get { return _Resolvers.Select(_r => _r); } }

        #region IResolveModel3D Members

        public Model3D GetPrivateModel3D(object key)
        {
            foreach (var _resource in _Resolvers)
            {
                var _mdl = _resource.GetPrivateModel3D(key);
                if (_mdl != null)
                {
                    return _mdl;
                }
            }
            return null;
        }

        public bool CanResolveModel3D(object key)
        {
            return _Resolvers.Any(_r => _r.CanResolveModel3D(key));
        }

        public IResolveModel3D IResolveModel3DParent { get { return RootModel3DResolver.Root; } }

        public IEnumerable<Model3DPartListItem> ResolvableModels
            => _Resolvers
            .SelectMany(_r => _r.ResolvableModels)
            .OrderBy(_m => _m.Model3DPart.Name);

        #endregion
    }
}