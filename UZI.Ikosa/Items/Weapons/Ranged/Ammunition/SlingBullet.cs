using System;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    [Serializable]
    [ItemInfo(@"Sling Bullet", @"1d4 bludgeoning", @"bullet")]
    public class SlingBullet : SlingAmmo
    {
        public SlingBullet()
            : base(@"1d4")
        {
            Name = @"Sling Bullet";
        }

        protected override ItemBase SetupItem()
        {
            var _item = new ItemBase(@"Sling Bullet", Size.Fine)
            {
                ItemMaterial = Uzi.Ikosa.Items.Materials.StoneMaterial.Static
            };
            _item.Price.CorePrice = 0.01m;
            _item.MaxStructurePoints.BaseValue = 30;
            _item.BaseWeight = 0.5d;
            return _item;
        }

        public override object Clone()
        {
            var _clone = new SlingBullet();
            _clone.ItemSizer.ExpectedCreatureSize = ItemSizer.ExpectedCreatureSize;
            _clone.CopyAdjuncts(this);
            return _clone;
        }

        protected override string ClassIconKey { get { return @"bullet"; } }
    }
}
