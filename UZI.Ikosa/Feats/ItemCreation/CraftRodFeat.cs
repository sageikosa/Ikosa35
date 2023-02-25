using System;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo("Craft Rod"),
    CasterLevelRequirement(9)
    ]
    public class CraftRodFeat:ItemCreationFeatBase
    {
        public CraftRodFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit
        {
            get { return "You can create any rod whose prerequisites you meet."; }
        }
    }
}
