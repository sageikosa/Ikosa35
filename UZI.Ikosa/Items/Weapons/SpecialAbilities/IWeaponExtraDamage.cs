using System.Linq;
using Uzi.Ikosa.Actions;
using Uzi.Core;
using System.Collections.Generic;

namespace Uzi.Ikosa.Items.Weapons
{
    public interface IWeaponExtraDamage
    {
        IEnumerable<DamageRollPrerequisite> DamageRollers(Interaction workSet);
        bool PoweredUp { get; set; }
    }

    public static class WeaponExtraDamageHelper
    {
        public static IEnumerable<IWeaponExtraDamage> ExtraDamages<Source>(this Source dmgSource)
            where Source : IDamageSource, IAdjunctable
            => from _eff in dmgSource.Adjuncts.OfType<IWeaponExtraDamage>()
               select _eff as IWeaponExtraDamage;
    }
}
