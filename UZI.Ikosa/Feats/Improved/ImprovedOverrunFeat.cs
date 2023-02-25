using System;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FighterBonusFeat,
    AbilityRequirement(Abilities.MnemonicCode.Str, 13),
    FeatChainRequirement(typeof(PowerAttackFeat)),
    FeatInfo(@"Improved Overrun")
    ]
    public class ImprovedOverrunFeat : FeatBase
    {
        public ImprovedOverrunFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit
        {
            get { return @"Target cannot avoid overrun.  +4 bonus on strength check to known down opponent."; }
        }
    }
}
