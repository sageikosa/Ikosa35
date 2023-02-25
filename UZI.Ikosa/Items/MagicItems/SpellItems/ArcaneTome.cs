using System;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class ArcaneTome: SpellBook // TODO: lockable
    {
        public ArcaneTome()
            : base(@"Arcane Tome", 1000, 0)
        {
            Price.BaseItemExtraPrice = 12500;
            BaseWeight = 1d;
            ItemMaterial = IronMaterial.Static; // book binding
            MaxStructurePoints.BaseValue = 5;
            // TODO: ... break DC
        }

        protected override void SetAugmentationPrice()
        {
        }
    }
}
