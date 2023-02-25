using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Core.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class SysNotify
    {
        public SysNotify(string topic, params Info[] infos)
        {
            Topic = topic;
            Infos = infos.ToList();
        }

        [DataMember]
        public string Topic { get; set; }

        [DataMember]
        public List<Info> Infos { get; set; }
    }
}
