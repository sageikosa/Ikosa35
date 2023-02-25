using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Adjuncts
{
    public static class GroupValidations
    {
        private static PlanarPresence MasterPresence(this AdjunctGroup self)
            => self.Members.OfType<GroupMasterAdjunct>()
            .Select(_m => _m.Anchor as IAdjunctSet)
            .FirstOrDefault()?
            .GetPlanarPresence() ?? PlanarPresence.None;

        private static PlanarPresence MemberPresence(this AdjunctGroup self)
            => self.Members.OfType<GroupMemberAdjunct>()
            .Where(_m => !(_m is GroupMasterAdjunct))
            .Select(_m => _m.Anchor as IAdjunctSet)
            .FirstOrDefault()?
            .GetPlanarPresence() ?? PlanarPresence.None;

        private static IEnumerable<(GroupMemberAdjunct member, PlanarPresence presence)> MemberPresences(this AdjunctGroup self)
            => from _m in self.Members.OfType<GroupMemberAdjunct>()
               where !(_m is GroupMasterAdjunct)
               let _as = _m.Anchor as IAdjunctSet
               where _as != null
               select (_m, _as.GetPlanarPresence());

        public static void ValidateMasteredPlanarLink(this AdjunctGroup self)
        {
            var _planar = self.MasterPresence();
            if (_planar == PlanarPresence.None)
            {
                // only remove this if NULL presence
                self.EjectMembers();
            }
            else
            {
                // eject all members (if any) not with master presence
                // NOTE: could result in empty-set, but the master is still OK
                foreach (var (_member, _presence) in self.MemberPresences()
                    .Where(_g => !_g.presence.HasOverlappingPresence(_planar))
                    .ToList())
                {
                    // get rid of members not with same planar presence
                    _member.Eject();
                }
            }
        }

        public static void ValidateParticipantsPlanarGroup(this AdjunctGroup self)
        {
            if (self.MemberPresences().Select(_mp => _mp.presence).Distinct().Count() != 1)
            {
                self.EjectMembers();
            }
        }

        public static void ValidateOneToOnePlanarGroup(this AdjunctGroup self)
        {
            var _planar = self.MasterPresence();
            if ((_planar == PlanarPresence.None)
                || !self.MemberPresence().HasOverlappingPresence(_planar))
            {
                self.EjectMembers();
            }
        }

        public static void ValidateOneToManyPlanarGroup(this AdjunctGroup self)
        {
            var _planar = self.MasterPresence();
            if (_planar != PlanarPresence.None)
            {
                foreach (var (_member, _presence) in self.MemberPresences()
                    .Where(_g => !_g.presence.HasOverlappingPresence(_planar))
                    .ToList())
                {
                    // get rid of members not with same planar presence
                    _member.Eject();
                }

                if (self.Members.Count() <= 1)
                {
                    // just the master or less left
                    self.EjectMembers();
                }
            }
            else
            {
                self.EjectMembers();
            }
        }
    }
}
