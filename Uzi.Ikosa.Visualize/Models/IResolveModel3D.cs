using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Uzi.Visualize.Packaging;
using Uzi.Packaging;

namespace Uzi.Visualize
{
    /// <summary>
    /// Provides Model3D to IkosaExtension when given a key
    /// </summary>
    public interface IResolveModel3D
    {
        /// <summary>Gets a model 3D distinct for the caller (rematerializes for each call)</summary>
        Model3D GetPrivateModel3D(object key);

        /// <summary>True if model 3D is defined for this resolver</summary>
        bool CanResolveModel3D(object key);

        /// <summary>Used to provide lists of models that are expected to resolve at runtime</summary>
        IResolveModel3D IResolveModel3DParent { get; }

        /// <summary>Used to provide lists of models that are expected to resolve at runtime</summary>
        IEnumerable<Model3DPartListItem> ResolvableModels { get; }
    }

    public static class IResolveModel3DHelper
    {
        public static IResolveModel3D GetIResolveModel3D(this IBasePart self)
        {
            if (self.NameManager is IResolveModel3D)
            {
                return self.NameManager as IResolveModel3D;
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
                    return new ICorePartModel3DResolver(_folder);
                }
            }
            return null;
        }
    }
}
