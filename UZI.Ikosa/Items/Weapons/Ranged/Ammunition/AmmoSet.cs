using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    [Serializable]
    public class AmmoSet<AmmoType, ProjectileWeapon>
        where AmmoType : AmmunitionBoundBase<ProjectileWeapon>
        where ProjectileWeapon : WeaponBase, IProjectileWeapon
    {
        public AmmoType Ammunition { get; set; }
        public int Count { get; set; }

        public decimal Price => Ammunition.Price.BasePrice * Count;
        public double Weight => Ammunition.Weight * Count;

        /// <summary>Known AmmoInfo for actor with current count</summary>
        public AmmoInfo ToAmmoInfo(CoreActor actor)
        {
            var _info = GetInfoData.GetInfoFeedback(Ammunition, actor) as AmmoInfo;
            if (_info != null)
            {
                _info.Count = Count;
                _info.InfoIDs = new HashSet<Guid>(Ammunition.GetInfoIDs(actor));
            }

            return _info;
        }
    }
}
