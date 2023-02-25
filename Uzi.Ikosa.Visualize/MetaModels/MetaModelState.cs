using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Visualize.Contracts;

namespace Uzi.Visualize
{
    [DataContract(Namespace = Statics.Namespace)]
    public class MetaModelState
    {
        #region ctor()
        public MetaModelState()
        {
            DefaultBrushes = new BrushCrossRefNodeCollection();
            RootFragment = new MetaModelRootNode();
            MasterIntReferences = new IntReferenceCollection();
            MasterDoubleReferences = new DoubleReferenceCollection();
        }

        public MetaModelState(MetaModelState source)
        {
            // clone all masters and defaults
            DefaultBrushes = new BrushCrossRefNodeCollection(
                source.DefaultBrushes.Select(_db => new BrushCrossRefNode(_db)).ToList());
            MasterIntReferences = new IntReferenceCollection(
                source.MasterIntReferences.Select(_mi => new IntReference(_mi)).ToList());
            MasterDoubleReferences = new DoubleReferenceCollection(
                source.MasterDoubleReferences.Select(_md => new DoubleReference(_md)).ToList());

            // root fragment
            RootFragment = new MetaModelRootNode(source.RootFragment);
        }
        #endregion

        [DataMember]
        public BrushCrossRefNodeCollection DefaultBrushes { get; set; }
        [DataMember]
        public MetaModelRootNode RootFragment { get; set; }
        [DataMember]
        public IntReferenceCollection MasterIntReferences { get; set; }
        [DataMember]
        public DoubleReferenceCollection MasterDoubleReferences { get; set; }

        private MetaModelFragmentNodeCollection FindParent(MetaModelFragmentNode child, MetaModelFragmentNodeCollection maybeParent)
        {
            if (maybeParent.Contains(child))
                return maybeParent;
            foreach (var _next in maybeParent)
            {
                var _found = FindParent(child, _next.Components);
                if (_found != null)
                    return _found;
            }
            return null;
        }

        public MetaModelFragmentNodeCollection FindParent(MetaModelFragmentNode fragNode)
            => FindParent(fragNode, RootFragment.Components);

        public IEnumerable<MetaModelRootNode> RootList
            => new[] { RootFragment };
    }
}
