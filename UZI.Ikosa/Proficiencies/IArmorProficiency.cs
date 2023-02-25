using Uzi.Ikosa.Items.Armor;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa
{
    public interface IArmorProficiency
    {
        bool IsProficientWith(ArmorProficiencyType profType, int powerLevel);
        bool IsProficientWith(ArmorBase armor, int powerLevel);
        string Description { get; }
    }
}
