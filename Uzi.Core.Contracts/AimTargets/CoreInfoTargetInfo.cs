using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Core.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class CoreInfoTargetInfo : AimTargetInfo
    {
        public CoreInfoTargetInfo() : base()
        {
        }

        [DataMember]
        public CoreInfo CoreInfo { get; set; }
    }
}
