using System;
using Uzi.Ikosa.Advancement;
using Uzi.Core;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FighterBonusFeat,
    AbilityRequirement(Abilities.MnemonicCode.Dex, 17),
    ItemSlotRequirement(@"Two Hands", @"Creature must have exactly two holding slots", ItemSlot.HoldingSlot, null, 2, 2),
    FeatChainRequirement(typeof(TwoWeaponFightingFeat)),
    FeatInfo(@"Improved Two-Weapon Fighting"),
    BaseAttackRequirement(6)
    ]
    public class ImprovedTwoWeaponFightingFeat : FeatBase
    {
        public ImprovedTwoWeaponFightingFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
            // NOTE: using same as Multi-Weapon, so these bonuses cannot stack
            _Boost = new Delta(1, typeof(ImprovedMultiWeaponFightingFeat));
        }

        private Delta _Boost;

        public override string Benefit
        {
            get { return @"2nd extra attack with off-hand at -5 penalty"; }
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