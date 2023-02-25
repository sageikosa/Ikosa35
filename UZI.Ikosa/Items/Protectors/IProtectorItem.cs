using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
namespace Uzi.Ikosa.Items
{
    /// <summary>
    /// Common interface elements for IArmor and IShield
    /// </summary>
    public interface IProtectorItem : IEnhancementTracker, IModifier, ISlottedItem
    {
        Deltable ProtectionBonus { get; }
        ConstDeltable ArcaneSpellFailureChance { get; }
        ConstDeltable CheckPenalty { get; }
        decimal EnhancementCost { get; }
    }
}
