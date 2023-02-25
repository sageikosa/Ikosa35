using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Items.Weapons
{
    public interface IWeaponHead: ICoreObject, IAttackSource, IDamageSource, IEnhancementTracker,
        IMonitorChange<Size>, IMonitorChange<DeltaValue>, IActionSource
    {
        IEnumerable<ItemSlot> AssignedSlots { get; }
        IWeapon ContainingWeapon { get; }
        int CriticalLow { get; }
        int CriticalMultiplier { get; }
        string CurrentDamageRollString { get; }
        Material HeadMaterial { get; set; }
        bool IsMainHead { get; }
        bool IsOffHand { get; }
        Lethality LethalitySelection { get; }
        string SizedDamageRollString { get; }
        decimal SpecialCost { get; }

        WeaponHeadInfo ToWeaponHeadInfo(CoreActor actor, bool baseValues);
    }
}