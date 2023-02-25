using System.Runtime.Serialization;
using System.ComponentModel;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class UserDefinition
    {
        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [DefaultValue(false)]
        public bool IsMasterUser { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [DefaultValue(false)]
        public bool IsDisabled { get; set; }
    }
}
