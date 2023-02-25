using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class CoveringSlot : ItemSlot
    {
        public CoveringSlot(CoveringWrapper target)
            : base(target, ItemSlot.Covering, false)
        {
        }

        public CoveringWrapper CoveringWrapper 
            => Source as CoveringWrapper;

        public override bool AllowUnSlotAction 
            => CoveringWrapper.CoverSource is ICanCoverAsSlot;

        public override ItemSlot Clone(object source)
            => new CoveringSlot(Source as CoveringWrapper);
    }
}
