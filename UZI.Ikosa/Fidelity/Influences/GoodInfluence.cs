using System;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Fidelity
{
    [Serializable]
    public class GoodInfluence : DescriptorInfluence<Good>
    {
        public GoodInfluence(Devotion devotion, IPrimaryInfluenceClass influenceClass)
            : base(devotion, influenceClass, @"Good Influence")
        {
        }

        public override string Name { get { return @"Good Influence"; } }
        public override string Description { get { return @"+1 Caster Level for Good Spells"; } }
        public override object Clone() { return new GoodInfluence(Devotion, InfluenceClass); }
    }
}