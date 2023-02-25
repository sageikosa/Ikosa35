using System;
using Uzi.Ikosa.Advancement;
using Uzi.Core;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FighterBonusFeat,
    BaseAttackRequirement(11),
    AbilityRequirement(Abilities.MnemonicCode.Dex, 19),
    ItemSlotRequirement(@"Two Hands", @"Creature must have exactly two holding slots", ItemSlot.HoldingSlot, null, 2, 2),
    FeatChainRequirement(typeof(TwoWeaponFightingFeat)),
    FeatChainRequirement(typeof(ImprovedTwoWeaponFightingFeat)),
    FeatInfo(@"Greater Two-Weapon Fighting")
    ]
    public class GreaterTwoWeaponFightingFeat : FeatBase
    {
        public GreaterTwoWeaponFightingFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
            // NOTE: using same as Multi-Weapon, so these bonuses cannot stack
            _Boost = new Delta(1, typeof(GreaterMultiWeaponFightingFeat));
        }

        private Delta _Boost;

        public override string Benefit
        {
            get { return @"3rd attack with off-hand at -10 penalty"; }
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