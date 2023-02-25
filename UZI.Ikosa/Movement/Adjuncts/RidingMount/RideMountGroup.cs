using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class RideMountGroup : AdjunctGroup
    {
        public RideMountGroup(object source)
            : base(source)
        {
        }

        public RidingMount RidingMount 
            => Members.OfType<RidingMount>().FirstOrDefault();

        public IEnumerable<Rider> Riders
            => Members.OfType<Rider>();

        public override void ValidateGroup()
            => this.ValidateOneToManyPlanarGroup();
    }
}
