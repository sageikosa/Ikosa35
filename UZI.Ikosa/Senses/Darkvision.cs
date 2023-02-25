using System;

namespace Uzi.Ikosa.Senses
{
    [Serializable]
    public class Darkvision : SensoryBase
    {
        public Darkvision(double range, object source)
            : base(source)
        {
            Range = range;
        }

        public override bool UsesLineOfEffect { get { return true; } }
        public override bool UsesSight { get { return true; } }
        public override int Precedence { get { return 90; } }
    }
}
