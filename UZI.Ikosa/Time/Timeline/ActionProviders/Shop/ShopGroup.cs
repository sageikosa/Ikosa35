using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class ShopGroup : AdjunctGroup, IActionSource
    {
        public ShopGroup(object source)
            : base(source)
        {
        }

        public Merchant Merchant => Members.OfType<Merchant>().FirstOrDefault();
        public List<Customer> Customers => Members.OfType<Customer>().ToList();

        public override void ValidateGroup()
            => this.ValidateOneToManyPlanarGroup();

        public IVolatileValue ActionClassLevel
            => new Deltable(1);
    }
}
