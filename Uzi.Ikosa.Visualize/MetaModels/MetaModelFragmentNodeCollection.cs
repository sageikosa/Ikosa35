using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using Uzi.Visualize.Contracts;

namespace Uzi.Visualize
{
    [CollectionDataContract(Namespace = Statics.Namespace)]
    public class MetaModelFragmentNodeCollection : Collection<MetaModelFragmentNode>
    {
        public IEnumerable<MetaModelFragmentNode> this[string key]
            => this.Where(_n => _n.ReferenceKey.Equals(key));

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsExpanded { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsSelected { get; set; }
    }
}
