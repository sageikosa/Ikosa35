using System;
using Uzi.Ikosa.Advancement;
using Uzi.Core;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    AbilityRequirement(Abilities.MnemonicCode.Dex, 15),
    ItemSlotRequirement(@"Two Hands", @"Creature must have exactly two holding slots", ItemSlot.HoldingSlot, null, 2, 2),
    FighterBonusFeat,
    FeatInfo(@"Two-Weapon Fighting")
    ]
    public class TwoWeaponFightingFeat : FeatBase
    {
        public TwoWeaponFightingFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
            _MainDelta = new Delta(2, typeof(TwoWeaponFightingFeat), @"Two-Weapon Fighting Feat");
            _OffDelta = new Delta(6, typeof(TwoWeaponFightingFeat), @"Two-Weapon Fighting Feat");
        }

        private Delta _MainDelta;
        private Delta _OffDelta;

        public override string Benefit { get { return @"Reduce two-weapon fighting penalty by 2, and off-hand penalty by 6"; } }

        protected override void OnActivate()
        {
            Creature.MultiWeaponDelta.MainHandPenalty.Deltas.Add(_MainDelta);
            Creature.MultiWeaponDelta.OffHandPenalty.Deltas.Add(_OffDelta);
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            _MainDelta.DoTerminate();
            _OffDelta.DoTerminate();
            base.OnDeactivate();
        }
    }
}