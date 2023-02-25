using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Uzi.Visualize.Contracts;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Uzi.Visualize
{
    [CollectionDataContract(Namespace = Statics.Namespace)]
    public class DoubleReferenceCollection : Collection<DoubleReference>
    {
        public DoubleReferenceCollection()
        {
        }

        public DoubleReferenceCollection(IList<DoubleReference> source)
            : base(source)
        {
        }

        public DoubleReference this[string key]
        {
            get
            {
                return this.FirstOrDefault(_n => _n.Key.Equals(key));
            }
        }


        public IEnumerable<DoubleReference> Ordered
        {
            get
            {
                return this.OrderBy(_d => _d.Key);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsExpanded { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsSelected { get; set; }
    }
}
