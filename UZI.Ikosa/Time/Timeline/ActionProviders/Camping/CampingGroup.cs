using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class CampingGroup : AdjunctGroup, IActionSource
    {
        public CampingGroup(object source) 
            : base(source)
        {
        }

        public CampSite CampSite => Members.OfType<CampSite>().FirstOrDefault();
        public List<CampMember> CampMembers => Members.OfType<CampMember>().ToList();

        protected override void OnMemberRemoved(GroupMemberAdjunct member)
        {
            if ((member != CampSite) && !CampMembers.Any())
            {
                CampSite?.Eject();
            }
            base.OnMemberRemoved(member);
        }

        public IVolatileValue ActionClassLevel
            => new Deltable(1);

        public override void ValidateGroup()
            => this.ValidateOneToManyPlanarGroup();
    }
}
