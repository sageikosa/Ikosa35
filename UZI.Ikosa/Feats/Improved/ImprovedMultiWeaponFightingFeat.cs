using System;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Items;
using Uzi.Core;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FighterBonusFeat,
    AbilityRequirement(Abilities.MnemonicCode.Dex, 15),
    ItemSlotRequirement(@"Three or More Hands", @"Creature must have three or more holding slots", ItemSlot.HoldingSlot, null, 3, int.MaxValue),
    FeatChainRequirement(typeof(MultiWeaponFightingFeat)),
    FeatInfo(@"Improved Multi-Weapon Fighting"),
    BaseAttackRequirement(9)
    ]
    public class ImprovedMultiWeaponFightingFeat : FeatBase
    {
        public ImprovedMultiWeaponFightingFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
            _Boost = new Delta(1, typeof(ImprovedMultiWeaponFightingFeat));
        }

        private Delta _Boost;

        public override string Benefit
        {
            get { return @"2nd extra attacks with off-hands at -5 penalty"; }
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
