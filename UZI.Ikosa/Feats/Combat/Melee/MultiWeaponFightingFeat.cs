using System;
using Uzi.Core;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    AbilityRequirement(Abilities.MnemonicCode.Dex, 13),
    ItemSlotRequirement(@"Three or More Hands", @"Creature must have three or more holding slots", ItemSlot.HoldingSlot, null, 3, int.MaxValue),
    FighterBonusFeat,
    FeatInfo(@"Multi-Weapon Fighting")
    ]
    public class MultiWeaponFightingFeat : FeatBase
    {
        public MultiWeaponFightingFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
            _MainDelta = new Delta(2, typeof(MultiWeaponFightingFeat), @"Multi Weapon Fighting Feat");
            _OffDelta = new Delta(6, typeof(MultiWeaponFightingFeat), @"Multi Weapon Fighting Feat");
        }

        private Delta _MainDelta;
        private Delta _OffDelta;

        public override string Benefit { get { return @"Reduce multi-weapon fighting penalty by 2, and off-hand penalty by 6"; } }

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
