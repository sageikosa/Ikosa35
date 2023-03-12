using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Ikosa.Packaging;

namespace Uzi.Visualize.Packages
{
    public interface IPartResolveFragment
    {
        Model3D GetFragment(FragmentReference fragRef, MetaModelFragmentNode node);
        IPartResolveFragment IPartResolveFragmentParent { get; }

        /// <summary>Used to provide lists of fragments that are expected to resolve at runtime</summary>
        IEnumerable<MetaModelFragmentPartListItem> ResolvableFragments { get; }
    }

    public static class IPartResolveFragmentHelper
    {
        public static IPartResolveFragment GetIPartResolveFragment(this IStorablePart self)
        {
            if (self.PartNameManager is IPartResolveFragment _manager)
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
