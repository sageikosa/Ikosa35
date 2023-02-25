using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Uzi.Core.Contracts.Faults
{
    [DataContract(Namespace = Statics.Namespace)]
    public class InvalidArgumentFault
    {
        public InvalidArgumentFault()
        {
        }

        public InvalidArgumentFault(string arg)
        {
        }

        [DataMember]
        public string Argument { get; set; }
    }
}
