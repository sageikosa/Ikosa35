using System;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo(@"Quick Draw"),
    FighterBonusFeat,
    BaseAttackRequirement(1)
    ]
    public class QuickDrawFeat : FeatBase
    {
        public QuickDrawFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit
        {
            get { return @"Draw weapon as a free action.  Draw hidden weapon as a brief action."; }
        }
    }
}
