using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    [Serializable]
    public class CrossbowBoltBundle : AmmunitionBundle<CrossbowBolt, CrossbowBase>
    {
        public CrossbowBoltBundle(string name)
            : base(name, Size.Miniature)
        {
        }

        public override IAmmunitionBase CreateAmmo()
        {
            var _bolt = new CrossbowBolt();
            _bolt.ItemSizer.ExpectedCreatureSize = ItemSizer.ExpectedCreatureSize;
            return _bolt;
        }
    }
}
