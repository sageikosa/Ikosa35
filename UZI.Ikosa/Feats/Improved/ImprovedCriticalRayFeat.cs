using System;
using System.Linq;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FighterBonusFeat,
    FeatInfo(@"Improved Critical: Ray"),
    BaseAttackRequirement(8)
    ]
    public class ImprovedCriticalRayFeat : FeatBase
    {
        public ImprovedCriticalRayFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit { get { return string.Format(@"Critical threat range doubled for {0}", @"Ray Attacks"); } }

        public static int CriticalThreatStart(Creature critter)
        {
            if (critter.Feats.Contains(typeof(ImprovedCriticalRayFeat)))
            {
                if (critter.Feats.OfType<ImprovedCriticalRayFeat>()
                    .First().IsActive)
                {
                    return 19;
                }
            }
            return 20;
        }
    }
}
