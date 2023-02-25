using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class AmmoBundleInfo : ObjectInfo
    {
        public AmmoBundleInfo() : base()
        {
        }

        protected AmmoBundleInfo(AmmoBundleInfo copySource)
            : base(copySource)
        {
            Ammunitions = copySource.Ammunitions?.Select(_a => _a.Clone() as AmmoInfo).ToList();
            Capacity = copySource.Capacity;
        }

        [DataMember]
        public List<AmmoInfo> Ammunitions { get; set; }

        [DataMember]
        public int? Capacity { get; set; }

        public override object Clone()
        {
            return new AmmoBundleInfo(this);
        }
    }
}
