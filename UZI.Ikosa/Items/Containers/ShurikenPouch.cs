using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Items.Weapons.Ranged;

namespace Uzi.Ikosa.Items
{
    [
        Serializable,
        ItemInfo(@"Shuriken Pouch", @"Holds shuriken to throw", @"shuriken_pouch")
    ]
    public class ShurikenPouch : AmmunitionContainer<Shuriken, ShurikenGrip>
    {
        public ShurikenPouch()
            : base(20, @"Shuriken Pouch", ItemSlot.Pouch)
        {
            Initialize();
        }
        public ShurikenPouch(int capacity)
            : base(capacity, @"Shuriken Pouch", ItemSlot.Pouch)
        {
            Initialize();
        }
        private void Initialize()
        {
            BaseWeight = 0.5;
            Price.CorePrice = 1m;
            ItemMaterial = LeatherMaterial.Static;
        }

        protected override AmmunitionBundle<Shuriken, ShurikenGrip> CreateBundle()
            => new ShurikenBundle(@"Shuriken Pouch");

        protected override string ClassIconKey => @"shuriken_pouch";

        public override ActionTime SlottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool SlottingProvokes => false;
        public override ActionTime UnslottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool UnslottingProvokes => false;

        public override IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            // TODO: throw shuriken from pouch???
            yield return new ArmShuriken(this, @"101");
            foreach (var _act in base.GetActions(budget))
                yield return _act;
            yield break;
        }
    }
}
