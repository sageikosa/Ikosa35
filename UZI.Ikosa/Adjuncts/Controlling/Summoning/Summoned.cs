using System;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>
    /// Summoned creature marker adjunct
    /// </summary>
    [Serializable]
    public class Summoned : ActorControlled<SummoningGroup>
    {
        public Summoned(object source, SummoningGroup summoningGroup)
            : base(source, summoningGroup)
        {
        }

        public override object Clone()
        {
            return new Summoned(Source, ControlGroup);
        }
    }
}
