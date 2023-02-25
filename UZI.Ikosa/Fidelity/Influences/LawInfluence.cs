using System;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Fidelity
{
    [Serializable]
    public class LawInfluence : DescriptorInfluence<Lawful>
    {
        public LawInfluence(Devotion devotion, IPrimaryInfluenceClass influenceClass)
            : base(devotion, influenceClass, @"Law Influence")
        {
        }

        public override string Name { get { return @"Law Influence"; } }
        public override string Description { get { return @"+1 Caster Level for Lawful Spells"; } }
        public override object Clone() { return new LawInfluence(Devotion, InfluenceClass); }
    }
}