using System;
using System.Runtime.Serialization;
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
