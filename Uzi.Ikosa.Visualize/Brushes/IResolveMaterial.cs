using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Uzi.Packaging;
using Uzi.Visualize.Packaging;

namespace Uzi.Visualize
{
    /// <summary>Provides Material to XAML Reader</summary>
    public interface IResolveMaterial
    {
        Material GetMaterial(object key, VisualEffect effect);
        IResolveMaterial IResolveMaterialParent { get; }
        IEnumerable<BrushDefinitionListItem> ResolvableBrushes { get; }
    }

    public static class IResolveMaterialHelper
    {
        public static IResolveMaterial GetIResolveMaterial(this IBasePart self)
        {
            if (self.NameManager is IResolveMaterial)
            {
                return self.NameManager as IResolveMaterial;
            }
            else if (self.NameManager is CorePackagePartsFolder)
            {
                var _folder = self.NameManager as CorePackagePartsFolder;
                if (_folder.NameManager is VisualResources)
                {
                    return _folder.NameManager as VisualResources;
                }
            }
            return null;
        }
    }
}
