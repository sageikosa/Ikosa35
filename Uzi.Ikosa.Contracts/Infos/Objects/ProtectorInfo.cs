using System;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class ProtectorInfo : ObjectInfo
    {
        #region construction
        public ProtectorInfo()
            : base()
        {
        }

        protected ProtectorInfo(ProtectorInfo copySource)
            : base(copySource)
        {
            ProtectionBonus = copySource.ProtectionBonus.Clone() as DeltableInfo;
            CheckPenalty = copySource.CheckPenalty.Clone() as DeltableInfo;
            ArcaneSpellFailureChance = copySource.ArcaneSpellFailureChance.Clone() as DeltableInfo;
        }
        #endregion

        [DataMember]
        public DeltableInfo ProtectionBonus { get; set; }
        [DataMember]
        public DeltableInfo CheckPenalty { get; set; }
        [DataMember]
        public DeltableInfo ArcaneSpellFailureChance { get; set; }

        public override object Clone()
        {
            return new ProtectorInfo(this);
        }
    }
}
