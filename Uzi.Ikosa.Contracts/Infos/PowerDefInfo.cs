using System;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class PowerDefInfo : Info
    {
        public PowerDefInfo()
        {
        }

        protected PowerDefInfo(PowerDefInfo copySource)
        {
            Message = copySource.Message;
            Description = copySource.Description;
            Descriptors = copySource.Descriptors.Select(_d => _d).ToArray();
            Key = copySource.Key;
        }

        [DataMember]
        public string Key { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string[] Descriptors { get; set; }

        public override object Clone()
            => new PowerDefInfo(this);
    }
}
