using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Tactical
{
    /// <summary>
    /// If all actors that would prompt for turn tracking are in this group, turn tracking won't be prompted.
    /// </summary>
    [Serializable]
    public class StandDownGroup : AdjunctGroup
    {
        /// <summary>
        /// If all actors that would prompt for turn tracking are in this group, turn tracking won't be prompted.
        /// </summary>
        public StandDownGroup(string name)
            : base(typeof(StandDownGroup))
        {
            _Name = name;
        }

        private readonly string _Name;

        public string Name => _Name;

        public IEnumerable<StandDownGroupParticipant> StandDownGroupParticipants
            => Members.OfType<StandDownGroupParticipant>();

        public bool IsInGroup(Guid id)
            => StandDownGroupParticipants.Any(_sdgp => _sdgp.Anchor.ID == id);

        public override void ValidateGroup() { }
    }
}
