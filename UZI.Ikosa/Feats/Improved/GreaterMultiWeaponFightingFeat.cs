using System;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Items;
using Uzi.Core;

namespace Uzi.Ikosa.Feats
{
    [   
    Serializable,
    FighterBonusFeat,
    BaseAttackRequirement(15),
    AbilityRequirement(Abilities.MnemonicCode.Dex, 19),
    ItemSlotRequirement(@"Three or More Hands", @"Creature must have three or more holding slots", ItemSlot.HoldingSlot, null, 3, int.MaxValue),
    FeatChainRequirement(typeof(MultiWeaponFightingFeat)),
    FeatChainRequirement(typeof(ImprovedMultiWeaponFightingFeat)),
    FeatInfo(@"Greater Multi-Weapon Fighting")
    ]
    public class GreaterMultiWeaponFightingFeat : FeatBase
    {
        public GreaterMultiWeaponFightingFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
            _Boost = new Delta(1, typeof(GreaterMultiWeaponFightingFeat));
        }

        private Delta _Boost;

        public override string Benefit
        {
            get { return @"3rd attacks with off-hands at -10 penalty"; }
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            Creature.OffHandIterations.Deltas.Add(_Boost);
        }

        protected override void OnDeactivate()
        {
            _Boost.DoTerminate();
            base.OnDeactivate();
        }
    }
}