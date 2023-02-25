using System;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Magic;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo(@"Empower Spell"),
    CasterLevelRequirement(1)
    ]
    public class EmpowerSpellFeat : MetamagicFeatBase
    {
        public EmpowerSpellFeat(object source, int powerLevel)
            : base(source, powerLevel, 2)
        {
        }

        public static string StaticMetaTag => @"Empowered";
        public static string StaticMetaBenefit => @"All variable, numeric effects of an empowered spell are increased by one-half.";

        public override string Benefit
            => DecorateWithLevelInfo(StaticMetaBenefit);

        public override string MetaMagicTag => StaticMetaTag;
        public override string MetaMagicBenefit => StaticMetaBenefit;

        public override ISpellDef ApplyMetamagic(ISpellDef spellDef, bool isSpontaneous)
        {
            // TODO: empower wrapper
            return spellDef;
        }
    }
}
