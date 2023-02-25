using System;
using Uzi.Ikosa.Items.Weapons.Ranged;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Items
{
    [
    Serializable,
    ItemInfo(@"Sling Ammo Bag", @"Holds bullets or stones for a sling", @"sling_bag")
    ]
    public class SlingBag : AmmunitionContainer<SlingAmmo, Sling>
    {
        public SlingBag()
            : base(10, @"Sling Bag", ItemSlot.Pouch)
        {
            Initialize();
        }
        public SlingBag(int capacity)
            : base(capacity, @"Sling Bag", ItemSlot.Pouch)
        {
            Initialize();
        }
        private void Initialize()
        {
            BaseWeight = 0.25;
            Price.CorePrice = 0.25m;
            ItemMaterial = LeatherMaterial.Static;
        }

        protected override AmmunitionBundle<SlingAmmo, Sling> CreateBundle()
            => new SlingAmmoBundle(@"Sling Bag");

        protected override string ClassIconKey => @"sling_bag";

        public override ActionTime SlottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool SlottingProvokes => false;
        public override ActionTime UnslottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool UnslottingProvokes => false;
    }
}
