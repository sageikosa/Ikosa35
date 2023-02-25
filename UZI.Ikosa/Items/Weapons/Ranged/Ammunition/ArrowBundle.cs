using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    [Serializable]
    public class ArrowBundle : AmmunitionBundle<Arrow, BowBase>
    {
        public ArrowBundle(string name)
            : base(name, Size.Miniature)
        {
        }

        public override IAmmunitionBase CreateAmmo()
        {
            var _arrow = new Arrow();
            _arrow.ItemSizer.ExpectedCreatureSize = ItemSizer.ExpectedCreatureSize;
            return _arrow;
        }
    }
}
