using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Uzi.Core;

namespace Uzi.Core.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class ObservedActivityInfo : Info
    {
        #region construction
        public ObservedActivityInfo()
            : base()
        {
        }
        #endregion

        [DataMember]
        public Guid ActorID { get; set; }
        [DataMember]
        public Info Actor { get; set; }
        [DataMember]
        public Info[] Targets { get; set; }
        [DataMember]
        public Info Implement { get; set; }
        [DataMember]
        public Info Details { get; set; }

        public override object Clone()
        {
            return new ObservedActivityInfo
            {
                Actor = Actor,
                ActorID = ActorID,
                Targets = Targets,
                Implement = Implement,
                Details = Details,
                Message = Message
            };
        }
    }
}
