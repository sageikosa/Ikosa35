using System;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FighterBonusFeat,
    AbilityRequirement(Abilities.MnemonicCode.Dex, 13),
    FeatChainRequirement(typeof(ImprovedUnarmedStrikeFeat)),
    FeatInfo(@"Improved Grapple")
    ]
    public class ImprovedGrappleFeat : FeatBase
    {
        public ImprovedGrappleFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit
            => @"No attack-of-opportunity to start a grapple.  +4 on all grapple checks.";
    }
}
