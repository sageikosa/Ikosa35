using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    [Serializable]
    public abstract class SlingAmmo : AmmunitionBoundBase<Sling>
    {
        protected SlingAmmo(string damage)
            : base(damage, DamageType.Bludgeoning, 20, new DeltableQualifiedDelta(2, @"Multipler", typeof(WeaponHead)), Materials.StoneMaterial.Static)
        {
        }

        public override IEnumerable<DamageRollPrerequisite> BaseDamageRollers(Interaction attack, string keyFix, int minGroup)
        {
            // filters out sling ammo's enhancement (still uses damage roll string)
            foreach (var _roller in from _r in DeepBaseDamageRollers(attack, keyFix, minGroup)
                                    where (_r.Source as Type) != typeof(DeltableQualifiedDelta)
                                    select _r)
            {
                yield return _roller;
            }
            yield break;
        }

        public override IAmmunitionBundle ToAmmunitionBundle(string name)
        {
            var _bundle = new SlingAmmoBundle(name);
            _bundle.Merge((this, 1));
            return _bundle;
        }
    }
}
