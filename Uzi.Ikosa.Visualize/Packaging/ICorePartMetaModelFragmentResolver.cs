using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uzi.Packaging;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize.Packaging
{
    /// <summary>Resolves Meta-Model Fragments related under an ICorePart</summary>
    public class ICorePartMetaModelFragmentResolver : IResolveFragment
    {
        /// <summary>Resolves bitmap images related under an ICorePart</summary>
        public ICorePartMetaModelFragmentResolver(ICorePart part)
        {
            _Part = part;
        }

        private ICorePart _Part;

        public ICorePart Part { get { return _Part; } }

        #region IResolveFragment Members

        public Model3D GetFragment(FragmentReference fragRef, MetaModelFragmentNode node)
        {
            var _group = new Model3DGroup();

            if (_Part.Relationships.OfType<MetaModelFragment>().Any(_mmf => _mmf.Name == node.FragmentKey))
            {
                var _fragPart = _Part.Relationships.OfType<MetaModelFragment>()
                    .FirstOrDefault(_mmf => _mmf.Name == node.FragmentKey);
                var _mdl = _fragPart.ResolveModel(node);

                // collect and end resolve of fragment
                if (_mdl != null)
                    _group.Children.Add(fragRef.ApplyTransforms(node, _mdl));
            }

            // return smallest possible grouping
            if (_group.Children.Count == 1)
                return _group.Children.First();
            else if (_group.Children.Any())
                return _group;
            return null;
        }

        public IResolveFragment IResolveFragmentParent { get { return null; } }

        public IEnumerable<MetaModelFragmentListItem> ResolvableFragments
        {
            get
            {
                if (_Part != null)
                    return _Part.Relationships.OfType<MetaModelFragment>()
                        .Select(_mmf => new MetaModelFragmentListItem
                        {
                            MetaModelFragment = _mmf,
                            IsLocal = true
                        });

                // empty list
                return new MetaModelFragmentListItem[] { };
            }
        }

        #endregion
    }
}
