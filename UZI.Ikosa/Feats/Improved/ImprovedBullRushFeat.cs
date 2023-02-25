using System;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FighterBonusFeat,
    AbilityRequirement(Abilities.MnemonicCode.Str, 13),
    FeatChainRequirement(typeof(PowerAttackFeat)),
    FeatInfo("Improved Bull Rush")
    ]
    public class ImprovedBullRushFeat: FeatBase
    {
        public ImprovedBullRushFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit
        {
            get 
            {
                return "Will not provoke an attack-of-opportunity on bull-rush.  +4 bonus on strength check to push defender.";
            }
        }
    }
}
