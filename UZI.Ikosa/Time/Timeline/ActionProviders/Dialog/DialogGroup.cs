using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class DialogGroup : AdjunctGroup
    {
        public DialogGroup(object source)
            : base(source)
        {
        }

        public DialogNPCTarget DialogNPCTarget => Members.OfType<DialogNPCTarget>().FirstOrDefault();
        public List<DialogParticipant> DialogParticipants => Members.OfType<DialogParticipant>().ToList();

        public override void ValidateGroup()
            => this.ValidateOneToManyPlanarGroup();
    }
}
