using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class ConditionDamageInfo : DamageInfo
    {
        [DataMember]
        public string Condition { get; set; }
        [DataMember]
        public string TimeUnit { get; set; }
    }
}
