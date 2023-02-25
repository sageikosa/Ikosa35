using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Uzi.Core.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class Info : ICloneable
    {
        [DataMember]
        public string Message { get; set; }

        public string TypeName => GetType().FullName;

        #region ICloneable Members

        public virtual object Clone()
        {
            return new Info { Message = Message };
        }

        #endregion
    }
}
