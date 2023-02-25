using System;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    [Serializable]
    [ItemInfo(@"Sling Stone", @"1d3 bludgeoning", @"sling_stone")]
    public class SlingStone : SlingAmmo
    {
        public SlingStone()
            :base(@"1d3")
        {
            // TODO: -1 penalty on attack rolls
            Name = @"Sling Stone";
        }

        protected override ItemBase SetupItem()
        {
            var _item = new ItemBase(@"Sling Stone", Size.Fine)
            {
                ItemMaterial = Materials.StoneMaterial.Static
            };
            _item.Price.CorePrice = 0m;
            _item.MaxStructurePoints.BaseValue = 20;
            _item.BaseWeight = 0.5d;
            return _item;
        }

        public override object Clone()
        {
            var _clone = new SlingStone();
            _clone.ItemSizer.ExpectedCreatureSize = ItemSizer.ExpectedCreatureSize;
            _clone.CopyAdjuncts(this);
            return _clone;
        }

        protected override string ClassIconKey { get { return @"sling_stone"; } }
    }
}
