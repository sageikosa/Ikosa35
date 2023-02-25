using System;
using System.Collections.Generic;
using Uzi.Core;
using System.Linq;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    [Serializable]
    [ItemInfo(@"Crossbow Bolt", @"1d10, 1d8, or 1d4 piercing damage by crossbow type", @"bolt")]
    public class CrossbowBolt : AmmunitionBoundBase<CrossbowBase>
    {
        public CrossbowBolt() :
            base(@"1d8", DamageType.Piercing, 19, new DeltableQualifiedDelta(2, @"Multipler", typeof(WeaponHead)), Uzi.Ikosa.Items.Materials.SteelMaterial.Static)
        {
            Name = @"Crossbow Bolt";
        }

        protected override ItemBase SetupItem()
        {
            var _item = new ItemBase(@"Bolt", Size.Miniature);
            _item.Price.CorePrice = 0.1m;
            _item.BaseWeight = 0.1d;
            return _item;
        }

        public override object Clone()
        {
            var _clone = new CrossbowBolt();
            _clone.ItemSizer.ExpectedCreatureSize = ItemSizer.ExpectedCreatureSize;
            _clone.CopyAdjuncts(this);
            return _clone;
        }

        public override IEnumerable<DamageRollPrerequisite> BaseDamageRollers(Interaction attack, string keyFix, int minGroup)
        {
            // filters out bolt's direct damage roll string and enhancement
            foreach (var _roller in from _r in DeepBaseDamageRollers(attack, keyFix, minGroup)
                                    where ((_r.Source as Type) != typeof(DeltableQualifiedDelta)) && (_r.Source != this)
                                    select _r)
            {
                yield return _roller;
            }
            yield break;
        }

        protected override string ClassIconKey
            => @"bolt";

        public override IAmmunitionBundle ToAmmunitionBundle(string name)
        {
            var _bundle = new CrossbowBoltBundle(name);
            _bundle.Merge((this, 1));
            return _bundle;
        }
    }
}
