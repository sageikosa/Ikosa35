using System;

namespace Uzi.Ikosa.Senses
{
    [Serializable]
    public class Scent : SensoryBase
    {
        public Scent(object source)
            : base(source)
        {
        }

        public override bool ForTargeting { get { return false; } }
        public override bool ForTerrain { get { return false; } }
        public override int Precedence { get { return 30; } }
    }
}