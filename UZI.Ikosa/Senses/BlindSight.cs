using System;

namespace Uzi.Ikosa.Senses
{
    [Serializable]
    public class BlindSight : SensoryBase
    {
        public BlindSight(double range, bool usesHearing, object source)
            : base(source)
        {
            Range = range;
        }

        public override int Precedence => 75;
        public override bool IgnoresInvisibility => true;
        public override bool UsesLineOfEffect => true;
        public override bool UsesHearing => true;
        public override bool IgnoresVisualEffects => true;
        public override bool IgnoresConcealment => true;
    }
}
