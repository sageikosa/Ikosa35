using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Uzi.Visualize.Packaging;
using Uzi.Packaging;

namespace Uzi.Visualize
{
    /// <summary>
    /// Provides BitmapImage to IkosaExtension when given a key
    /// </summary>
    public interface IResolveBitmapImage
    {
        BitmapSource GetImage(object key, VisualEffect effect);
        IGetImageByEffect GetIGetImageByEffect(object key);
        IResolveBitmapImage IResolveBitmapImageParent { get; }
        IEnumerable<BitmapImagePartListItem> ResolvableImages { get; }
    }

    public static class IResolveBitmapImageHelper
    {
        public static IResolveBitmapImage GetIResolveBitmapImage(this IBasePart self)
        {
            if (self.NameManager is IResolveBitmapImage)
            {
                return self.NameManager as IResolveBitmapImage;
            }
            else if (self.NameManager is CorePackagePartsFolder)
            {
                var _folder = self.NameManager as CorePackagePartsFolder;
                if (_folder.NameManager is VisualResources)
                {
                    return _folder.NameManager as VisualResources;
                }
                else
                {
                    return new ICorePartImageResolver(_folder);
                }
            }
            return null;
        }
    }
}
