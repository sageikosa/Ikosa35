using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Weapons;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FighterBonusFeat,
    FeatInfo(@"Improved Unarmed Strike")
    ]
    public class ImprovedUnarmedStrikeFeat : FeatBase, IInteractHandler
    {
        public ImprovedUnarmedStrikeFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit
        {
            get
            {
                return
                    @"Do not provoke opportunistic attacks for unarmed strike.  " +
                    @"Can deal lethal damage.";
            }
        }

        private void RefreshUnarmedSlot()
        {
            // forces the body to regenerate an unarmed strike
            var _slot = this.Creature.Body.ItemSlots[ItemSlot.UnarmedSlot];
            if (_slot != null)
            {
                var _item = _slot.SlottedItem;
                if (_item != null)
                    _item.ClearSlots();
            }
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            this.Creature.AddIInteractHandler(this);
            RefreshUnarmedSlot();
        }

        protected override void OnDeactivate()
        {
            this.Creature.RemoveIInteractHandler(this);
            RefreshUnarmedSlot();
            base.OnDeactivate();
        }

        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
            if (workSet != null)
            {
                var _critter = workSet.Actor as Creature;
                if (_critter != null)
                {
                    var _improved = new ImprovedUnarmedWeapon();
                    _improved.Possessor = _critter;
                    workSet.Feedback.Add(new GetUnarmedWeaponFeedback(this) { Weapon = _improved });
                }
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(GetUnarmedWeapon);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            // slightly better than the default
            return (existingHandler is GetUnarmedWeaponHandler);
        }

        #endregion
    }
}
