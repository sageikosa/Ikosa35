using System;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class AdjunctInfo : Info
    {
        public AdjunctInfo()
        {
        }

        public AdjunctInfo(string title, Guid id)
        {
            Message = title;
            ID = id;
        }

        [DataMember]
        public Guid ID { get; set; }

        public override object Clone()
        {
            return new AdjunctInfo(Message, ID);
        }
    }
}
