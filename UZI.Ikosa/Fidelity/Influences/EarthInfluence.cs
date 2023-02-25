using System;
using Uzi.Ikosa.Creatures.SubTypes;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Fidelity
{
    [Serializable]
    public class EarthInfluence : ElementalInfluence<AirSubType, EarthSubType>
    {
        public EarthInfluence(Devotion devotion, IPrimaryInfluenceClass influenceClass)
            : base(devotion, influenceClass)
        {
        }

        public override string Name { get { return @"Earth Influence"; } }
        public override object Clone() { return new EarthInfluence(Devotion, InfluenceClass); }
    }
}
