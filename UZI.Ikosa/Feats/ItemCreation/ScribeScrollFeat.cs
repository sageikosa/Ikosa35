using System;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo(@"Scribe Scroll"),
    CasterLevelRequirement(1)
    ]
    public class ScribeScrollFeat : ItemCreationFeatBase
    {
        public ScribeScrollFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit
        {
            get { return @"You can create a scroll of any spells that you know."; }
        }
    }
}
