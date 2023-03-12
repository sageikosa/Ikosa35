using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Ikosa.Packaging;

namespace Uzi.Visualize.Packages
{
    public interface IPartResolveModel3D
    {
        /// <summary>Gets a model 3D distinct for the caller (rematerializes for each call)</summary>
        Model3D GetPrivateModel3D(object key);

        /// <summary>True if model 3D is defined for this resolver</summary>
        bool CanResolveModel3D(object key);

        /// <summary>Used to provide lists of models that are expected to resolve at runtime</summary>
        IPartResolveModel3D IPartResolveModel3DParent { get; }

        /// <summary>Used to provide lists of models that are expected to resolve at runtime</summary>
        IEnumerable<Model3DPartListItem> ResolvableModels { get; }
    }

    public static class IPartResolveModel3DHelper
    {
        public static IPartResolveModel3D GetIResolveModel3D(this IStorablePart self)
        {
            if (self.PartNameManager is IPartResolveModel3D _manager)
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
