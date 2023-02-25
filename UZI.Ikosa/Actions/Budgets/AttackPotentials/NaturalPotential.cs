using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class NaturalPotential : ISlotAttackPotential
    {
        public NaturalPotential(NaturalWeapon weapon)
        {
            _Weapon = weapon;
            _Slots = _Weapon.AllSlots.ToArray();
            _Used = false;
        }

        #region state
        private CoreActionBudget _Budget;
        private ItemSlot[] _Slots;
        private NaturalWeapon _Weapon;
        private bool _Used;
        #endregion

        #region IAttackPotential Members

        public bool CanUse(AttackActionBase attack)
        {
            // not used, the head matches our natural weapon head, and nothing blocks our weapon or potential
            return !_Used
                && _Weapon.MainHead.Equals(attack.WeaponHead)
                && !_Budget.BudgetItems.Items.OfType<IAttackPotential>().Any(_bi => _bi.BlocksUse(attack, this));
        }

        public bool RegisterUse(AttackActionBase attack)
        {
            if (CanUse(attack))
            {
                _Used = true;
                return true;
            }
            return false;
        }

        public bool BlocksUse(AttackActionBase attack, IAttackPotential potential)
        {
            if (IsUsed)
            {
                // natural was used
                if (attack.WeaponHead?.AssignedSlots.Any(_s => _Slots.Contains(_s)) ?? false)
                {
                    // weapon has a head trying to use our slots
                    return true;
                }

                if ((potential as ISlotAttackPotential)?.ItemSlots.Any(_s => _Slots.Contains(_s)) ?? false)
                {
                    // potential also is bound to one of the same slots
                    return true;
                }
            }
            return false;
        }

        public bool IsUsed
            => _Used;

        public IEnumerable<ItemSlot> ItemSlots => _Slots.Select(_s => _s);

        public IDelta Delta => null;

        #endregion

        public object Source => _Weapon;

        #region IBudgetItem Members
        public void Added(CoreActionBudget budget)
        {
            _Budget = budget;
        }

        public void Removed() { }
        public string Name => $@"Natural Weapon ({_Weapon.SourceName()})";
        public string Description => @"One attack with natural weapon";
        #endregion

        #region IResetBudgetItem Members

        public bool Reset()
        {
            // must eject
            return true;
        }

        #endregion
    }
}
