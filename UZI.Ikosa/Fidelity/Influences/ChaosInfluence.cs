using System;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Fidelity
{
    [Serializable]
    public class ChaosInfluence : DescriptorInfluence<Chaotic>
    {
        public ChaosInfluence(Devotion devotion, IPrimaryInfluenceClass influenceClass)
            : base(devotion, influenceClass, @"Chaos Influence")
        {
        }

        public override string Name { get { return @"Chaos Influence"; } }
        public override string Description { get { return @"+1 Caster Level for Chaotic Spells"; } }
        public override object Clone() { return new ChaosInfluence(Devotion, InfluenceClass); }
    }
}