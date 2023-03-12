using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Ikosa.Packaging;

namespace Uzi.Visualize.Packages
{
    public interface IPartResolveMaterial
    {
        Material GetMaterial(object key, VisualEffect effect);
        IPartResolveMaterial IPartResolveMaterialParent { get; }
        IEnumerable<BrushDefinitionListItem> ResolvableBrushes { get; }
    }

    public static class IPartResolveMaterialHelper
    {
        public static IPartResolveMaterial GetIPartResolveMaterial(this IStorablePart self)
        {
            if (self.PartNameManager is IPartResolveMaterial _manager)
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
