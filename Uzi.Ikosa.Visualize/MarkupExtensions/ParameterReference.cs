using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Uzi.Visualize.Contracts;

namespace Uzi.Visualize
{
    [DataContract(Namespace = Statics.Namespace)]
    public abstract class ParameterReference<ParamType>
    {
        protected ParameterReference()
        {
        }

        protected ParameterReference(ParameterReference<ParamType> source)
        {
            UseMaster = source.UseMaster;
            Key = source.Key;
            Value = source.Value;
            MinValue = source.MinValue;
            MaxValue = source.MaxValue;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsActive { get; set; }

        [DataMember]
        public bool UseMaster { get; set; }

        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public ParamType Value { get; set; }

        [DataMember]
        public ParamType MinValue { get; set; }

        [DataMember]
        public ParamType MaxValue { get; set; }

        public override string ToString()
            => $@"'{Key}'={Value} [UM={UseMaster}, A={IsActive}]";
    }
}
