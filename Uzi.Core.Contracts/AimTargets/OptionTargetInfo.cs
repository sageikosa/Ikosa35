using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Uzi.Core;

namespace Uzi.Core.Contracts
{
    // NOTE: to be used mostly for communication from client back to server
    [DataContract(Namespace = Statics.Namespace)]
    public class OptionTargetInfo : AimTargetInfo
    {
        public OptionTargetInfo()
            : base()
        {
        }

        [DataMember]
        public string OptionKey { get; set; }
        [DataMember]
        public string OptionName { get; set; }
        [DataMember]
        public string OptionDescription { get; set; }
    }
}
