using System;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Fidelity
{
    [Serializable]
    public class EvilInfluence : DescriptorInfluence<Evil>
    {
        public EvilInfluence(Devotion devotion, IPrimaryInfluenceClass influenceClass)
            : base(devotion, influenceClass, @"Evil Influence")
        {
        }

        public override string Name { get { return @"Evil Influence"; } }
        public override string Description { get { return @"+1 Caster Level for Evil Spells"; } }
        public override object Clone() { return new EvilInfluence(Devotion, InfluenceClass); }
    }
}