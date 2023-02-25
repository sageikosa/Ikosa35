using System;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo("Craft Magic Arms and Armor"),
    CasterLevelRequirement(5)
    ]
    public class CraftMagicArmsAndArmorFeat : ItemCreationFeatBase
    {
        public CraftMagicArmsAndArmorFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit
        {
            get { return "You can create any magic weapon, armor, or shield whose prerequisites you meet."; }
        }
    }
}
