using System;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FighterBonusFeat,
    FeatChainRequirement(typeof(PointBlankShotFeat)),
    FeatInfo("Far Shot")
    ]
    public class FarShotFeat: FeatBase
    {
        public FarShotFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit
        {
            get 
            {
                return @"Increases range increment of projectile weapons by 1/2, and doubles thrown weapon ranges.";
            }
        }
    }
}
