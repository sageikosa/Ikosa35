using System;
using Uzi.Core;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Creatures.BodyType
{
    [Serializable]
    public class BodyDock : LinkableDock<Body>, ICreatureBound
    {
        public BodyDock(Creature creature)
            : base(@"Body")
        {
            Creature = creature;

            // Body bound modifiers
            var _ptr = new Delta(0, typeof(Deltas.Size));
            _SizeDelta = new DeltaPtr(_ptr);
            _OpposedDelta = new DeltaPtr(_ptr);
            _HideDelta = new DeltaPtr(_ptr);
            _NaturalArmor = new DeltaPtr(_ptr);
        }

        public Creature Creature { get; private set; }
        public Body Body { get { return Link; } }

        protected override void OnPreLink(Body newVal)
        {
            if (Body != null)
            {
                // unequip all items associated with items slots (still) sourced on the body                
                foreach (ItemSlot _slot in Body.ItemSlots.AllSlots)
                {
                    if (_slot.Source.Equals(Body))
                    {
                        _slot.SlottedItem?.ClearSlots();
                    }
                }

                // clear body watching for item possession changes
                Creature.Possessions.RemoveChangeMonitor(Body.ItemSlots);
            }
            base.OnPreLink(newVal);
        }

        protected override void OnLink()
        {
            // Changing
            _SizeDelta.CurrentModifier = Body.Sizer.SizeModifier;
            _OpposedDelta.CurrentModifier = Body.Sizer.OpposedModifier;
            _HideDelta.CurrentModifier = Body.Sizer.HideModifier;
            _NaturalArmor.CurrentModifier = Body.NaturalArmor;
            Creature.Possessions.AddChangeMonitor(Body.ItemSlots);
            base.OnLink();
        }

        #region protected data
        protected DeltaPtr _SizeDelta;
        protected DeltaPtr _OpposedDelta;
        protected DeltaPtr _HideDelta;
        protected DeltaPtr _NaturalArmor;
        #endregion

        /// <summary>Used for Armor Ratings and Standard Attack modes</summary>
        public IModifier SizeModifier => _SizeDelta; 

        /// <summary>Used for Opposed Checks</summary>
        public IModifier OpposedModifier => _OpposedDelta; 

        /// <summary>Used for Hide checks</summary>
        public IModifier HideModifier => _HideDelta; 

        /// <summary>Natural Armor Rating</summary>
        public IModifier NaturalArmorModifier => _NaturalArmor; 
    }
}
