using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using Ikosa.Packaging;

namespace Uzi.Visualize.Packages
{
    /// <summary>
    /// Provides BitmapImage to IkosaExtension when given a key
    /// </summary>
    public interface IPartResolveImage
    {
        BitmapSource GetImage(object key, VisualEffect effect);
        IGetImageByEffect GetIGetImageByEffect(object key);
        IPartResolveImage IPartResolveImageParent { get; }
        IEnumerable<BitmapImagePartListItem> ResolvableImages { get; }
    }

    public static class IPartResolveImageHelper
    {
        public static IPartResolveImage GetIPartResolveBitmapImage(this IStorablePart self)
        {
            if (self.PartNameManager is IPartResolveImage _manager)
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
