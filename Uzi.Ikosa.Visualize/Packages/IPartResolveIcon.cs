using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Ikosa.Packaging;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Visualize.Packages
{
    public interface IPartResolveIcon
    {
        Visual GetIconVisual(string key, IIconReference iconRef);
        Material GetIconMaterial(string key, IIconReference iconRef, IconDetailLevel detailLevel);
        IPartResolveIcon IPartResolveIconParent { get; }
        IEnumerable<IconPartListItem> ResolvableIcons { get; }
    }

    public static class IPartResolveIconHelper
    {
        public static IPartResolveIcon GetIPartResolveIcon(this IStorablePart self)
        {
            if (self.PartNameManager is IPartResolveIcon _manager)
            {
                return _manager;
            }
            else if (self.PartNameManager is ArchivePartsFolder _folder)
            {
                if (_folder.PartNameManager is VisualResourcePart _visPart)
                {
                    return _visPart;
                }
            }
            return null;
        }
    }
}
