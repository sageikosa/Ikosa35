using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    public interface IAmmunitionBundle : IControlChange<int>, IItemBase, IActionProvider
    {
        Type AmmunitionType { get; }
        IAmmunitionBase CreateAmmo();
        (IAmmunitionBase ammo, int count) ExtractAmmo(IAmmunitionBase ammo);
        (IAmmunitionBase ammo, int count) MergeAmmo((IAmmunitionBase ammo, int count) ammoSet);
        IEnumerable<AmmoEditSet> AmmoSets { get; }
        void SetCount(IAmmunitionBase ammo, int count);
        void SyncSets();
        int? Capacity { get; }
        int Count { get; }
    }

    public interface IAmmunitionTypedBundle<AmmoType, ProjectileWeapon> : IAmmunitionBundle, IActionSource
        where AmmoType : AmmunitionBoundBase<ProjectileWeapon>
        where ProjectileWeapon : WeaponBase, IProjectileWeapon
    {
        IEnumerable<AmmoInfo> AmmunitionInfos(CoreActor actor);
        void Use(AmmoType ammunitionBase);
        AmmoType GetAmmunition(CoreActor actor, HashSet<Guid> infoIDs);

        // used to reload primarily
        (AmmoType ammunition, int count) Merge((AmmoType ammunition, int count) ammoSet);

        AmmunitionBundle<AmmoType, ProjectileWeapon> Extract
            (CoreActor actor, params (HashSet<Guid> infoIDs, int count)[] exemplars);

        AmmunitionBundle<AmmoType, ProjectileWeapon> Merge(AmmunitionBundle<AmmoType, ProjectileWeapon> ammoGroup);
    }
}
