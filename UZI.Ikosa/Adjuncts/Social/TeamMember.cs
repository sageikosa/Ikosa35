using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Guildsmanship;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class TeamMember : GroupParticipantAdjunct, IActionProvider
    {
        private TeamMember(TeamGroup team)
            : base(typeof(TeamGroup), team)
        {
            _Primary = false;
        }

        #region data
        private bool _Primary;
        #endregion

        public TeamGroup TeamGroup => ParticipantGroup as TeamGroup;
        public Creature Creature => Anchor as Creature;

        #region public bool IsPrimary { get; set; }
        public bool IsPrimary
        {
            get => _Primary;
            set
            {
                if (value)
                {
                    // only one primary
                    _Primary = true;
                    foreach (var _tm in Creature.Adjuncts.OfType<TeamMember>()
                        .Where(_tm => _tm != this && _tm.IsActive))
                    {
                        // all the rest are not
                        _tm._Primary = false;
                        _tm.DoPropertyChanged(nameof(IsPrimary));
                    }
                    DoPropertyChanged(nameof(IsPrimary));
                }
            }
        }
        #endregion

        public override object Clone()
            => new TeamMember(TeamGroup);

        #region OnActivate
        protected override void OnActivate(object source)
        {
            if (Creature.Adjuncts.OfType<TeamMember>()
                .Where(_tm => _tm.IsActive)
                .Count() == 1)
            {
                IsPrimary = true;
            }
            Creature.Actions.Providers.Add(this, this);
            base.OnActivate(source);
        }
        #endregion

        #region OnDeactivate()
        protected override void OnDeactivate(object source)
        {
            Creature.Actions.Providers.Remove(this);
            if (IsPrimary)
            {
                var _fallback = Creature.Adjuncts.OfType<TeamMember>()
                    .Where(_tm => _tm != this && _tm.IsActive)
                    .FirstOrDefault();
                if (_fallback != null)
                    _fallback.IsPrimary = true;
            }
            base.OnDeactivate(source);
        }
        #endregion

        #region public static TeamMember SetTeamMember(Creature critter, string teamName)
        /// <summary>Tries to add the creature to the specified team</summary>
        public static TeamMember SetTeamMember(Creature critter, string teamName)
        {
            // get group (creates if not exists)
            var _group = TeamGroup.GetTeamGroup(critter?.Setting, teamName);
            if (!_group.TeamCreatures.Contains(critter))
            {
                // add if needed
                var _member = new TeamMember(_group);
                return critter.AddAdjunct(_member)
                    ? _member
                    : null;
            }

            // get pre-existing one
            return critter.Adjuncts.OfType<TeamMember>()
                .FirstOrDefault(_tm => _tm.TeamGroup.Name.Equals(teamName, StringComparison.OrdinalIgnoreCase));
        }
        #endregion

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (IsPrimary
                && budget is LocalActionBudget _budget
                && !_budget.IsInitiative)
            {
                yield return new DecideSetupCamp(TeamGroup, new ActionTime(Contracts.TimeType.Total), @"201");
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => new AdjunctInfo($@"Team Member of {TeamGroup.Name}", ID);
    }

    [Serializable]
    public class TeamGroup : AdjunctGroup, IActionSource
    {
        private TeamGroup(string teamName)
            : base(typeof(TeamGroup))
        {
            _Name = teamName;
        }

        private TeamGroup(Guid id, string teamName)
            : base(typeof(TeamGroup), id)
        {
            _Name = teamName;
        }

        private string _Name;

        public string Name { get => _Name; set => _Name = value; }

        public IEnumerable<Creature> TeamCreatures
            => Members.OfType<TeamMember>().Select(_m => _m.Creature);

        public IEnumerable<TeamMember> TeamMembers
            => Members.OfType<TeamMember>().Select(_tm => _tm);

        public IVolatileValue ActionClassLevel
            => new Deltable(1);

        public static TeamGroup GetTeamGroup(CoreSetting setting, string teamName)
            => setting?.ContextSet.AdjunctGroups
            .All()
            .OfType<TeamGroup>()
            .FirstOrDefault(_tg => _tg.Name.Equals(teamName, StringComparison.OrdinalIgnoreCase))
            ?? new TeamGroup(teamName);

        public override void ValidateGroup() { }
    }

    public static class TeamMemberHelper
    {
        /// <summary>All teams in the current setting</summary>
        public static IEnumerable<TeamGroup> GetAvailableTeams(this Creature self)
            => self.Setting?.ContextSet.AdjunctGroups.All().OfType<TeamGroup>()
            ?? new List<TeamGroup>();

        /// <summary>All teams shared with another creature</summary>
        public static IEnumerable<TeamGroup> GetSameTeams(this Creature self, Creature other)
            => self.Adjuncts.OfType<TeamMember>()
            .Where(_t => _t.TeamGroup.TeamCreatures.Contains(other))
            .Select(_t => _t.TeamGroup);

        /// <summary>All teams shared with another creature</summary>
        public static IEnumerable<TeamGroup> GetSameTeams(this Creature self, Guid other)
            => self.Adjuncts.OfType<TeamMember>()
            .Where(_t => _t.TeamGroup.TeamCreatures.Any(_tc => _tc.ID == other))
            .Select(_t => _t.TeamGroup);
    }
}
