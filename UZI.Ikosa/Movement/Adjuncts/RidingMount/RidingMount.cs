using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class RidingMount : GroupMasterAdjunct
    {
        public RidingMount(object source, RideMountGroup group)
            : base(source, group)
        {
        }

        public RideMountGroup RideMountGroup => Group as RideMountGroup;

        public override object Clone()
            => new RidingMount(Source, RideMountGroup);
    }
}
