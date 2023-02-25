using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Proxy.ViewModel;
using Uzi.Ikosa.Proxy.VisualizationSvc;
using Uzi.Visualize;
using Uzi.Visualize.Contracts.Tactical;
using Uzi.Visualize.Packaging;

namespace Uzi.Ikosa.Proxy
{
    public class MapIconResolver : IResolveIcon
    {
        public MapIconResolver(ProxyModel proxies)
        {
            _Proxies = proxies;
            _IconNames = MyProxy.Service.IconNames();
        }

        #region data
        private string[] _IconNames { get; set; }
        private ConcurrentDictionary<string, IconPart> _Icons
            = new ConcurrentDictionary<string, IconPart>();
        private ProxyModel _Proxies;
        #endregion

        public VisualizationServiceClient MyProxy
            => _Proxies.ViewProxy;

        // IResolveIcon
        private IconPart GetIconPart(string name)
            => _Icons.GetOrAdd(name,
                (key) => MyProxy.Service.GetIcon(key)?.ToIconPart());

        public System.Windows.Media.Visual GetIconVisual(string key, IIconReference iconRef)
        {
            var _key = key.ToString().ToLowerInvariant();
            if (_IconNames.Any(_i => string.Equals(_i, _key, StringComparison.OrdinalIgnoreCase)))
            {
                var _icon = GetIconPart(_key);
                return _icon.GetIconContent(iconRef);
            }
            return null;
        }
        public Material GetIconMaterial(string key, IIconReference iconRef, IconDetailLevel detailLevel)
        {
            var _key = key.ToString().ToLowerInvariant();
            if (_IconNames.Any(_i => string.Equals(_i, _key, StringComparison.OrdinalIgnoreCase)))
            {
                var _icon = GetIconPart(_key);
                return _icon.GetIconMaterial(detailLevel, iconRef);
            }
            return null;
        }

        public IResolveIcon IResolveIconParent
            => null;

        public IEnumerable<IconPartListItem> ResolvableIcons
            => Enumerable.Empty<IconPartListItem>();
    }
}
