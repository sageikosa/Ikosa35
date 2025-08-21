using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using Uzi.Visualize.Contracts;

namespace Uzi.Visualize
{
    [CollectionDataContract(Namespace = Statics.Namespace)]
    public class BrushCrossRefNodeCollection : Collection<BrushCrossRefNode>
    {
        public BrushCrossRefNodeCollection()
        {
        }

        public BrushCrossRefNodeCollection(IList<BrushCrossRefNode> source)
            : base(source)
        {
        }

        public BrushCrossRefNode this[string key]
            => this.FirstOrDefault(_n => _n.ReferenceKey.Equals(key));

        public IEnumerable<BrushCrossRefNode> Ordered
            => this.OrderBy(_b => _b.ReferenceKey);

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsExpanded { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsSelected { get; set; }
    }
}
