using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Armor
{
    [SourceInfo(@"Armor")]
    public interface IArmor : IProtectorItem
    {
        ArmorProficiencyType ProficiencyType { get; }
        ConstDeltable MaxDexterityBonus { get; }
    }
}
