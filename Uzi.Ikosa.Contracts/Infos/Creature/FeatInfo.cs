using System;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class FeatInfo : Info
    {
        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public string FullName { get; set; }
        [DataMember]
        public int PowerLevel { get; set; }
        [DataMember]
        public bool IsBonusFeat { get; set; }
        [DataMember]
        public string PreRequisite { get; set; }
        [DataMember]
        public string Benefit { get; set; }

        public string FeatName => Message;

        public override object Clone()
        {
            return new FeatInfo
            {
                Message = Message,
                FullName = FullName,
                PowerLevel = PowerLevel,
                IsBonusFeat = IsBonusFeat,
                PreRequisite = PreRequisite,
                Benefit = Benefit,
                ID = ID
            };
        }
    }
}