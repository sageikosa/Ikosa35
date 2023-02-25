using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Senses
{
    [Serializable]
    public class TrueSight : SensoryBase
    {
        public TrueSight(double range, object source)
            : base(source)
        {
            LowLight = true;
            Range = range;
        }

        public override int Precedence => 200;
        public override bool ForTargeting => true;
        public override bool ForTerrain => true;
        public override bool IgnoresInvisibility => true;
        public override bool IgnoresVisualEffects => true;
        public override bool IgnoresConcealment => false;
        public override bool UsesLineOfEffect => true;
        public override bool UsesSenseTransit => true;
        public override bool UsesSight => true;
        public override bool UsesLight => false;
        public override PlanarPresence PlanarPresence => PlanarPresence.Both;
    }
}
