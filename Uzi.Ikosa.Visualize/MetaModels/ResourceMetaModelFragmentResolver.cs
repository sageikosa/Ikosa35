using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    public class ResourceMetaModelFragmentResolver : IResolveFragment
    {
        public ResourceMetaModelFragmentResolver(IEnumerable<IResolveFragment> resolvers)
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

        private List<IResolveFragment> _Resolvers;
        public IEnumerable<IResolveFragment> Resolvers { get { return _Resolvers.Select(_r => _r); } }

        #region IResolveFragment Members

        public Model3D GetFragment(FragmentReference fragRef, MetaModelFragmentNode node)
        {
            foreach (var _resource in _Resolvers)
            {
                var _frag = _resource.GetFragment(fragRef, node);
                if (_frag != null)
                {
                    return _frag;
                }
            }
            return null;
        }

        public IResolveFragment IResolveFragmentParent { get { return null; } }

        public IEnumerable<MetaModelFragmentListItem> ResolvableFragments
        {
            get { return _Resolvers.SelectMany(_r => _r.ResolvableFragments); }
        }

        #endregion
    }
}
