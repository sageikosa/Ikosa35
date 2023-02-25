using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class AdvancementRequirementInfo
    {
        [DataMember]
        public RequirementKeyInfo Key { get; set; }
        [DataMember]
        public virtual string Name { get; set; }
        [DataMember]
        public virtual string Description { get; set; }
        [DataMember]
        public FeatureInfo CurrentValue { get; set; }
        [DataMember]
        public List<AdvancementOptionInfo> AvailableOptions { get; set; }
    }
}
