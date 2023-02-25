using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Uzi.Core.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class CoreInfo : Info
    {
        public CoreInfo()
        {
        }

        public CoreInfo(CoreInfo source)
        {
            ID = source.ID;
            Message = source.Message;
        }

        [DataMember]
        public Guid ID { get; set; }

        public virtual string CompareKey => ID.ToString();

        // TODO: OGLReference...

        public override object Clone()
        {
            return new CoreInfo(this);
        }
    }
}
