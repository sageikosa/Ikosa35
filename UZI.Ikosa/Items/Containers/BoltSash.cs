using System;
using Uzi.Ikosa.Items.Weapons.Ranged;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Items
{
    [
    Serializable,
    ItemInfo(@"Bolt Sash", @"Holds bullets or stones for a sling", @"bolt_sash")
    ]
    public class BoltSash : AmmunitionContainer<CrossbowBolt, CrossbowBase>
    {
        public BoltSash()
            : base(10, @"Bolt Sash", ItemSlot.AmmoSash)
        {
            Initialize();
        }
        public BoltSash(int capacity)
            : base(capacity, @"Bolt Sash", ItemSlot.AmmoSash)
        {
            Initialize();
        }
        private void Initialize()
        {
            BaseWeight = 0.25;
            Price.CorePrice = 0.25m;
            ItemMaterial = LeatherMaterial.Static;
        }

        protected override string ClassIconKey => @"bolt_sash";

        protected override AmmunitionBundle<CrossbowBolt, CrossbowBase> CreateBundle()
            => new CrossbowBoltBundle(@"Bolt Sash");

        public override ActionTime SlottingTime => new ActionTime(Contracts.TimeType.Regular);
        public override bool SlottingProvokes => true;
        public override ActionTime UnslottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool UnslottingProvokes => false;
    }
}
