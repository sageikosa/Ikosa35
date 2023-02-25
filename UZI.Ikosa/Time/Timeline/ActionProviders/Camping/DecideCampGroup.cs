using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class DecideCampGroup : AdjunctGroup, IActionSource
    {
        public DecideCampGroup(TeamGroup source)
            : base(source)
        {
        }

        public TeamGroup TeamGroup => Source as TeamGroup;
        public DecideCampSite DecideCampSite => Members.OfType<DecideCampSite>().FirstOrDefault();
        public List<DecideCampMember> CampMembers => Members.OfType<DecideCampMember>().ToList();

        public bool HasVoteSucceeded
            => CampMembers.Count >= (int)Math.Ceiling(TeamGroup.TeamMembers.Where(_tm => _tm.IsPrimary).Count() / 2m);

        public IVolatileValue ActionClassLevel => new Deltable(1);

        protected override void OnMemberRemoved(GroupMemberAdjunct member)
        {
            if ((member != DecideCampSite) && !CampMembers.Any())
            {
                // if no one deciding, there is no decision to be made
                DecideCampSite?.Eject();
            }
            base.OnMemberRemoved(member);
        }

        public override void ValidateGroup()
            => this.ValidateOneToManyPlanarGroup();
    }
}
