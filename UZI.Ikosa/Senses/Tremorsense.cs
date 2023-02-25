using System;

namespace Uzi.Ikosa.Senses
{
    [Serializable]
    public class Tremorsense : SensoryBase
    {
        public Tremorsense(double range, object source)
            : base(source)
        {
            Range = range;
        }
        public override bool IgnoresInvisibility => true;
        public override int Precedence => 60;
        public override bool ForTerrain => false;
        public override bool IgnoresConcealment => true;
        public override bool IgnoresVisualEffects => true;
    }
}
