using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class Summoner : ActorController<SummoningGroup>
    {
        public Summoner(object source, SummoningGroup controlGroup) 
            : base(source, controlGroup)
        {
        }

        public override object Clone()
        {
            return new Summoner(Source, ControlGroup);
        }
    }
}
