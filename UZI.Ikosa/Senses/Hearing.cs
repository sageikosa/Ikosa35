using System;

namespace Uzi.Ikosa.Senses
{
    [Serializable]
    public class Hearing : SensoryBase
    {
        public Hearing(object source)
            : base(source)
        {
        }

        public override bool UsesHearing { get { return true; } }
        public override bool ForTargeting { get { return false; } }
        public override bool ForTerrain { get { return false; } }
        public override int Precedence { get { return 40; } }
    }
}
