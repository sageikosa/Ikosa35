using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class SummoningGroup : ActorControlGroup
    {
        public SummoningGroup(IPowerActionSource source) 
            : base(source)
        {
        }

        public override void ValidateGroup()
        {
            this.ValidateParticipantsPlanarGroup();
        }

        public Summoner Summoner => ActorController as Summoner;
        public Summoned Summoned => ActorControlled as Summoned;
        public SummoningWindowLink SummoningWindowLink => Members.OfType<SummoningWindowLink>().FirstOrDefault();
    }
}
