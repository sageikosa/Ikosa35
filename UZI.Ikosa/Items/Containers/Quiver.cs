using System;
using Uzi.Ikosa.Items.Weapons.Ranged;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Items
{
    [
    Serializable,
    ItemInfo(@"Quiver", @"Holds arrows for a bow", @"quiver")
    ]
    public class Quiver : AmmunitionContainer<Arrow, BowBase>
    {
        public Quiver()
            : base(20, @"Quiver", ItemSlot.LargeWieldMount)
        {
            Initialize();
        }
        public Quiver(int capacity)
            : base(capacity, @"Quiver", ItemSlot.LargeWieldMount)
        {
            Initialize();
        }
        private void Initialize()
        {
            BaseWeight = 1;
            Price.CorePrice = 1;
            MaxStructurePoints.BaseValue = 2;
            ItemMaterial = LeatherMaterial.Static;
        }

        protected override string ClassIconKey => @"quiver";

        protected override AmmunitionBundle<Arrow, BowBase> CreateBundle()
            => new ArrowBundle(@"Quiver");

        public override ActionTime SlottingTime => new ActionTime(Contracts.TimeType.Regular);
        public override bool SlottingProvokes => true;
        public override ActionTime UnslottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool UnslottingProvokes => false;
    }
}
