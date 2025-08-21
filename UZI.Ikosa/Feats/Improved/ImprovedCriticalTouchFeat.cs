using System;
using System.Linq;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FighterBonusFeat,
    FeatInfo("Improved Critical: Touch"),
    BaseAttackRequirement(8)
    ]
    public class ImprovedCriticalTouchFeat : FeatBase
    {
        public ImprovedCriticalTouchFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit { get { return string.Format(@"Critical threat range doubled for {0}", @"Touch Attacks"); } }

        public static int CriticalThreatStart(Creature critter)
        {
            if (critter.Feats.Contains(typeof(ImprovedCriticalTouchFeat)))
            {
                if (critter.Feats.OfType<ImprovedCriticalTouchFeat>()
                    .First().IsActive)
                {
                    return 19;
                }
            }
            return 20;
        }
    }
}
