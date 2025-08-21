using System;
using System.Runtime.Serialization;
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
