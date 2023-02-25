using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Uzi.Core.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class RollerLog
    {
        [DataMember]
        public string Expression { get; set; }
        [DataMember]
        public int Total { get; set; }
        [DataMember]
        public List<RollerLog> Parts { get; set; }

        public string PartList()
        {
            if (Parts?.Any() ?? false)
            {
                string.Join(@", ", Parts.Select(_p => _p.Total.ToString()));
            }
            return null;
        }
    }
}
