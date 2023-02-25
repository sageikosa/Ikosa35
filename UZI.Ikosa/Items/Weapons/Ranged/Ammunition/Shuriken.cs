using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    [Serializable]
    [ItemInfo(@"Shuriken", @"Exotic Ranged 1d2 (x2) Piercing (Throw 10')", @"shuriken")]
    public class Shuriken : AmmunitionBoundBase<ShurikenGrip>
    {
        public Shuriken() :
            base(@"1d2", DamageType.Piercing, 20, new DeltableQualifiedDelta(2, @"Multipler", typeof(WeaponHead)), Materials.SteelMaterial.Static)
        {
            Name = @"Shuriken";
        }

        protected override ItemBase SetupItem()
        {
            var _item = new ItemBase(@"Shuriken", Size.Fine);
            _item.Price.CorePrice = 0.2m;
            _item.BaseWeight = 0.1d;
            return _item;
        }

        public override object Clone()
        {
            var _clone = new Shuriken();
            _clone.ItemSizer.ExpectedCreatureSize = ItemSizer.ExpectedCreatureSize;
            _clone.CopyAdjuncts(this);
            return _clone;
        }

        public override IEnumerable<DamageRollPrerequisite> BaseDamageRollers(Interaction attack, string keyFix, int minGroup)
        {
            // filters out arrow's direct damage roll string and enhancement
            foreach (var _roller in from _r in DeepBaseDamageRollers(attack, keyFix, minGroup)
                                    where ((_r.Source as Type) != typeof(DeltableQualifiedDelta)) && (_r.Source != this)
                                    select _r)
            {
                yield return _roller;
            }
            yield break;
        }

        protected override string ClassIconKey
            => @"shuriken";

        public override IAmmunitionBundle ToAmmunitionBundle(string name)
        {
            var _bundle = new ShurikenBundle(name);
            _bundle.Merge((this, 1));
            return _bundle;
        }
    }
}
