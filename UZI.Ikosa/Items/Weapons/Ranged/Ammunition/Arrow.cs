using System;
using System.Collections.Generic;
using Uzi.Core;
using System.Linq;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    [Serializable]
    [ItemInfo(@"Arrow", @"1d8 or 1d6 piercing damage by bow type", @"arrow")]
    public class Arrow : AmmunitionBoundBase<BowBase>
    {
        public Arrow() :
            base(@"1d8", DamageType.Piercing, 20, new DeltableQualifiedDelta(3, @"Multipler", typeof(WeaponHead)), Uzi.Ikosa.Items.Materials.SteelMaterial.Static)
        {
            Name = @"Arrow";
        }

        protected override ItemBase SetupItem()
        {
            var _item = new ItemBase(@"Arrow", Size.Miniature);
            _item.Price.CorePrice = 0.05m;
            _item.BaseWeight = 0.15d;
            return _item;
        }

        public override object Clone()
        {
            var _clone = new Arrow();
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
            => @"arrow";

        public override IAmmunitionBundle ToAmmunitionBundle(string name)
        {
            var _bundle = new ArrowBundle(name);
            _bundle.Merge((this, 1));
            return _bundle;
        }
    }
}
