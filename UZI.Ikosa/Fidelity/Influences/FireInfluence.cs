using System;
using Uzi.Ikosa.Creatures.SubTypes;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Fidelity
{
    [Serializable]
    public class FireInfluence : ElementalInfluence<WaterSubType, FireSubType>
    {
        public FireInfluence(Devotion devotion, IPrimaryInfluenceClass influenceClass)
            : base(devotion, influenceClass)
        {
        }

        public override string Name { get { return @"Fire Influence"; } }
        public override object Clone() { return new FireInfluence(Devotion, InfluenceClass); }
    }
}
