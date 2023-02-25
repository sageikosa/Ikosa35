using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Senses
{
    [Serializable]
    public class EtherealVision : SensoryBase
    {
        public EtherealVision(double range, EtherealState ethereal)
            : base(ethereal)
        {
            Range = range;
        }

        public EtherealState Ethereal => Source as EtherealState;
        public override int Precedence => 95;
        public override bool UsesLineOfEffect => true;
        public override bool UsesSight => true;
        public override PlanarPresence PlanarPresence => PlanarPresence.Both;
    }
}
