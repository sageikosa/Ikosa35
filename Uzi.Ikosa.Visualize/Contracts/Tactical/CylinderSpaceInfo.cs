using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.Namespace)]
    public class CylinderSpaceInfo : CellSpaceInfo
    {
        public CylinderSpaceInfo()
        {
        }

        public CylinderSpaceInfo(IPlusCellSpace cylinder)
            : base(cylinder)
        {
            PlusMaterial = cylinder.PlusMaterialName;
            PlusTiling = cylinder.PlusTilingName;
            IsPlusGas = cylinder.IsPlusGas;
            IsPlusSolid = cylinder.IsPlusSolid;
            IsPlusLiquid = cylinder.IsPlusLiquid;
            IsPlusInvisible = cylinder.IsPlusInvisible;
        }

        [DataMember]
        public bool IsPlusGas { get; set; }
        [DataMember]
        public bool IsPlusSolid { get; set; }
        [DataMember]
        public bool IsPlusLiquid { get; set; }
        [DataMember]
        public bool IsPlusInvisible { get; set; }
        [DataMember]
        public string PlusMaterial { get; set; }
        [DataMember]
        public string PlusTiling { get; set; }
    }
}
