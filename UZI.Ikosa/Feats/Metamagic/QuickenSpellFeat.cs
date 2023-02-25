using System;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Magic;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo(@"Quicken Spell"),
    CasterLevelRequirement(1)
    ]
    public class QuickenSpellFeat : MetamagicFeatBase
    {
        public QuickenSpellFeat(object source, int powerLevel)
            : base(source, powerLevel, 4)
        {
        }

        public static string StaticMetaTag => @"Quickened";
        public static string StaticMetaBenefit => @"Spell can be cast as a twitch action.";

        public override string Benefit
            => DecorateWithLevelInfo(StaticMetaBenefit);

        public override string MetaMagicTag => StaticMetaTag;
        public override string MetaMagicBenefit => StaticMetaBenefit;

        public override ISpellDef ApplyMetamagic(ISpellDef spellDef, bool isSpontaneous)
        {
            if (!isSpontaneous)
            {
                // should only be used when preparing spells for the day...
                return new QuickenSpellDef(spellDef, PresenterID, false);
            }
            else
            {
                return spellDef;
            }
        }
    }
}
