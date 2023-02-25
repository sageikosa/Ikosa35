using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Ranged;

namespace Uzi.Ikosa.UI
{
    public class PresentableAmmunitionBundle<Ammo, Projector, BType> : PresentableThingVM<BType>
        where Ammo : AmmunitionBoundBase<Projector>
        where Projector : WeaponBase, IProjectileWeapon
        where BType : AmmunitionBundle<Ammo, Projector>
    {
    }

    public class ArrowBundleVM : PresentableAmmunitionBundle<Arrow, BowBase, ArrowBundle>
    {
    }

    public class CrossbowBoltBundleVM : PresentableAmmunitionBundle<CrossbowBolt, CrossbowBase, CrossbowBoltBundle>
    {
    }

    public class SlingAmmoBundleVM : PresentableAmmunitionBundle<SlingAmmo, Sling, SlingAmmoBundle>
    {
    }

    public class ShurikenBundleVM : PresentableAmmunitionBundle<Shuriken, ShurikenGrip, ShurikenBundle>
    {
    }
}
