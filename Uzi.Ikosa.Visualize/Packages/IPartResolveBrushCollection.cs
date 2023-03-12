using System;
using System.Collections.Generic;
using System.Linq;
using Ikosa.Packaging;

namespace Uzi.Visualize.Packages
{
    public interface IPartResolveBrushCollection
    {
        BrushCollection GetBrushCollection(object key);
        IPartResolveBrushCollection IPartResolveBrushCollectionParent { get; }
        IEnumerable<BrushCollectionPartListItem> ResolvableBrushCollections { get; }
    }

    public static class IPartResolveBrushCollectionHelper
    {
        public static IPartResolveBrushCollection GetIPartResolveBrushCollection(this IStorablePart self)
        {
            if (self.PartNameManager is IPartResolveBrushCollection _manager)
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
