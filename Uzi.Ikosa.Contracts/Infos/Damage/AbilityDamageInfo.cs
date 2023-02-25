using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class AbilityDamageInfo : DamageInfo
    {
        [DataMember]
        public string AbilityMnemonic { get; set; }
        [DataMember]
        public bool IsDrain { get; set; }
    }
}
