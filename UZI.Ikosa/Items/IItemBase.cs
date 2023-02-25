using Uzi.Ikosa.Items.Materials;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;
using Uzi.Packaging;

namespace Uzi.Ikosa.Items
{
    public interface IItemBase : ICoreItem, ISizable, IArmorRating, IStructureDamage, IProvideSaves
    {
        Price Price { get; }
        Creature CreaturePossessor { get; }
        Deltable Hardness { get; }
        Material ItemMaterial { get; set; }
        Deltable MaxStructurePoints { get; }
        ItemSizer ItemSizer { get; }

        /// <summary>True if local to the creature, and can be used in a local selection list</summary>
        bool IsLocal(Locator locator);

        /// <summary>Base weight for an item when at its base size</summary>
        double BaseWeight { get; set; }
    }
}
