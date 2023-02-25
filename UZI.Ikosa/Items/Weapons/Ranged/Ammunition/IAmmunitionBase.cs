using System;
using System.Collections.Generic;
using Uzi.Core;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    public interface IAmmunitionBase : IItemBase, ICloneable, IMonitorChange<DeltaValue>, IWeaponHead
    {
        Type GetProjectileWeaponType();
        IAmmunitionBundle ToAmmunitionBundle(string name);
        IEnumerable<Guid> GetInfoIDs(CoreActor actor);
        bool IsInfoMatch(CoreActor actor, HashSet<Guid> infoIDs);
    }
}