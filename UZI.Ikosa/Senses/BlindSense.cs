using System;

namespace Uzi.Ikosa.Senses
{
    [Serializable]
    public class BlindSense : SensoryBase
    {
        public BlindSense(double range, object source)
            : base(source)
        {
            Range = range;
        }
        public override bool UsesLineOfEffect { get { return true; } }
        public override bool IgnoresInvisibility { get { return true; } }
        public override bool ForTerrain { get { return false; } }
        public override bool UsesSight { get { return true; } }
        public override int Precedence { get { return 50; } }
    }
}
