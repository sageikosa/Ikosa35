using System;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo("Craft Wand"),
    CasterLevelRequirement(5)
    ]
    public class CraftWandFeat : ItemCreationFeatBase
    {
        public CraftWandFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit
        {
            get { return @"You can create a wand of any 4th-level or lower spell that you know."; }
        }
    }
}
