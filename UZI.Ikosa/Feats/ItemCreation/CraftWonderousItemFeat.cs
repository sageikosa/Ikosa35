using System;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo("Craft Wonderous Item"),
    CasterLevelRequirement(3)
    ]
    public class CraftWonderousItemFeat : ItemCreationFeatBase
    {
        public CraftWonderousItemFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit
        {
            get { return @"You can create wondrous items.  You can also mend broken wondrous items."; }
        }
    }
}
