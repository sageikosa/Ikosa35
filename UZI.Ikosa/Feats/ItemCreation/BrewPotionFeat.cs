using System;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo("Brew Potion"),
    CasterLevelRequirement(3)
    ]
    public class BrewPotionFeat: ItemCreationFeatBase
    {
        public BrewPotionFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit
        {
            get { return "You can create a potion of any 3rd-level or lower spell that you know and that targets one or more creatures."; }
        }
    }
}
