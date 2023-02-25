using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class WorkshopFacility : GroupMasterAdjunct
    {
        public WorkshopFacility(WorkshopGroup workshop)
            : base(workshop, workshop)
        {
        }

        public override object Clone()
            => new WorkshopFacility(WorkshopGroup);

        public WorkshopGroup WorkshopGroup => Group as WorkshopGroup;
    }
}
