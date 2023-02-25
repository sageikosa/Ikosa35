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
    public class StandDownGroupParticipant : GroupParticipantAdjunct
    {
        /// <summary>
        /// If all actors that would prompt for turn tracking are in this group, turn tracking won't be prompted.
        /// </summary>
        public StandDownGroupParticipant(StandDownGroup group)
            : base(typeof(StandDownGroup), group)
        {
        }

        public StandDownGroup StandDownGroup => Group as StandDownGroup;

        public override object Clone()
            => new StandDownGroupParticipant(StandDownGroup);
    }
}
