using System.Collections.Generic;
using Uzi.Visualize.Packaging;
using Uzi.Packaging;

namespace Uzi.Visualize
{
    /// <summary>Provides BrushCollections</summary>
    public interface IResolveBrushCollection
    {
        BrushCollection GetBrushCollection(object key);
        IResolveBrushCollection IResolveBrushCollectionParent { get; }
        IEnumerable<BrushCollectionListItem> ResolvableBrushCollections { get; }
    }

    public static class IResolveBrushCollectionHelper
    {
        public static IResolveBrushCollection GetIResolveBrushCollection(this IBasePart self)
        {
            if (self.NameManager is IResolveBrushCollection)
            {
                return self.NameManager as IResolveBrushCollection;
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
                    return new ICorePartBrushCollectionResolver(_folder);
                }
            }
            return null;
        }
    }
}
