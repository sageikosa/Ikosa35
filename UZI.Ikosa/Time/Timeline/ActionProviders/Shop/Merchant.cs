using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class Merchant : GroupMasterAdjunct
    {
        public Merchant(ShopGroup shop)
           : base(shop, shop)
        {
        }

        public override object Clone()
            => new Merchant(ShopGroup);

        public ShopGroup ShopGroup => Group as ShopGroup;
    }
}
