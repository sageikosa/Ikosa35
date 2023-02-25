using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Ranged;

namespace Uzi.Ikosa.UI
{
    public class PresentableAmmunitionContainer<Ammo, Projector, Container> : PresentableThingVM<Container>
        where Ammo : AmmunitionBoundBase<Projector>
        where Projector : WeaponBase, IProjectileWeapon
        where Container: AmmunitionContainer<Ammo, Projector>
    {
    }

    public class QuiverVM : PresentableAmmunitionContainer<Arrow, BowBase, Quiver>
    {
    }

    public class BoltSashVM : PresentableAmmunitionContainer<CrossbowBolt, CrossbowBase, BoltSash>
    {
    }

    public class SlingBagVM : PresentableAmmunitionContainer<SlingAmmo, Sling, SlingBag>
    {
    }

    public class ShurikenPouchVM : PresentableAmmunitionContainer<Shuriken, ShurikenGrip, ShurikenPouch>
    {
    }
}
