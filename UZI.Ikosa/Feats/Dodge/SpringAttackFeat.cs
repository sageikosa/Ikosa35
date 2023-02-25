using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Feats
{
    [
        Serializable,
        FeatInfo(@"Spring Attack", true),
        AbilityRequirement(MnemonicCode.Dex, 13),
        FeatChainRequirement(typeof(DodgeFeat)),
        FeatChainRequirement(typeof(MobilityFeat)),
        BaseAttackRequirement(4),
        FighterBonusFeat
    ]
    public class SpringAttackFeat : FeatBase
    {
        public SpringAttackFeat(object source, int powerLevel) 
            : base(source, powerLevel)
        {
        }

        public override string Benefit => @"Move before and after melee attack to avoid opportunistic attack from target.";

        protected override void OnActivate()
        {
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
        }
    }
}
