using System.Runtime.Serialization;
using System.Collections.ObjectModel;

namespace Uzi.Ikosa.Contracts.Host
{
    [DataContract(Namespace = Statics.Namespace)]
    public class IkosaPartInfo
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public Collection<IkosaPartInfo> Relationships { get; set; }
        [DataMember]
        public string TypeName { get; set; }
    }
}
