using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Ikosa.Feats
{
    [
        Serializable,
        FighterBonusFeat,
        FeatInfo(@"Improved Shield Bash")
    ]
    public class ImprovedShieldBash : FeatBase
    {
        public ImprovedShieldBash(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit => @"Continue to apply shield delta to armor-rating after shield bash";
        public override string Name => @"Improved Shield Bash";
        public override string PreRequisite => @"Shield proficiency";

        public override bool MeetsPreRequisite(Creature creature)
        {
            return base.MeetsPreRequisite(creature)
                && creature.Proficiencies.IsProficientWithShield(false, PowerLevel);
        }

        public override bool MeetsRequirementsAtPowerLevel
        {
            get
            {
                return base.MeetsRequirementsAtPowerLevel
                && Creature.Proficiencies.IsProficientWithShield(false, PowerLevel);
            }
        }
    }
}
