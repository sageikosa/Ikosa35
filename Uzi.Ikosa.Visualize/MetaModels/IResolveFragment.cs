using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uzi.Visualize.Packaging;
using System.Windows.Media.Media3D;
using Uzi.Packaging;

namespace Uzi.Visualize
{
    public interface IResolveFragment
    {
        Model3D GetFragment(FragmentReference fragRef, MetaModelFragmentNode node);
        IResolveFragment IResolveFragmentParent { get; }

        /// <summary>Used to provide lists of fragments that are expected to resolve at runtime</summary>
        IEnumerable<MetaModelFragmentListItem> ResolvableFragments { get; }
    }

    public static class IResolveFragmentHelper
    {
        public static IResolveFragment GetIResolveFragment(this IBasePart self)
        {
            if (self.NameManager is IResolveFragment)
            {
                return self.NameManager as IResolveFragment;
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
                    return new ICorePartMetaModelFragmentResolver(_folder);
                }
            }
            return null;
        }
    }
}
