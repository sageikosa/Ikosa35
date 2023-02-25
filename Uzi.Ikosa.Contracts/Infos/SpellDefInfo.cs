using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class SpellDefInfo : MagicPowerDefInfo
    {
        public SpellDefInfo()
        {
        }

        protected SpellDefInfo(SpellDefInfo copySource)
            : base(copySource)
        {
            ArcaneCharisma = copySource.ArcaneCharisma;
            MetaMagics = copySource.MetaMagics.Select(_mm => _mm.Clone() as MetaMagicInfo).ToList();
        }

        [DataMember]
        public bool ArcaneCharisma { get; set; }

        [DataMember]
        public List<MetaMagicInfo> MetaMagics { get; set; }

        public override object Clone()
            => new SpellDefInfo(this);
    }
}
