using System.Collections.Generic;
using System.Linq;
using Uzi.Packaging;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Visualize.Packaging
{
    /// <summary>Resolves icons related under an ICorePart</summary>
    public class ICorePartIconResolver : IResolveIcon
    {
        /// <summary>Resolves icons related under an ICorePart</summary>
        public ICorePartIconResolver(ICorePart part)
        {
            _Part = part;
        }

        private readonly ICorePart _Part;

        public ICorePart Part => _Part;

        #region IResolveIcon Members

        public Visual GetIconVisual(string key, IIconReference iconRef)
            => (_Part?.FindBasePart(key) as IconPart)?.GetIconContent(iconRef);

        public Material GetIconMaterial(string key, IIconReference iconRef, IconDetailLevel detailLevel)
            => (_Part?.FindBasePart(key) as IconPart)?.GetIconMaterial(detailLevel, iconRef);

        public IResolveIcon IResolveIconParent => null;

        public IEnumerable<IconPartListItem> ResolvableIcons
            => _Part?.Relationships.OfType<IconPart>()
            .Select(_ip => new IconPartListItem
            {
                IconPart = _ip,
                IsLocal = true
            })
            ?? new IconPartListItem[] { };

        #endregion
    }
}
