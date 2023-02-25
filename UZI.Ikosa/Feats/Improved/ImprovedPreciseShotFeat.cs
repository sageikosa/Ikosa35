using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Feats
{
    [
        Serializable,
        FighterBonusFeat,
        AbilityRequirement(Abilities.MnemonicCode.Dex, 19),
        BaseAttackRequirement(11),
        FeatChainRequirement(typeof(PointBlankShotFeat)),
        FeatChainRequirement(typeof(PreciseShotFeat)),
        FeatInfo(@"Improved Precise Shot")
        ]
    public class ImprovedPreciseShotFeat : FeatBase
    {
        public ImprovedPreciseShotFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit 
            => @"Ranged attacks ignore cover and miss change for concealment.  Total cover and total concealment are effective normally.";

        // TODO: ¿¡¿ give bonus to overcome cover...?!?  miss chance alterations also...?!?
    }
}
