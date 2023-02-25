using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class AdvancementOptionInfo
    {
        [DataMember]
        public string FullName { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public List<AdvancementOptionInfo> AvailableParameters { get; set; }
        [DataMember]
        public AdvancementOptionInfo ParameterValue { get; set; }

        public AdvancementOptionInfo EnfoldParameter(AdvancementOptionInfo info)
            => new AdvancementOptionInfo
            {
                FullName = FullName,
                Name = Name,
                Description = Description,
                ParameterValue = info
            };
    }
}
