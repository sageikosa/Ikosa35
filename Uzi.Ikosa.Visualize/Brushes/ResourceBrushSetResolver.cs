using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uzi.Visualize
{
    public class ResourceBrushSetResolver : IResolveBrushCollection
    {
        public ResourceBrushSetResolver(IEnumerable<IResolveBrushCollection> resolvers)
        {
            if (resolvers != null)
                _Resolvers = resolvers.ToList();
            else
                _Resolvers = new List<IResolveBrushCollection>();
        }

        private readonly List<IResolveBrushCollection> _Resolvers;
        public IEnumerable<IResolveBrushCollection> Resolvers => _Resolvers.Select(_r => _r);

        #region IResolveBrushCollection Members

        public BrushCollection GetBrushCollection(object key)
        {
            foreach (var _resource in _Resolvers)
            {
                var _set = _resource.GetBrushCollection(key);
                if (_set != null)
                    return _set;
            }
            return null;
        }

        public IResolveBrushCollection IResolveBrushCollectionParent => null;

        public IEnumerable<BrushCollectionListItem> ResolvableBrushCollections
            => _Resolvers.SelectMany(_r => _r.ResolvableBrushCollections);

        #endregion
    }
}
