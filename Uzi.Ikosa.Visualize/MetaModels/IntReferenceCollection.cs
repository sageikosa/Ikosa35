using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Visualize.Contracts;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Uzi.Visualize
{
    [CollectionDataContract(Namespace = Statics.Namespace)]
    public class IntReferenceCollection : Collection<IntReference>
    {
        public IntReferenceCollection()
        {
        }

        public IntReferenceCollection(IList<IntReference> source)
            : base(source)
        {
        }

        public IntReference this[string key]
        {
            get
            {
                return this.FirstOrDefault(_n => _n.Key.Equals(key));
            }
        }

        public IEnumerable<IntReference> Ordered
        {
            get
            {
                return this.OrderBy(_i => _i.Key);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsExpanded { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsSelected { get; set; }
    }
}
