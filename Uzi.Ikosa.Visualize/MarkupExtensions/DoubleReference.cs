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
    public class DoubleReference : ParameterReference<Double>
    {
        public DoubleReference()
            : base()
        {
        }

        public DoubleReference(DoubleReference source)
            : base(source)
        {
        }
    }
}
