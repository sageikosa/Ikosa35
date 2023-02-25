using System;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo("Craft Staff"),
    CasterLevelRequirement(12)
    ]
    public class CraftStaffFeat : ItemCreationFeatBase
    {
        public CraftStaffFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit
        {
            get { return "You can create any staff whose prerequisites you meet."; }
        }
    }
}
