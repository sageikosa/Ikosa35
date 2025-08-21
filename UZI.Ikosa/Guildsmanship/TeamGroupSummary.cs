using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Packaging;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class TeamGroupSummary : IModuleElement, ICorePart
    {
        private readonly Guid _ID;
        private readonly Description _Description;
        private readonly Dictionary<Guid, TeamAttitude> _Attitudes;

        /// <summary>Define a new team without an existing team group</summary>
        public TeamGroupSummary(string teamName)
        {
            _ID = Guid.NewGuid();
            _Description = new Description(teamName);
            _Attitudes = [];
        }

        /// <summary>Define a team from an existing team group</summary>
        public TeamGroupSummary(TeamGroup teamGroup)
        {
            _ID = teamGroup.ID;
            _Description = new Description(teamGroup.Name);
            _Attitudes = [];
        }

        public Guid ID => _ID;
        public string TeamName => _Description?.Message ?? string.Empty;
        public Description Description => _Description;
        public Dictionary<Guid, TeamAttitude> Attitudes => _Attitudes;

        public string Name => TeamName;
        public IEnumerable<ICorePart> Relationships => Enumerable.Empty<ICorePart>();
        public string TypeName => typeof(TeamGroupSummary).FullName;
    }
}
