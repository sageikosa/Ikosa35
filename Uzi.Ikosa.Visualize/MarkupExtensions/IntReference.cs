using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Uzi.Visualize.Contracts;

namespace Uzi.Visualize
{
    [DataContract(Namespace = Statics.Namespace)]
    public class IntReference : ParameterReference<Int32>
    {
        public IntReference()
            : base()
        {
        }

        public IntReference(IntReference source)
            : base(source)
        {
        }
    }
}
