using System;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FighterBonusFeat,
    AbilityRequirement(Abilities.MnemonicCode.Int, 13),
    FeatChainRequirement(typeof(ImprovedDefensiveCombatFeat)),
    FeatInfo(@"Improved Trip")
    ]
    public class ImprovedTripFeat: FeatBase
    {
        public ImprovedTripFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit
        {
            get 
            {
                return @"Will not provoke an attack-of-opportunity on trip.  +4 to strength to trip.  Make an immediate melee attack if trip opponent in melee";
            }
        }
    }
}
