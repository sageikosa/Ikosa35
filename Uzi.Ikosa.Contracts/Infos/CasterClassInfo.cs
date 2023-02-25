using System;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class CasterClassInfo : PowerClassInfo
    {
        public CasterClassInfo()
        {
        }

        protected CasterClassInfo(CasterClassInfo copySource)
            : base(copySource)
        {
            MagicType = copySource.MagicType;
            Alignment = copySource.Alignment;
            EffectiveLevel = copySource.EffectiveLevel.Clone() as DeltableInfo;
            SpellDifficultyBase = copySource.SpellDifficultyBase.Clone() as DeltableInfo;
        }

        [DataMember]
        public MagicType MagicType { get; set; }
        [DataMember]
        public string Alignment { get; set; }
        [DataMember]
        public VolatileValueInfo EffectiveLevel { get; set; }
        [DataMember]
        public DeltableInfo SpellDifficultyBase { get; set; }

        public override object Clone()
        {
            return new CasterClassInfo(this);
        }
    }
}
