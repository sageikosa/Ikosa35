using System;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo("Forge Ring"),
    CasterLevelRequirement(12)
    ]
    public class ForgeRingFeat : ItemCreationFeatBase
    {
        public ForgeRingFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit
        {
            get { return "You can create any ring whose prerequisites you meet.  You can also mend a broken ring if it is one that you could make."; }
        }
    }
}
