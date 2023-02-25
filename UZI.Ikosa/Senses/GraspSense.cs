using System;

namespace Uzi.Ikosa.Senses
{
    [Serializable]
    public class GraspSense : SensoryBase
    {
        public GraspSense(object source)
            : base(source)
        {
            Range = 5d;
        }

        public override bool IgnoresInvisibility => true;
        public override int Precedence => 70;
        public override bool ForTerrain => false;
        public override bool IgnoresConcealment => true;
        public override bool IgnoresVisualEffects => true;
    }
}
