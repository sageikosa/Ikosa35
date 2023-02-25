using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class WorkshopGroup : AdjunctGroup
    {
        public WorkshopGroup(object source) 
            : base(source)
        {
        }

        public WorkshopFacility WorkshopFacility => Members.OfType<WorkshopFacility>().FirstOrDefault();
        public List<CraftsPerson> CraftsPersons => Members.OfType<CraftsPerson>().ToList();

        public override void ValidateGroup()
            => this.ValidateOneToManyPlanarGroup();
    }
}
