using System;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>ItemSlot as a Trait</summary>
    [Serializable]
    public class ItemSlotTrait : TraitEffect
    {
        /// <summary>ItemSlot as a Trait</summary>
        public ItemSlotTrait(ITraitSource traitSource, string slotType, string subType, bool isMagical, bool allowUnslot)
            : base(traitSource)
        {
            _Slot = new ItemSlot(traitSource, slotType, subType, isMagical, allowUnslot);
        }

        #region state
        private ItemSlot _Slot;
        #endregion

        public string SlotType => _Slot.SlotType;
        public string SubType => _Slot.SubType;
        public bool IsMagical => _Slot.MagicalSlot;
        public bool AllowUnslot => _Slot.AllowUnSlotAction;

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            (Anchor as Creature).Body.ItemSlots.Add(_Slot);
        }

        protected override void OnDeactivate(object source)
        {
            (Anchor as Creature).Body.ItemSlots.Remove(_Slot);
            base.OnDeactivate(source);
        }

        public override object Clone()
            => new ItemSlotTrait(TraitSource, SlotType, SubType, IsMagical, AllowUnslot);

        public override TraitEffect Clone(ITraitSource traitSource)
            => new ItemSlotTrait(traitSource, SlotType, SubType, IsMagical, AllowUnslot);
    }
}
