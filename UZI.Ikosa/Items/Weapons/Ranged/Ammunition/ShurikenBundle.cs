using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    [Serializable]
    public class ShurikenBundle : AmmunitionBundle<Shuriken, ShurikenGrip>
    {
        public ShurikenBundle(string name)
            : base(name, Size.Fine)
        {
        }

        public override IAmmunitionBase CreateAmmo()
        {
            var _shuriken = new Shuriken();
            _shuriken.ItemSizer.ExpectedCreatureSize = ItemSizer.ExpectedCreatureSize;
            return _shuriken;
        }

        public override IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            // TODO: throw shuriken from bundle???
            yield return new ArmShuriken(this, @"101");
            foreach (var _act in base.GetActions(budget))
            {
                yield return _act;
            }

            yield break;
        }
    }
}
