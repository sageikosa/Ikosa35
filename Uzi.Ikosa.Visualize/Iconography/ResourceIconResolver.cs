using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Uzi.Visualize.Packaging;
using System.Windows.Media.Media3D;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Visualize
{
    public class ResourceIconResolver : IResolveIcon
    {
        public ResourceIconResolver(IEnumerable<IResolveIcon> resolvers)
        {
            _Resolvers = resolvers?.ToList() ?? new List<IResolveIcon>();
        }

        private readonly List<IResolveIcon> _Resolvers;
        public IEnumerable<IResolveIcon> Resolvers => _Resolvers.Select(_r => _r);

        #region IResolveIcon Members

        public Visual GetIconVisual(string key, IIconReference iconRef)
        {
            foreach (var _resource in _Resolvers)
            {
                var _ico = _resource.GetIconVisual(key, iconRef);
                if (_ico != null)
                    return _ico;
            }
            return null;
        }

        public Material GetIconMaterial(string key, IIconReference iconRef, IconDetailLevel detailLevel)
        {
            foreach (var _resource in _Resolvers)
            {
                var _ico = _resource.GetIconMaterial(key, iconRef, detailLevel);
                if (_ico != null)
                    return _ico;
            }
            return null;
        }

        public IResolveIcon IResolveIconParent => null;

        public IEnumerable<IconPartListItem> ResolvableIcons
            => _Resolvers.SelectMany(_r => _r.ResolvableIcons);

        #endregion
    }
}
