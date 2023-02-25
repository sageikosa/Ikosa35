using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Items.Shields
{
    [ItemInfo(@"Shield, Tower", @"AR:+4 Check:-10 SpellFail:50%", @"tower_shield"), Serializable]
    public class TowerShield: ShieldBase, IActionFilter
    {
        #region Construction
        public TowerShield()
            : base(@"Shield, Tower", true, 4, -10, 50, false)
        {
            Init(30m, 45d, Materials.WoodMaterial.Static, 20);
            Sizer.NaturalSize = Size.Medium;
        }
        #endregion

        public virtual int MaxDexterityToArmorRating { get { return 2; } }
        public override int OpposedDelta { get { return 4; } }

        protected override void OnSetItemSlot()
        {
            base.OnSetItemSlot();
            if (MainSlot != null)
            {
                CreaturePossessor.MaxDexterityToARBonus.SetValue(typeof(TowerShield), MaxDexterityToArmorRating);
            }
        }

        protected override void OnClearSlots(ItemSlot slotA, ItemSlot slotB)
        {
            base.OnClearSlots(slotA, slotB);
            if (slotA != null)
            {
                CreaturePossessor.MaxDexterityToARBonus.ClearValue(typeof(TowerShield));
            }
        }

        #region IActionProvider Members
        public override IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            var _budget = budget as LocalActionBudget;
            // TODO: budget...

            // TODO: tower defense (on or off once per round): provide total cover, as free action, but only if action budget is unused
            // TODO: suppress shield bash by overriding
            yield break;
        }
        #endregion

        #region IActionFilter Members
        public override bool SuppressAction(object source, CoreActionBudget budget, CoreAction action)
        {
            // TODO: if using as tower defense, suppress attack actions
            return false;
        }
        #endregion

        protected override string ClassIconKey { get { return @"tower_shield"; } }
    }
}
