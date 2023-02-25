using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.Actions
{
    public interface IAttackPotential : IResetBudgetItem
    {
        /// <summary>Returns true if the budget allows the use of the weapon head</summary>
        bool CanUse(AttackActionBase attack);

        /// <summary>Returns true if the budget registers the use of the weapon head</summary>
        bool RegisterUse(AttackActionBase attack);

        /// <summary>Indicates the budget item has been used</summary>
        bool IsUsed { get; }

        /// <summary>Returns true if the budget prevents the use of the attack for the potential</summary>
        bool BlocksUse(AttackActionBase attack, IAttackPotential potential);

        /// <summary>Delta (if any) to apply when using this attack potential</summary>
        IDelta Delta { get; }
    }

    public interface ISlotAttackPotential : IAttackPotential
    {
        /// <summary>List of item slots bound to this potential (if any)</summary>
        IEnumerable<ItemSlot> ItemSlots { get; }
    }
}
