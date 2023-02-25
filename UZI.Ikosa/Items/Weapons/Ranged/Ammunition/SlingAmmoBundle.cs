using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    [Serializable]
    public class SlingAmmoBundle : AmmunitionBundle<SlingAmmo, Sling>
    {
        public SlingAmmoBundle(string name)
            : base(name, Size.Fine)
        {
        }

        public override IAmmunitionBase CreateAmmo()
        {
            var _bullet = new SlingBullet();
            _bullet.ItemSizer.ExpectedCreatureSize = ItemSizer.ExpectedCreatureSize;
            return _bullet;
        }
    }
}
