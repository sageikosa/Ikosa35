using System;
using Uzi.Ikosa.Creatures.SubTypes;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Fidelity
{
    [Serializable]
    public class WaterInfluence : ElementalInfluence<FireSubType, WaterSubType>
    {
        public WaterInfluence(Devotion devotion, IPrimaryInfluenceClass influenceClass)
            : base(devotion, influenceClass)
        {
        }

        public override string Name { get { return @"Water Influence"; } }
        public override object Clone() { return new WaterInfluence(Devotion, InfluenceClass); }
    }
}
