using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Core.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class QuantitySelectorInfo
    {
        [DataMember]
        public CoreInfo CoreInfo { get; set; }
        [DataMember]
        public int CurrentSelection { get; set; }
        [DataMember]
        public int MinimumSelection { get; set; }
        [DataMember]
        public int MaximumSelection { get; set; }
    }
}
