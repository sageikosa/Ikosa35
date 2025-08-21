using System;
using System.Runtime.Serialization;
using System.Windows.Markup;
using Uzi.Visualize.Contracts;

namespace Uzi.Visualize
{
    [DataContract(Namespace = Statics.Namespace)]
    public abstract class ParameterExtension<ParamType> : MarkupExtension
    {
        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public ParamType Value { get; set; }

        [DataMember]
        public ParamType MinValue { get; set; }

        [DataMember]
        public ParamType MaxValue { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Value;
        }
    }
}
