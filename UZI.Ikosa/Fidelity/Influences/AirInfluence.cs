using System;
using Uzi.Ikosa.Creatures.SubTypes;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Fidelity
{
    [Serializable]
    public class AirInfluence : ElementalInfluence<EarthSubType, AirSubType>
    {
        public AirInfluence(Devotion devotion, IPrimaryInfluenceClass influenceClass)
            : base(devotion, influenceClass)
        {
        }

        public override string Name { get { return @"Air Influence"; } }
        public override object Clone() { return new AirInfluence(Devotion, InfluenceClass); }
    }
}
