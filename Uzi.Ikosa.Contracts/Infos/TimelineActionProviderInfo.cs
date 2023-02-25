using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class TimelineActionProviderInfo : AdjunctInfo
    {
        public TimelineActionProviderInfo()
        {
        }

        public TimelineActionProviderInfo(string title, Guid id)
            :base(title, id)
        {
        }

        public override object Clone()
            => new TimelineActionProviderInfo(Message, ID);
    }
}
