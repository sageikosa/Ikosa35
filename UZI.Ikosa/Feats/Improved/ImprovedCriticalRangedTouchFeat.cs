using System;
using System.Linq;
using Uzi.Ikosa.Advancement;
using Uzi.Core;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FighterBonusFeat,
    FeatInfo(@"Improved Critical: Ranged Touch"),
    BaseAttackRequirement(8)
    ]
    public class ImprovedCriticalRangedTouchFeat : FeatBase
    {
        public ImprovedCriticalRangedTouchFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit
        {
            get { return string.Format(@"Critical threat range doubled for {0}", "Ranged Touch Attacks"); }
        }

        public static int CriticalThreatStart(CoreActor critter)
        {
            var _critter = critter as Creature;
            if ((_critter !=null) && _critter.Feats.Contains(typeof(ImprovedCriticalRangedTouchFeat)))
            {
                if (_critter.Feats.OfType<ImprovedCriticalRangedTouchFeat>()
                    .First().IsActive)
                    return 19;
            }
            return 20;
        }
    }
}
